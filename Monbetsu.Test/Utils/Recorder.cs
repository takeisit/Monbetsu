using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using FluentAssertions;
using FluentAssertions.Execution;
using Monbetsu.Test.Utils;
using NUnit.Framework;

namespace Monbetsu.Test
{
    [Flags]
    public enum SnapshotFlags
    {
        None = 0,
        Initial = 1,
        Intermediates = 2,
        Last = 4,
        Assert = 8,
    }

    [Flags]
    public enum EdgeStatus
    {
        None = 0,
        Highlight = 3,
        Subhighlight = 2,
        Labeled = 4,
    }

    internal interface IAssertionResult
    {
        string ToMessage();
    }

    class AssertionFailedException : AssertionException
    {
        private static string MakeMessage(IReadOnlyList<IAssertionResult> results)
        {
            return string.Join("\n", results.Select(r => r.ToMessage()));
        }

        public IReadOnlyList<IAssertionResult> Results { get; }

        public AssertionFailedException(IReadOnlyList<IAssertionResult> results) : base(MakeMessage(results))
        {
            Results = results;
        }

        public void ShouldAllOf<TAssertionResult, TKey>(Func<TAssertionResult, TKey> selector, params (TKey key, Action<TAssertionResult> asserter)[] actions)
            where TAssertionResult: IAssertionResult
        {
            var actualDic = Results.OfType<TAssertionResult>().ToDictionary(item => selector(item), item => item);

            actualDic.Should().HaveSameCount(actions);
            using (new AssertionScope())
            {
                var expectedDic = actions.ToDictionary(a => a.key, a => a.asserter);

                foreach (var kv in actualDic)
                {
                    using (var scope = new AssertionScope($"item {kv.Key}:"))
                    {
                        if (expectedDic.TryGetValue(kv.Key, out var action))
                        {
                            action(kv.Value);
                        }
                        else
                        {
                            scope.FailWith($"no assertion was found for {kv.Key}.");
                        }
                    }
                }
            }
        }
    }

    partial class Recorder<TGraph, TNode, TEdge, TLabel>
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : class
    {
        public delegate string NameFactory(string testMethod, string patternId, int step);

        public class RecordOptions
        {
            public bool HideSubgraphEdges { get; set; }
            public bool HideLabels { get; set; }
            public bool HighlightLatests { get; set; }
            public bool HighlightBelongingEdges { get; set; }

            public NameFactory? NameFactory { get; set; }

            public Action<Dot.RootGraph>? OptionConfigurer { get; set; }

            public Action<Dot.Edge, NodePair<TNode, TEdge>, TLabel?, EdgeStatus, RecordOptions>? EdgeDecorator { get; set; }
            public Action<Dot.Edge, NodePair<TNode, UnorderedNTuple<TLabel>>, TLabel, EdgeStatus, RecordOptions>? SeriesSubgraphDecorator { get; set; }
            public Action<Dot.Edge, NodePair<TNode, UnorderedNTuple<TLabel>>, TLabel, EdgeStatus, RecordOptions>? ParallelSubgraphDecorator { get; set; }
            public Action<Dot.Edge, NodePair<TNode, UnorderedNTuple<TLabel>>, TLabel, EdgeStatus, RecordOptions>? KnotSubgraphDecorator { get; set; }

            public bool FitToLast { get; set; }
            public SnapshotFlags SnapshotFlags { get; set; } = SnapshotFlags.Initial | SnapshotFlags.Intermediates | SnapshotFlags.Last;
            public ImplementationVersions Versions { get; set; } = ImplementationVersions.Latest;

        }

        [DebuggerDisplay("{Kind}: '{Label}'({Key})")]
        public class LabelKey : IEquatable<LabelKey?>
        {
            public TLabel Label { get; }
            public int Key { get; }
            public GroupKind Kind { get; }

            public LabelKey(TLabel label, int key, GroupKind kind)
            {
                Label = label ?? throw new ArgumentNullException(nameof(label));
                Key = key;
                Kind = kind;
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as LabelKey);
            }

            public bool Equals(LabelKey? other)
            {
                return other != null &&
                       EqualityComparer<TLabel>.Default.Equals(Label, other.Label) &&
                       Key == other.Key &&
                       Kind == other.Kind;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Label, Key, Kind);
            }
        }

        class StepLog
        {
            public int Step { get; }
            public List<NodePair<TNode, TEdge>> EdgeKeys { get; }
            public List<NodePair<TNode, UnorderedNTuple<TLabel>>> SerKeys { get; }
            public List<NodePair<TNode, UnorderedNTuple<TLabel>>> ParKeys { get; }
            public List<NodePair<TNode, UnorderedNTuple<TLabel>>> KnotKeys { get; }
            public List<object>? Highlights { get; }

            public string? GeneratedPath { get; set; }

            public StepLog(int step, List<NodePair<TNode, TEdge>> edgeKeys, List<NodePair<TNode, UnorderedNTuple<TLabel>>> serKeys, List<NodePair<TNode, UnorderedNTuple<TLabel>>> parKeys, List<NodePair<TNode, UnorderedNTuple<TLabel>>> knotKeys, List<object>? highlights)
            {
                Step = step;
                EdgeKeys = edgeKeys ?? throw new ArgumentNullException(nameof(edgeKeys));
                SerKeys = serKeys ?? throw new ArgumentNullException(nameof(serKeys));
                ParKeys = parKeys ?? throw new ArgumentNullException(nameof(parKeys));
                KnotKeys = knotKeys ?? throw new ArgumentNullException(nameof(knotKeys));
                Highlights = highlights;
            }
        }

        public IReadOnlyDictionary<NodePair<TNode, TEdge>, LabelKey> EdgeResults => edgeResults;
        public IReadOnlyDictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey> SerResults => serResults;
        public IReadOnlyDictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey> ParResults => parResults;
        public IReadOnlyDictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey> KnotResults => knotResults;
        public IReadOnlyDictionary<LabelKey, LabelKey> LabelParents => labelParents;
        public ILookup<LabelKey, LabelKey> LabelChildren => labelParents.ToLookup(kv => kv.Value, kv => kv.Key);

        private RecordOptions options;
        private readonly string outputDirPath;
        private readonly Func<TNode, string> nodeToIdFunc;

        private readonly Dictionary<NodePair<TNode, TEdge>, LabelKey> edgeResults = new Dictionary<NodePair<TNode, TEdge>, LabelKey>();
        private readonly Dictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey> serResults = new Dictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey>();
        private readonly Dictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey> parResults = new Dictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey>();
        private readonly Dictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey> knotResults = new Dictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey>();
        private readonly Dictionary<LabelKey, LabelKey> labelParents = new Dictionary<LabelKey, LabelKey>();
        private readonly Dictionary<TLabel, List<LabelKey>> topLabels = new Dictionary<TLabel, List<LabelKey>>();
        private List<object>? highlights = null;

        private readonly LinkedList<StepLog> resultLogs = new LinkedList<StepLog>();

        private string runId = "";
        private List<NodePair<TNode, TEdge>> graphEdges = new List<NodePair<TNode, TEdge>>();
        private int generatedStepCounter = 0;
        private int stepCounter = 0;
        private int labelKeyCounter = 0;

        private readonly List<string> outputImagePaths = new List<string>();

        public Recorder(Func<TNode, string> nodeToIdFunc, RecordOptions options, string outputDirPath)
        {
            this.nodeToIdFunc = nodeToIdFunc;
            this.options = options;
            this.outputDirPath = outputDirPath;
        }

        private void Step(object? highlight, LabelKey? highlightLabel)
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

            resultLogs.AddLast(new StepLog(
                stepCounter,
                edgeResults.Keys.ToList(),
                serResults.Keys.ToList(),
                parResults.Keys.ToList(),
                knotResults.Keys.ToList(),
                highlights?.ToList()
                ));

            if (!options.FitToLast)
            {
                var step = resultLogs.First();
                if (step.Step == 0)
                {
                    if (options.SnapshotFlags.HasFlag(SnapshotFlags.Initial))
                    {
                        GenerateStep(step.EdgeKeys, step.SerKeys, step.ParKeys, step.KnotKeys, step.Highlights);
                        resultLogs.RemoveFirst();
                    }
                }
                else 
                {
                    if (resultLogs.Count > 1)
                    {
                        if (options.SnapshotFlags.HasFlag(SnapshotFlags.Intermediates))
                        {
                            GenerateStep(step.EdgeKeys, step.SerKeys, step.ParKeys, step.KnotKeys, step.Highlights);
                        }
                        resultLogs.RemoveFirst();
                    }
                }
            }
            stepCounter++;
        }

        private string? GenerateStep(
                    IEnumerable<NodePair<TNode, TEdge>>? edgeKeys = null,
                    IEnumerable<NodePair<TNode, UnorderedNTuple<TLabel>>>? serKeys = null,
                    IEnumerable<NodePair<TNode, UnorderedNTuple<TLabel>>>? parKeys = null,
                    IEnumerable<NodePair<TNode, UnorderedNTuple<TLabel>>>? knotKeys = null,
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
            var parentSubgraphs = new Dictionary<LabelKey, Dot.GraphBase>();
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

            bool isSubhighlight(LabelKey label)
            {
                if (highlights != null && options.HighlightBelongingEdges)
                {
                    return highlights.Select(h =>
                    {
                        LabelKey? ancestorLabel = null;
                        switch (h)
                        {
                            case NodePair<TNode, UnorderedNTuple<TLabel>> h1:
                                if (serKeys?.Contains(h1) == true)
                                {
                                    serResults?.TryGetValue(h1, out ancestorLabel);
                                }
                                else if (parKeys?.Contains(h1) == true)
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
                var color = Visualizer.EdgeColor;
                var edgeFlags = EdgeStatus.None;

                if (edgeResults.TryGetValue(edge, out var label) && parentSubgraphs.TryGetValue(label, out var graph))
                {
                    if (highlights != null)
                    {
                        if (highlights.Contains(edge))
                        {
                            edgeFlags |= EdgeStatus.Highlight;
                        }
                        else if (isSubhighlight(label))
                        {
                            edgeFlags |= EdgeStatus.Subhighlight;
                        }
                        else
                        {
                            color = color.WithAlpha(64);
                        }
                    }

                    var fontColor = color;
                    if (options.HideLabels)
                    {
                        fontColor = fontColor.WithAlpha(0);
                    }

                    switch (edgeKeys?.Contains(edge))
                    {
                        case false:
                            fontColor = fontColor.WithAlpha(0);
                            break;
                        case true:
                            edgeFlags |= EdgeStatus.Labeled;
                            break;
                    }

                    var dotEdge = new Dot.Edge
                    {
                        From = nodeToIdFunc(edge.StartNode),
                        To = nodeToIdFunc(edge.EndNode),
                        Color = color,
                        FontColor = fontColor,
                    };
                    options.EdgeDecorator?.Invoke(dotEdge, edge, label.Label, edgeFlags, options);
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

            void MakeSubgraphEdges(
                Dictionary<NodePair<TNode, UnorderedNTuple<TLabel>>, LabelKey> result,
                IEnumerable<NodePair<TNode, UnorderedNTuple<TLabel>>>? progress,
                Dot.RgbaColor color,
                Action<Dot.Edge, NodePair<TNode, UnorderedNTuple<TLabel>>, TLabel, EdgeStatus, RecordOptions>? decorator
                )
            {
                foreach (var kv in result)
                {
                    var (fromNode, sublabels, toNode) = kv.Key;
                    var graph = subgraphs[kv.Value];

                    var edgeFlags = EdgeStatus.None;
                    
                    if (highlights != null)
                    {
                        if (highlights.Contains(kv.Key))
                        {
                            edgeFlags |= EdgeStatus.Highlight;
                        }
                        else if (isSubhighlight(kv.Value))
                        {
                            edgeFlags |= EdgeStatus.Subhighlight;
                            color = color.WithAlpha(128);
                        }
                        else
                        {
                            color = color.WithAlpha(64);
                        }
                    }

                    if (options.HideSubgraphEdges)
                    {
                        color = color.WithAlpha(0);
                    }

                    switch (progress?.Contains(kv.Key))
                    {
                        case false:
                            color = color.WithAlpha(0);
                            break;
                        case true:
                            edgeFlags |= EdgeStatus.Labeled;
                            break;
                    }

                    var fontColor = color;
                    if (options.HideLabels)
                    {
                        fontColor = fontColor.WithAlpha(0);
                    }

                    var dotEdge = new Dot.Edge
                    {
                        From = nodeToIdFunc(fromNode),
                        To = nodeToIdFunc(toNode),
                        Color = color,
                        FontColor = fontColor,
                        Style = Dot.EdgeStyle.dashed,
                    };

                    decorator?.Invoke(dotEdge, kv.Key, kv.Value.Label, edgeFlags, options);

                    var (edges, points) = dotEdge.SplitByInvisibleRelay(2);

                    edges[0].Label = "";
                    edges[2].Label = "";

                    subgraphParents[graph].Edges.Add(edges[0]);
                    graph.Edges.Add(edges[1]);
                    subgraphParents[graph].Edges.Add(edges[2]);
                    graph.Nodes.AddRange(points);
                }
            }

            MakeSubgraphEdges(serResults, serKeys, Visualizer.SeriesColor, options.SeriesSubgraphDecorator);
            MakeSubgraphEdges(parResults, parKeys, Visualizer.ParallelColor, options.ParallelSubgraphDecorator);
            MakeSubgraphEdges(knotResults, knotKeys, Visualizer.KnotColor, options.KnotSubgraphDecorator);

            options.OptionConfigurer?.Invoke(rootGraph);

            var index = generatedStepCounter++;
            var outputPath = Path.Combine(outputDirPath, (options.NameFactory?.Invoke(TestContext.CurrentContext.Test.Name, runId, index) ?? $"{TestContext.CurrentContext.Test.Name}_{runId}_step-{index:D4}") + ".png");


            Visualizer.GenerateImageFromDot(rootGraph.Build(), outputPath);
            if (File.Exists(outputPath))
            {
                outputImagePaths.Add(outputPath);
                return outputPath;
            }

            return null;
        }

        internal IEnumerable<string> Record(
            string id,
            IEnumerable<NodePair<TNode, TEdge>> edges,
            Action<Recorder<TGraph, TNode, TEdge, TLabel>> run,
            Func<Recorder<TGraph, TNode, TEdge, TLabel>, IEnumerable<IAssertionResult>?>? assert = null
            )
        {
            runId = id;
            graphEdges = edges.Select(t => new NodePair<TNode, TEdge>(t.StartNode, t.Via, t.EndNode)).ToList();


            edgeResults.Clear();
            serResults.Clear();
            parResults.Clear();
            knotResults.Clear();
            labelParents.Clear();
            topLabels.Clear();
            highlights?.Clear();

            resultLogs.Clear();


            highlights = options.HighlightLatests ? new List<object>() : null;

            labelKeyCounter = 0;
            stepCounter = 0;
            generatedStepCounter = 0;

            Step(null, null);

            Exception? exception = null;

            try
            {
                run(this);
            }
            catch(Exception e)
            {
                exception = e;
            }

            if (options.FitToLast)
            {
                foreach (var step in resultLogs)
                {
                    var isGeneratable = options.SnapshotFlags.HasFlag(SnapshotFlags.Initial) && step == resultLogs.First()
                                    || options.SnapshotFlags.HasFlag(SnapshotFlags.Last) && step == resultLogs.Last()
                                    || options.SnapshotFlags.HasFlag(SnapshotFlags.Intermediates);

                    if (isGeneratable)
                    {
                        GenerateStep(step.EdgeKeys, step.SerKeys, step.ParKeys, step.KnotKeys, step.Highlights);
                    }
                }
            }
            else
            {
                if (options.SnapshotFlags.HasFlag(SnapshotFlags.Last))
                {
                    if (resultLogs.Count > 0)
                    {
                        var step = resultLogs.Last();
                        GenerateStep(step.EdgeKeys, step.SerKeys, step.ParKeys, step.KnotKeys, step.Highlights);
                    }
                }
            }

            if (exception != null)
            {
                throw new Exception("exception occured in runnning.", exception);
            }

            if (assert != null)
            {
                var assertions = assert(this);

                if (options.SnapshotFlags.HasFlag(SnapshotFlags.Assert))
                {
                    GenerateStep(edgeResults.Keys, serResults.Keys, parResults.Keys, knotResults.Keys, null);
                }

                if (assertions?.Any() == true)
                {
                    throw new AssertionFailedException(assertions.ToList());
                }
            }

            return outputImagePaths;
        }


        public void LabelEdge(TGraph _, TNode fromNode, TEdge edge, TNode toNode, TLabel label)
        {
            var pair = new NodePair<TNode, TEdge>(fromNode, edge, toNode);
            var key = MakeLabelKey(label, GroupKind.Edge, Enumerable.Empty<TLabel>());
            edgeResults[pair] = key;
            Step(pair, key);
        }
        
        public void LabelSeries(TGraph _, TNode startNode, TLabel sublabel, TNode endNode, TLabel label)
        {
            var pair = new NodePair<TNode, UnorderedNTuple<TLabel>>(startNode, new UnorderedNTuple<TLabel>(sublabel), endNode);
            var key = MakeLabelKey(label, GroupKind.Series, new[] { sublabel });
            serResults[pair] = key;
            Step(pair, key);
        }

        public void LabelSeries(TGraph _, TNode startNode, IEnumerable<TLabel> sublabels, TNode endNode, TLabel label)
        {
            var pair = new NodePair<TNode, UnorderedNTuple<TLabel>>(startNode, new UnorderedNTuple<TLabel>(sublabels), endNode);
            var key = MakeLabelKey(label, GroupKind.Series, sublabels);
            serResults[pair] = key;
            Step(pair, key);
        }

        public void LabelParallel(TGraph _, TNode startNode, IEnumerable<TLabel> sublabels, TNode endNode, TLabel label)
        {
            var pair = new NodePair<TNode, UnorderedNTuple<TLabel>>(startNode, new UnorderedNTuple<TLabel>(sublabels), endNode);
            var key = MakeLabelKey(label, GroupKind.Parallel, sublabels);
            parResults[pair] = key;
            Step(pair, key);
        }

        public void LabelKnot(TGraph _, TNode startNode, IEnumerable<TLabel> sublabels, TNode endNode, TLabel label)
        {
            var pair = new NodePair<TNode, UnorderedNTuple<TLabel>>(startNode, new UnorderedNTuple<TLabel>(sublabels), endNode);
            var key = MakeLabelKey(label, GroupKind.Knot, sublabels);
            knotResults[pair] = key;
            Step(pair, key);
        }

        private LabelKey MakeLabelKey(TLabel label, GroupKind kind, IEnumerable<TLabel> sublabels)
        {
            var id = labelKeyCounter++;
            var key = new LabelKey(label, id, kind);

            IEnumerable<LabelKey> ResolveSub(TLabel sublabel)
            {
                if (!topLabels.TryGetValue(sublabel, out var list))
                {
                    topLabels[sublabel] = list = new List<LabelKey> { new LabelKey(sublabel, labelKeyCounter++, GroupKind.Unknown) };
                }
                return list;
            }

            foreach (var sublabelKey in sublabels.SelectMany(ResolveSub))
            {
                labelParents[sublabelKey] = key;
            }

            if (!topLabels.TryGetValue(label, out var list))
            {
                topLabels[label] = list = new List<LabelKey>();
            }

            if (kind == GroupKind.Series)
            {
                list.Clear();
            }

            list.Add(key);

            return key;
        }
    }
}
