using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Monbetsu.Test
{
    class Recorder<TGraph, TNode, TEdge, TLabel>
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : class
    {
        public delegate string NameFactory(string testMethod, string patternId, int step);

        [Flags]
        public enum EdgeFlags
        {
            None = 0,
            Highlight = 3,
            Subhighlight = 2,
            Labeled = 4,

        }

        public class RecordOptions
        {
            public bool HideSubgraphEdges { get; set; }
            public bool HideLabels { get; set; }
            public bool HighlightLatests { get; set; }
            public bool HighlightBelongingEdges { get; set; }

            public NameFactory? NameFactory { get; set; }

            public Action<Dot.RootGraph>? OptionConfigurer { get; set; }

            public Action<Dot.Edge, NodePair<TEdge>, TLabel?, EdgeFlags, RecordOptions>? EdgeDecorator { get; set; }
            public Action<Dot.Edge, NodePair<TLabel>, TLabel, EdgeFlags, RecordOptions>? SeriesSubgraphDecorator { get; set; }
            public Action<Dot.Edge, NodePair<UnorderedNTuple<TLabel>>, TLabel, EdgeFlags, RecordOptions>? ParallelSubgraphDecorator { get; set; }
            public Action<Dot.Edge, NodePair<UnorderedNTuple<TLabel>>, TLabel, EdgeFlags, RecordOptions>? KnotSubgraphDecorator { get; set; }

            public bool FitToLast { get; set; }

            public bool OmitProgress { get; set; }
            public ImplementationVersions Versions { get; set; } = ImplementationVersions.Latest;

        }

        public class NodePair<TVia> : IEquatable<NodePair<TVia>?>
        {
            public TNode StartNode { get; }
            public TVia Via { get; }
            public TNode EndNode { get; }

            public NodePair(TNode startNode, TVia via, TNode endNode)
            {
                StartNode = startNode;
                Via = via;
                EndNode = endNode;
            }

            public void Deconstruct(out TNode startNode, out TVia via, out TNode endNode)
            {
                startNode = StartNode;
                via = Via;
                endNode = EndNode;
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as Recorder<TGraph, TNode, TEdge, TLabel>.NodePair<TVia>);
            }

            public bool Equals(Recorder<TGraph, TNode, TEdge, TLabel>.NodePair<TVia>? other)
            {
                return other != null &&
                       EqualityComparer<TNode>.Default.Equals(StartNode, other.StartNode) &&
                       EqualityComparer<TVia>.Default.Equals(Via, other.Via) &&
                       EqualityComparer<TNode>.Default.Equals(EndNode, other.EndNode);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(StartNode, Via, EndNode);
            }
        }

        private RecordOptions options;
        private readonly string outputDirPath;
        private readonly Func<TNode, string> nodeToIdFunc;

        private Dictionary<NodePair<TEdge>, TLabel> edgeResults = new Dictionary<NodePair<TEdge>, TLabel>();
        private Dictionary<NodePair<TLabel>, TLabel> serResults = new Dictionary<NodePair<TLabel>, TLabel>();
        private Dictionary<NodePair<UnorderedNTuple<TLabel>>, TLabel> parResults = new Dictionary<NodePair<UnorderedNTuple<TLabel>>, TLabel>();
        private Dictionary<NodePair<UnorderedNTuple<TLabel>>, TLabel> knotResults = new Dictionary<NodePair<UnorderedNTuple<TLabel>>, TLabel>();
        private Dictionary<TLabel, TLabel> labelParents = new Dictionary<TLabel, TLabel>();
        private List<object>? highlights = null;

        private List<(
                    List<NodePair<TEdge>> edgeKeys,
                    List<NodePair<TLabel>> serKeys,
                    List<NodePair<UnorderedNTuple<TLabel>>> parKeys,
                    List<NodePair<UnorderedNTuple<TLabel>>> knotKeys,
                    List<object>? highlights
                    )> incrementalResults = new List<(
                    List<NodePair<TEdge>> edgeKeys,
                    List<NodePair<TLabel>> serKeys,
                    List<NodePair<UnorderedNTuple<TLabel>>> parKeys,
                    List<NodePair<UnorderedNTuple<TLabel>>> knotKeys,
                    List<object>? highlights
                    )>();


        private string runId = "";
        private List<NodePair<TEdge>> graphEdges = new List<NodePair<TEdge>>();
        private int stepCounter = 0;

        private readonly List<string> outputImagePaths = new List<string>();

        public Recorder(Func<TNode, string> nodeToIdFunc, RecordOptions options, string outputDirPath)
        {
            this.nodeToIdFunc = nodeToIdFunc;
            this.options = options;
            this.outputDirPath = outputDirPath;
        }


        private void SaveStep()
        {
            if (options.OmitProgress && incrementalResults.Count > 1)
            {
                incrementalResults.RemoveAt(1);
            }

            incrementalResults.Add(
                (
                edgeResults.Keys.ToList(),
                serResults.Keys.ToList(),
                parResults.Keys.ToList(),
                knotResults.Keys.ToList(),
                highlights?.ToList()
                ));
        }

        private void GenerateStep(
                    IEnumerable<NodePair<TEdge>>? edgeKeys = null,
                    IEnumerable<NodePair<TLabel>>? serKeys = null,
                    IEnumerable<NodePair<UnorderedNTuple<TLabel>>>? parKeys = null,
                    IEnumerable<NodePair<UnorderedNTuple<TLabel>>>? knotKeys = null,
                    IEnumerable<object>? highlights = null
                    )
        {
            var rootGraph = new Dot.Digraph { Id = TestContext.CurrentContext.Test.MethodName };
            var subgraphIdCounter = 0;
            var subgraphs = labelParents.Values.Distinct().ToDictionary(
                l => l,
                l => new Dot.Cluster
                {
                    Id = $"S{subgraphIdCounter++}",
                    Style = Dot.ClusterStyle.invis
                } as Dot.SubgraphBase);
            var parentSubgraphs = new Dictionary<TLabel, Dot.GraphBase>();
            var subgraphParents = new Dictionary<Dot.GraphBase, Dot.GraphBase>();
            foreach (var g in labelParents.ToLookup(kv => kv.Value, kv => kv.Key))
            {
                foreach (var g2 in g)
                {
                    if (subgraphs.TryGetValue(g2, out var child))
                    {
                        subgraphs[g.Key].Subgraphs.Add(child);
                        subgraphParents[child] = subgraphs[g.Key];
                    }
                    parentSubgraphs[g2] = subgraphs[g.Key];
                }
            }
            foreach (var aa in labelParents.Values.Where(l => !labelParents.ContainsKey(l)).Distinct())
            {
                rootGraph.Subgraphs.Add(subgraphs[aa]);
                parentSubgraphs[aa] = rootGraph;
                subgraphParents[subgraphs[aa]] = rootGraph;
            }
            foreach (var aa in edgeResults.Values.Distinct())
            {
                if (labelParents.TryGetValue(aa, out var p))
                {
                    parentSubgraphs[aa] = subgraphs[p];
                }
                else
                {
                    parentSubgraphs[aa] = rootGraph;
                }
            }
            subgraphParents[rootGraph] = rootGraph;

            bool isSubhighlight(TLabel label)
            {
                if (highlights != null && options.HighlightBelongingEdges)
                {
                    return highlights.Select(h =>
                    {
                        TLabel? ancestorLabel = null;
                        switch (h)
                        {
                            case NodePair<TLabel> h0:
                                if (serKeys?.Contains(h0) == true)
                                {
                                    serResults?.TryGetValue(h0, out ancestorLabel);
                                }
                                break;
                            case NodePair<UnorderedNTuple<TLabel>> h1:
                                if (parKeys?.Contains(h1) == true)
                                {
                                    parResults?.TryGetValue(h1, out ancestorLabel);
                                }
                                else if (knotKeys?.Contains(h1) == true)
                                {
                                    knotResults?.TryGetValue(h1, out ancestorLabel);
                                }
                                break;
                        };

                        if (ancestorLabel != null)
                        {
                            var p = label;
                            while (labelParents.TryGetValue(p, out var p2))
                            {
                                if (ancestorLabel == p2)
                                {
                                    return true;
                                }
                                p = p2;
                            }

                        }
                        return false;
                    }).Any(_ => _);
                }
                return false;
            }


            foreach (var edge in graphEdges)
            {
                Dot.IColor color = Dot.NamedColor.Black;
                var edgeFlags = EdgeFlags.None;

                if (edgeResults.TryGetValue(edge, out var label) && parentSubgraphs.TryGetValue(label, out var graph))
                {
                    if (highlights != null)
                    {
                        if (highlights.Contains(edge))
                        {
                            edgeFlags |= EdgeFlags.Highlight;
                        }
                        else if (isSubhighlight(label))
                        {
                            edgeFlags |= EdgeFlags.Subhighlight;
                        }
                        else
                        {
                            color = new Dot.RgbaColor(0, 0, 0, 64);
                        }
                    }

                    var fontColor = color;
                    if (options.HideLabels)
                    {
                        fontColor = Dot.NamedColor.Transparent;
                    }

                    switch (edgeKeys?.Contains(edge))
                    {
                        case false:
                            fontColor = Dot.NamedColor.Transparent;
                            break;
                        case true:
                            edgeFlags |= EdgeFlags.Labeled;
                            break;
                    }

                    var dotEdge = new Dot.Edge
                    {
                        From = nodeToIdFunc(edge.StartNode),
                        To = nodeToIdFunc(edge.EndNode),
                        //Label = label.LabelText,
                        Color = color,
                        FontColor = fontColor,
                    };
                    options.EdgeDecorator?.Invoke(dotEdge, edge, label, edgeFlags, options);
                    graph.Edges.Add(dotEdge);
                }
                else
                {
                    if (highlights?.Contains(edge) == false)
                    {
                        color = new Dot.RgbaColor(0, 0, 0, 64);
                    }

                    var dotEdge = new Dot.Edge
                    {
                        From = nodeToIdFunc(edge.StartNode),
                        To = nodeToIdFunc(edge.EndNode),
                        Color = color
                    };
                    options.EdgeDecorator?.Invoke(dotEdge, edge, null, edgeFlags, options);
                    rootGraph.Edges.Add(dotEdge);
                }
            }


            foreach (var kv in knotResults)
            {
                var (fromNode, sublabels, toNode) = kv.Key;
                var graph = subgraphs[kv.Value];

                var edgeFlags = EdgeFlags.None;
                Dot.IColor color = Dot.NamedColor.Blue;

                switch(highlights?.Contains(kv.Key))
                {
                    case false:
                        color = new Dot.RgbaColor(0, 0, 255, 64);
                        break;
                    case true:
                        edgeFlags |= EdgeFlags.Highlight;
                        break;
                }

                if (options.HideSubgraphEdges)
                {
                    color = Dot.NamedColor.Transparent;
                }
                
                switch(knotKeys?.Contains(kv.Key))
                {
                    case false:
                        color = Dot.NamedColor.Transparent;
                        break;
                    case true:
                        edgeFlags |= EdgeFlags.Labeled;
                        break;
                }

                var fontColor = color;
                if (options.HideLabels)
                {
                    fontColor = Dot.NamedColor.Transparent;
                }

                var dotEdge = new Dot.Edge
                {
                    From = nodeToIdFunc(fromNode),
                    To = nodeToIdFunc(toNode),
                    Color = color,
                    FontColor = fontColor,
                    Style = Dot.EdgeStyle.dashed,
                };

                options.KnotSubgraphDecorator?.Invoke(dotEdge, kv.Key, kv.Value, edgeFlags, options);

                var (edges, points) = dotEdge.SplitByInvisibleRelay(2);

                edges[0].Label = "";
                edges[2].Label = "";

                subgraphParents[graph].Edges.Add(edges[0]);
                graph.Edges.Add(edges[1]);
                subgraphParents[graph].Edges.Add(edges[2]);
                graph.Nodes.AddRange(points);
            }

            foreach (var kv in serResults)
            {
                var (fromNode, sub, toNode) = kv.Key;
                var graph = subgraphs[kv.Value];

                var edgeFlags = EdgeFlags.None;
                Dot.IColor color = new Dot.RgbaColor(0, 200, 0, 255);

                switch (highlights?.Contains(kv.Key))
                {
                    case false:
                        color = new Dot.RgbaColor(0, 200, 0, 64);
                        break;
                    case true:
                        edgeFlags |= EdgeFlags.Highlight;
                        break;
                }

                if (options.HideSubgraphEdges)
                {
                    color = Dot.NamedColor.Transparent;
                }
                switch (serKeys?.Contains(kv.Key))
                {
                    case false:
                        color = Dot.NamedColor.Transparent;
                        break;
                    case true:
                        edgeFlags |= EdgeFlags.Labeled;
                        break;
                }

                var fontColor = color;
                if (options.HideLabels)
                {
                    fontColor = Dot.NamedColor.Transparent;
                }

                var dotEdge = new Dot.Edge
                {
                    From = nodeToIdFunc(fromNode),
                    To = nodeToIdFunc(toNode),
                    Color = color,
                    FontColor = fontColor,
                    Style = Dot.EdgeStyle.dashed,
                };

                options.SeriesSubgraphDecorator?.Invoke(dotEdge, kv.Key, kv.Value, edgeFlags, options);

                var (edges, points) = dotEdge.SplitByInvisibleRelay(2);
                edges[0].Label = "";
                edges[2].Label = "";

                subgraphParents[graph].Edges.Add(edges[0]);
                graph.Edges.Add(edges[1]);
                subgraphParents[graph].Edges.Add(edges[2]);
                graph.Nodes.AddRange(points);
            }

            foreach (var kv in parResults)
            {
                var (fromNode, sublabels, toNode) = kv.Key;
                var graph = subgraphs[kv.Value];

                var edgeFlags = EdgeFlags.None;
                Dot.IColor color = new Dot.RgbaColor(255, 0, 255, 255);

                switch (highlights?.Contains(kv.Key))
                {
                    case false:
                        color = new Dot.RgbaColor(255, 0, 255, 64);
                    break;
                    case true:
                        edgeFlags |= EdgeFlags.Highlight;
                    break;
                }

                if (options.HideSubgraphEdges)
                {
                    color = Dot.NamedColor.Transparent;
                }
                switch (parKeys?.Contains(kv.Key))
                {
                    case false:
                        color = Dot.NamedColor.Transparent;
                    break;
                    case true:
                        edgeFlags |= EdgeFlags.Labeled;
                    break;
                }

                var fontColor = color;
                if (options.HideLabels)
                {
                    fontColor = Dot.NamedColor.Transparent;
                }

                var dotEdge = new Dot.Edge
                {
                    From = nodeToIdFunc(fromNode),
                    To = nodeToIdFunc(toNode),
                    Color = color,
                    FontColor = fontColor,
                    Style = Dot.EdgeStyle.dashed,
                };

                options.ParallelSubgraphDecorator?.Invoke(dotEdge, kv.Key, kv.Value, edgeFlags, options);

                var (edges, points) = dotEdge.SplitByInvisibleRelay(2);
                edges[0].Label = "";
                edges[2].Label = "";
                
                subgraphParents[graph].Edges.Add(edges[0]);
                graph.Edges.Add(edges[1]);
                subgraphParents[graph].Edges.Add(edges[2]);
                graph.Nodes.AddRange(points);
            }

            options.OptionConfigurer?.Invoke(rootGraph);

            var index = stepCounter++;
            var outputPath = Path.Combine(outputDirPath, (options.NameFactory?.Invoke(TestContext.CurrentContext.Test.Name, runId, index) ?? $"step-{index:D4}") + ".png");


            Visualizer.GenerateImageFromDot(rootGraph.Build(), outputPath);
            if (File.Exists(outputPath))
            {
                outputImagePaths.Add(outputPath);
            }
        }

        internal IEnumerable<string> Record(string id, IEnumerable<(TNode fromNode, TEdge edge, TNode toNode)> edges, Action<EdgeLabeler<TGraph, TNode, TEdge, TLabel>, SeriesSubgraphLabeler<TGraph, TNode, TLabel>, ParallelSubgraphLabeler<TGraph, TNode, TLabel>, KnotSubgraphLabeler<TGraph, TNode, TLabel>> run)
        {
            runId = id;
            graphEdges = edges.Select(t => new NodePair<TEdge>(t.fromNode, t.edge, t.toNode)).ToList();


            edgeResults.Clear();
            serResults.Clear();
            parResults.Clear();
            knotResults.Clear();
            labelParents.Clear();
            highlights?.Clear();

            incrementalResults.Clear();


            highlights = options.HighlightLatests ? new List<object>() : null;
            
            stepCounter = 0;


            Step(null, null);

            run(LabelEdge, LabelSeries, LabelParallel, LabelKnot);

            if (options.FitToLast)
            {
                foreach (var step in incrementalResults)
                {
                    GenerateStep(step.edgeKeys, step.serKeys, step.parKeys, step.knotKeys, step.highlights);
                }
            }

            return outputImagePaths;
        }

        private void Step(object? highlight, TLabel? highlightLabel)
        {
            highlights?.Clear();

            if (highlight != null)
            {
                highlights?.Add(highlight);
            }
            if (highlightLabel != null)
            {
                highlights?.Add(highlightLabel);
            }

            if (options.FitToLast)
            {
                SaveStep();
            }
            else
            {
                GenerateStep(highlights: highlights);
            }
        }

        public void LabelEdge(TGraph _, TNode fromNode, TEdge edge, TNode toNode, TLabel label)
        {
            var pair = new NodePair<TEdge>(fromNode, edge, toNode);
            edgeResults[pair] = label;
            Step(pair, label);
        }
        
        public void LabelSeries(TGraph _, TNode startNode, TLabel sublabel, TNode endNode, TLabel label)
        {
            var pair = new NodePair<TLabel>(startNode, sublabel, endNode);
            serResults[pair] = label;

            labelParents[sublabel] = label;

            Step(pair, label);
        }
        
        public void LabelParallel(TGraph _, TNode startNode, IEnumerable<TLabel> sublabels, TNode endNode, TLabel label)
        {
            var pair = new NodePair<UnorderedNTuple<TLabel>>(startNode, new UnorderedNTuple<TLabel>(sublabels), endNode);
            parResults[pair] = label;

            foreach (var sublabel in sublabels)
            {
                labelParents[sublabel] = label;
            }

            Step(pair, label);
        }
        public void LabelKnot(TGraph _, TNode startNode, IEnumerable<TLabel> sublabels, TNode endNode, TLabel label)
        {
            var pair = new NodePair<UnorderedNTuple<TLabel>>(startNode, new UnorderedNTuple<TLabel>(sublabels), endNode);
            knotResults.Add(pair, label);

            foreach (var sublabel in sublabels)
            {
                labelParents[sublabel] = label;
            }

            Step(pair, label);
        }
    }
}
