using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Schema;
using FluentAssertions;
using Monbetsu.Test.Utils;
using NUnit.Framework;

namespace Monbetsu.Test
{
    public partial class Verifier
    {
        private int assertGroupingCount = 0;

        [SetUp]
        public void Setup()
        {
            assertGroupingCount = 1;
        }

        private protected void AssertGrouping(params Input[] inputs)
            => AssertGrouping(null, ClassificationVariation.Default, inputs.AsEnumerable());


        private protected void AssertGrouping(IEnumerable<Input> inputs)
            => AssertGrouping(null, ClassificationVariation.Default, inputs.AsEnumerable());

        private protected void AssertGrouping(ImplementationVersions? versionFilter, params Input[] inputs)
            => AssertGrouping(versionFilter, ClassificationVariation.Default, inputs.AsEnumerable());

        private protected void AssertGrouping(ImplementationVersions? versionFilter, IEnumerable<Input> inputs)
            => AssertGrouping(versionFilter, ClassificationVariation.Default, inputs.AsEnumerable());

        
        class VarificationOptions : Recorder<Graph, Node, Edge, Label>.RecordOptions
        {
            public VarificationOptions(Pattern pattern)
            {
                Pattern = pattern;
                unresolvedGroupSeq = pattern.UnexpectedLabelGroupSeq - 10000;

                SnapshotFlags = SnapshotFlags.Assert;
                EdgeDecorator = DecorateEdge;
                SeriesSubgraphDecorator = (dotEdge, pair, label, edgeStatus, options) => DecoreateSubgraph(SerDiff, dotEdge, pair, label, edgeStatus, options);
                ParallelSubgraphDecorator = (dotEdge, pair, label, edgeStatus, options) => DecoreateSubgraph(ParDiff, dotEdge, pair, label, edgeStatus, options);
                KnotSubgraphDecorator = (dotEdge, pair, label, edgeStatus, options) => DecoreateSubgraph(KnotDiff, dotEdge, pair, label, edgeStatus, options);
                OptionConfigurer = DecorateRoot;
            }

            private void DecorateEdge(Dot.Edge dotEdge, NodePair<Node, Edge> pair, Label? label, EdgeStatus edgeStatus, Recorder<Graph, Node, Edge, Label>.RecordOptions options)
            {
                if (EdgeDiff.Commons.TryGetValue(pair, out var common))
                {
                    groupMap.TryGetValue(common.labelGroup.Group, out var labels);
                    // reverseGroupMap.TryGetValue(common.labelKey.Label, out var groups);

                    if (labels?.Count == 1)
                    {
                        dotEdge.Label = $"{common.labelGroup.Group}";
                    }
                    else
                    {
                        dotEdge.Label = $"{common.labelGroup.Group}:[{common.labelKey.Label}]";
                        dotEdge.FontColor = Visualizer.ErrorColor;
                    }
}
                else if (EdgeDiff.ExpectedOnlys.TryGetValue(pair, out var expectedOnly))
                {
                    dotEdge.Label = $"{expectedOnly.Group} / ?";
                    dotEdge.FontColor = Visualizer.ErrorColor;                        
                }
            }

            private void DecoreateSubgraph(Diff<UnorderedNTuple<int>, UnorderedNTuple<Label>> subgraphDiff, Dot.Edge dotEdge, NodePair<Node, UnorderedNTuple<Label>> pair, Label label, EdgeStatus edgeStatus, Recorder<Graph, Node, Edge, Label>.RecordOptions options)
            {
                if (subgraphDiff.Commons.TryGetValue(pair, out var common))
                {
                    groupMap.TryGetValue(common.labelGroup.Group, out var labels);
                    
                    if (labels?.Count == 1)
                    {
                        dotEdge.Label = @$"{common.labelGroup.Group}
{{{string.Join(", ", common.labelGroup.Children)}}}";
                    }
                    else
                    {
                        dotEdge.Label = $"{common.labelGroup.Group}:[{common.labelKey.Label}]";
                        dotEdge.FontColor = Visualizer.ErrorColor;
                    }
                }
                else if (subgraphDiff.ExpectedOnlys.TryGetValue(pair, out var expectedOnly))
                {
                    dotEdge.Label = @$"{expectedOnly.Group} / ?
{{{string.Join(", ", common.labelGroup.Children)}}}";
                    dotEdge.FontColor = Visualizer.ErrorColor;
                }
                else if (subgraphDiff.Unmatches.TryGetValue(pair, out var unmatch))
                {
                    dotEdge.Label = @$"{unmatch.labelGroup.Group}
{{{string.Join(", ", unmatch.labelGroup.Children)}}} / {{{string.Join(", ", pair.Via.Select(sl => ResolveLabelGroup(sl)))}}}";
                    dotEdge.FontColor = Visualizer.ErrorColor;
                }
            }

            private void DecorateRoot(Dot.RootGraph rootGraph)
            {
                var actualOnlys = EdgeDiff.ActualOnlys.Select(a => (start: a.Key.StartNode, end: a.Key.EndNode, color: Visualizer.EdgeColor, label: a.Value));

                foreach (var actual in actualOnlys)
                {
                    var label = reverseGroupMap.TryGetValue(actual.label.Label, out var groups) ? groups.First().ToString() : "?";

                    rootGraph.Edges.Add(new Dot.Edge
                    {
                        From = actual.start.Id,
                        To = actual.end.Id,
                        Label = $"? / {label}",
                        Color = actual.color,
                        FontColor = Visualizer.ErrorColor,
                        Style = Dot.EdgeStyle.dotted,
                    });
                }

                var expectedOnlys = SerDiff.ExpectedOnlys.Select(a => (start: a.Key.StartNode, end: a.Key.EndNode, color: Visualizer.SeriesColor, label: a.Value))
                            .Concat(ParDiff.ExpectedOnlys.Select(a => (start: a.Key.StartNode, end: a.Key.EndNode, color: Visualizer.ParallelColor, label: a.Value)))
                            .Concat(KnotDiff.ExpectedOnlys.Select(a => (start: a.Key.StartNode, end: a.Key.EndNode, color: Visualizer.KnotColor, label: a.Value)));

                foreach (var expected in expectedOnlys)
                {
                    rootGraph.Edges.Add(new Dot.Edge
                    {
                        From = expected.start.Id,
                        To = expected.end.Id,
                        Label = $"{expected.label.Group} / ?",
                        Color = expected.color,
                        FontColor = Visualizer.ErrorColor,
                        Style = Dot.EdgeStyle.dotted,
                    });
                }

                if (Assertions.Count > 0)
                {
                    var asserted = string.Join("\n", Assertions.Select(a => a.ToMessage()));

                    rootGraph.Nodes.Add(new Dot.Node
                    {
                        Id = "RESULT",
                        Color = Dot.NamedColor.Red,
                        Label = asserted,
                        Shape = Dot.PolygonBasedShape.Box
                    });

                    var tailNodes = Pattern.Edges.Select(k => k.ToNode).Except(Pattern.Edges.Select(k => k.FromNode));
                    foreach (var tailNode in tailNodes)
                    {
                        rootGraph.Edges.Add(new Dot.Edge
                        {
                            From = tailNode.Id,
                            To = "RESULT",
                            Style = Dot.EdgeStyle.invis,
                        });
                    }
                }
            }

            private readonly Dictionary<int, HashSet<Label>> groupMap = new Dictionary<int, HashSet<Label>>();
            private readonly Dictionary<Label, HashSet<int>> reverseGroupMap = new Dictionary<Label, HashSet<int>>();
            private readonly Dictionary<Label, int> unresolvedGroupMap = new Dictionary<Label, int>();
            private int unresolvedGroupSeq;

            public void Map(int group, Label label)
            {
                if (!groupMap.TryGetValue(group, out var labels))
                {
                    groupMap[group] = labels = new HashSet<Label>();
                }
                labels.Add(label);
                if (!reverseGroupMap.TryGetValue(label, out var groups))
                {
                    reverseGroupMap[label] = groups = new HashSet<int>();
                }
                groups.Add(group);
            }

            class UnresolvedLabel : Label { }

            public Label? ResolveGroupLabel(int subgroup, GroupKind kind)
            {
                if (groupMap.TryGetValue(subgroup, out var labels))
                {
                    return labels.First();
                }
                return new UnresolvedLabel();
            }

            public int ResolveLabelGroup(Label label)
            {
                if (reverseGroupMap.TryGetValue(label, out var groups))
                {
                    return groups.First();
                }
                if (!unresolvedGroupMap.TryGetValue(label, out var group))
                {
                    unresolvedGroupMap[label] = group = unresolvedGroupSeq++;
                }
                return group;
            }

            public UnorderedNTuple<Label> ResolveGroupLabels(IEnumerable<int> subgroups, GroupKind kind)
                => new UnorderedNTuple<Label>(subgroups.Select(group => ResolveGroupLabel(group, kind)).OfType<Label>());

            public List<IAssertionResult> Assertions { get; } = new List<IAssertionResult>();

            public Pattern Pattern { get; }
            public Diff<Edge, Edge> EdgeDiff { get; set; } = default!;
            public Diff<UnorderedNTuple<int>, UnorderedNTuple<Label>> SerDiff { get; set; } = default!;
            public Diff<UnorderedNTuple<int>, UnorderedNTuple<Label>> ParDiff { get; set; } = default!;

            public Diff<UnorderedNTuple<int>, UnorderedNTuple<Label>> KnotDiff { get; set; } = default!;

            

            public IEnumerable<IAssertionResult>? Assert(Recorder<Graph, Node, Edge, Label> rec)
            {
                var edgeDiff = new Diff<Edge, Edge>(Pattern.EdgeDic2, rec.EdgeResults, _ => _);

                var seensExpected = new HashSet<LabelGroup>();
                var seensActual = new HashSet<Recorder<Graph, Node, Edge, Label>.LabelKey>();
                foreach (var (expected, actual) in edgeDiff.Commons.Values)
                {
                    var expectedParentFound = true;
                    var actualParentFound = true;
                    var expected_ = expected;
                    var actual_ = actual;

                    while (expectedParentFound && actualParentFound)
                    {
                        if (seensExpected.Add(expected_!))
                        {
                            Map(expected_!.Group, actual_!.Label);

                            expectedParentFound = Pattern.LabelParents.TryGetValue(expected_, out expected_);
                            actualParentFound = rec.LabelParents.TryGetValue(actual_, out actual_);
                        }
                        else
                        {
                            if (seensActual.Add(actual_!))
                            {
                                Map(expected_!.Group, actual_!.Label);
                            }
                            break;
                        }
                    }
                }

                var serDiff = new Diff<UnorderedNTuple<int>, UnorderedNTuple<Label>>(Pattern.SerDic2, rec.SerResults, key => ResolveGroupLabels(key, GroupKind.Series));

                var parDiff = new Diff<UnorderedNTuple<int>, UnorderedNTuple<Label>>(Pattern.ParDic2, rec.ParResults, key => ResolveGroupLabels(key, GroupKind.Parallel));

                var knotDiff = new Diff<UnorderedNTuple<int>, UnorderedNTuple<Label>>(Pattern.KnotDic2, rec.KnotResults, key => ResolveGroupLabels(key, GroupKind.Knot));

                EdgeDiff = edgeDiff;
                SerDiff = serDiff;
                ParDiff = parDiff;
                KnotDiff = knotDiff;

                foreach (var multiLabelKv in groupMap.Where(kv => kv.Value.Count > 1))
                {
                    Assertions.Add(new MappingAssertion<int, Label>(multiLabelKv.Key, "group", multiLabelKv.Value, "label"));
                }

                foreach (var multiLabelKv in reverseGroupMap.Where(kv => kv.Value.Count > 1))
                {
                    Assertions.Add(new MappingAssertion<Label, int>(multiLabelKv.Key, "label", multiLabelKv.Value, "group"));
                }

                void CheckOnlys<TExpected, TActual>(Diff<TExpected, TActual> diff, GroupKind kind)
                    where TExpected : class
                {
                    foreach (var actual in diff.ActualOnlys)
                    {
                        Assertions.Add(new UnexpectedComponentAssertion<TActual>(kind, actual.Key));
                    }
                    foreach (var expected in diff.ExpectedOnlys)
                    {
                        Assertions.Add(new ComponentNotLabeledAssertion<TActual>(kind, expected.Key, expected.Value.Group));
                    }
                }

                void CheckUnmatches(Diff<UnorderedNTuple<int>, UnorderedNTuple<Label>> diff, GroupKind kind)
                {
                    foreach (var unmatch in diff.Unmatches)
                    {
                        var actualGroups = unmatch.Key.Via.Select(ResolveLabelGroup);
                        Assertions.Add(new SublabelsUnmatchAssertion(kind, unmatch.Key, unmatch.Value.labelGroup.Children, actualGroups));
                    }
                }

                CheckOnlys(EdgeDiff, GroupKind.Edge);
                CheckOnlys(SerDiff, GroupKind.Series);
                CheckOnlys(ParDiff, GroupKind.Parallel);
                CheckOnlys(KnotDiff, GroupKind.Knot);

                CheckUnmatches(SerDiff, GroupKind.Series);
                CheckUnmatches(ParDiff, GroupKind.Parallel);
                CheckUnmatches(KnotDiff, GroupKind.Knot);


                return Assertions;
            }
        }

        [DebuggerDisplay("common:{Commons.Count}, expectedOnly:{ExpectedOnlys.Count}, actualOnly:{ActualOnlys.Count}")]
        class Diff<TExpected, TActual>
            where TExpected : class
        {
            public IReadOnlyDictionary<NodePair<Node, TActual>, (LabelGroup labelGroup, Recorder<Graph, Node, Edge, Label>.LabelKey labelKey)> Commons { get; }
            public IReadOnlyDictionary<NodePair<Node, TActual>, LabelGroup> ExpectedOnlys { get; }
            public IReadOnlyDictionary<NodePair<Node, TActual>, Recorder<Graph, Node, Edge, Label>.LabelKey> ActualOnlys { get; }

            public IReadOnlyDictionary<NodePair<Node, TActual>, (LabelGroup labelGroup, Recorder<Graph, Node, Edge, Label>.LabelKey labelKey)> Unmatches { get; }

            public Diff(
                        IReadOnlyDictionary<NodePair<Node, TExpected>, LabelGroup> expected,
                        IReadOnlyDictionary<NodePair<Node, TActual>, Recorder<Graph, Node, Edge, Label>.LabelKey> actual,
                        Func<TExpected, TActual> converter
                        )
            {
                var common = new Dictionary<NodePair<Node, TActual>, (LabelGroup labelGroup, Recorder<Graph, Node, Edge, Label>.LabelKey labelKey)>();
                var expectedOnly = new Dictionary<NodePair<Node, TActual>, LabelGroup>();
                foreach (var expectedKv in expected)
                {
                    var actualKey = expectedKv.Key.Transform(converter);

                    if (actual.TryGetValue(actualKey, out var labelKey))
                    {
                        common[actualKey] = (expectedKv.Value, labelKey);
                    }
                    else
                    {
                        expectedOnly[actualKey] = expectedKv.Value;
                    }
                }
                var actualOnly = actual.Where(kv => !common.ContainsKey(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                var unmatches = new Dictionary<NodePair<Node, TActual>, (LabelGroup labelGroup, Recorder<Graph, Node, Edge, Label>.LabelKey labelKey)>();

                if (typeof(TExpected) == typeof(UnorderedNTuple<int>) && typeof(TActual) == typeof(UnorderedNTuple<Label>))
                {
                    var actualKeyLu = actualOnly.ToLookup(a => (startNode: a.Key.StartNode, endNode: a.Key.EndNode, kind: a.Value.Kind));
                    var expectedKeyLu = expectedOnly.ToLookup(e => (startNode: e.Key.StartNode, endNode: e.Key.EndNode, kind: e.Value.Kind));

                    foreach(var actualKey in actualKeyLu)
                    {
                        var expectedKey = expectedKeyLu[actualKey.Key];

                        var actualStat = actualKey.SelectMany(k => k.Key.Via as UnorderedNTuple<Label>, (k, g) => (k, g)).ToLookup(_ => _.g, _ => _.k);
                        var expectedStat = expectedKey.SelectMany(k => converter((new UnorderedNTuple<int>(k.Value.Children) as TExpected)!) as UnorderedNTuple<Label>, (k, g) => (k, g)).ToLookup(_ => _.g, _ => _.k);

                        var actualSeens = new HashSet<Recorder<Graph, Node, Edge, Label>.LabelKey>();
                        var expectedSeens = new HashSet<LabelGroup>();

                        foreach (var actualCandidate in actualStat.Where(g => g.Count(_ => !actualSeens.Contains(_.Value)) == 1))
                        {
                            var expectedCandidate = expectedStat[actualCandidate.Key];
                            if (expectedCandidate.Count(_ => !expectedSeens.Contains(_.Value)) == 1)
                            {
                                var actualSide = actualCandidate.First(_ => !actualSeens.Contains(_.Value));
                                var expectedSide = expectedCandidate.First(_ => !expectedSeens.Contains(_.Value));

                                actualSeens.Add(actualSide.Value);
                                expectedSeens.Add(expectedSide.Value);

                                unmatches[actualSide.Key] = (expectedSide.Value, actualSide.Value);
                                actualOnly.Remove(actualSide.Key);
                                expectedOnly.Remove(expectedSide.Key);
                            }
                        }
                    }

                    actualKeyLu = actualOnly.ToLookup(a => (startNode: a.Key.StartNode, endNode: a.Key.EndNode, kind: a.Value.Kind));
                    expectedKeyLu = expectedOnly.ToLookup(e => (startNode: e.Key.StartNode, endNode: e.Key.EndNode, kind: e.Value.Kind));

                    foreach (var actualKey in actualKeyLu)
                    {
                        if (actualKey.Count() == 1)
                        {
                            var expectedKey = expectedKeyLu[actualKey.Key];
                            if (expectedKey.Count() == 1)
                            {
                                unmatches[actualKey.First().Key] = (expectedKey.First().Value, actualKey.First().Value);
                                actualOnly.Remove(actualKey.First().Key);
                                expectedOnly.Remove(expectedKey.First().Key);
                            }
                        }
                    }
                }

                Commons = common;
                ExpectedOnlys = expectedOnly;
                ActualOnlys = actualOnly;
                Unmatches = unmatches;
            }
        }

        
        private protected class MappingAssertion<TFrom, TTo> : IAssertionResult
        {
            public TFrom From { get; }
            public IEnumerable<TTo> Tos { get; }

            public string FromLabel { get; }
            public string ToLabel { get; }
            public MappingAssertion(TFrom from, string fromLabel, IEnumerable<TTo> tos, string toLabel)
            {
                From = from;
                FromLabel = fromLabel;
                Tos = tos ?? throw new ArgumentNullException(nameof(tos));
                ToLabel = toLabel;
            }

            public string ToMessage()
            {
                return $"{FromLabel} {From} is mapped to multiple {ToLabel}s: [{string.Join(", ", Tos)}]";
            }
        }

        private protected class UnexpectedComponentAssertion<TVia> : IAssertionResult
        {
            public GroupKind Kind { get; }
            public NodePair<Node, TVia> Pair { get; }

            public UnexpectedComponentAssertion(GroupKind kind, NodePair<Node, TVia> pair)
            {
                Kind = kind;
                Pair = pair ?? throw new ArgumentNullException(nameof(pair));
            }

            public string ToMessage()
            {
                return $"unexpected {Kind} is found: {Pair.StartNode} -> {Pair.EndNode} via [{string.Join(", ", Pair.Via)}].";
            }
        }

        private protected class ComponentNotLabeledAssertion<TVia> : IAssertionResult
        {
            public GroupKind Kind { get; }
            public NodePair<Node, TVia> Pair { get; }
            public int ExpectedGroup { get; }

            public ComponentNotLabeledAssertion(GroupKind kind, NodePair<Node, TVia> pair, int expectedGroup)
            {
                Kind = kind;
                ExpectedGroup = expectedGroup;
                Pair = pair ?? throw new ArgumentNullException(nameof(pair));
            }

            public string ToMessage()
            {
                return $"{Kind} {Pair.StartNode} -> {Pair.EndNode} via [{string.Join(", ", Pair.Via)}] labeled '{ExpectedGroup}' is not found.";
            }
        }

        private protected class SublabelsUnmatchAssertion : IAssertionResult
        {
            public GroupKind Kind { get; }
            public NodePair<Node, UnorderedNTuple<Label>> Pair { get; }
            public IReadOnlyList<int> ExpectedGroups { get; }
            public IReadOnlyList<int> ActualGroups { get; }

            public SublabelsUnmatchAssertion(GroupKind kind, NodePair<Node, UnorderedNTuple<Label>> pair, IEnumerable<int> expectedGroups, IEnumerable<int> actualGroups)
            {
                Kind = kind;
                ExpectedGroups = expectedGroups.OrderBy(_ => _).ToList();
                ActualGroups = actualGroups.OrderBy(_ => _).ToList();
                Pair = pair ?? throw new ArgumentNullException(nameof(pair));
            }

            public string ToMessage()
            {
                return $"{Kind} {Pair.StartNode} -> {Pair.EndNode} via [{string.Join(", ", Pair.Via)}] expect to have sublabels [{string.Join(", ", ExpectedGroups)}], but [{string.Join(", ", ActualGroups)}].";
            }
        }


        private protected virtual ITestGraphEnvironment CreateEnvironment(Pattern pattern)
        {
            var env = pattern.Version switch
            {
                ImplementationVersions.Latest => new RecordGraphEnvironment() as ITestGraphEnvironment,
                ImplementationVersions.V03 => new Test.v03.RecordGraphEnvironment(),
                _ => throw new Exception()
            };

            return env;
        }

        private protected void AssertGrouping(ClassificationVariation variation, IEnumerable<Input> inputs)
            => AssertGrouping(default, variation, inputs);

        private protected void AssertGrouping(ImplementationVersions? versionFilter, ClassificationVariation variation, IEnumerable<Input> inputs)
        {
            foreach (var pattern in Pattern.Generate(inputs, assertGroupingCount++, versionFilter, variation))
            {
                AssertGrouping(pattern);
            }
        }

        private protected void AssertGrouping(Pattern pattern)
        {
            var env = CreateEnvironment(pattern);

            var options = new VarificationOptions(pattern);

            var graph = new Graph(pattern.Nodes, pattern.Edges);

            var outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "test-images", TestContext.CurrentContext.Test.FullName);

            Directory.CreateDirectory(outputDir);

            var recorder = new Recorder<Graph, Node, Edge, Label>(node => node.Id.ToString(), options, outputDir);

            recorder.Record(pattern.Id, pattern.EdgeWithNodePair, rec =>
            {
                switch (pattern.Variation)
                {
                    case ClassificationVariation.Default:
                        env.Classify(graph, pattern.StartNodes,
                            rec.LabelEdge,
                            rec.LabelSeries,
                            rec.LabelParallel,
                            rec.LabelKnot);
                        break;
                    case ClassificationVariation.Integrated:
                        env.ClassifyIntegratedly(graph, pattern.StartNodes,
                            rec.LabelEdge,
                            rec.LabelSeries,
                            rec.LabelParallel,
                            rec.LabelKnot);
                        break;
                    case ClassificationVariation.Unique:
                        env.ClassifyUniquely(graph, pattern.StartNodes,
                            rec.LabelEdge,
                            rec.LabelSeries,
                            rec.LabelParallel,
                            rec.LabelKnot);
                        break;
                }
            },
            options.Assert);
        }

        private protected static LabelGroup Knot(params int[] children) => new LabelGroup(children, GroupKind.Knot);
        private protected static LabelGroup Ser(params int[] children) => new LabelGroup(children, GroupKind.Series);
        private protected static LabelGroup Par(params int[] children) => new LabelGroup(children, GroupKind.Parallel);

        private protected static Input Sentinel() => new Input();
    }

    public class VerifierTest : Verifier
    {
        private protected class MockEnv : ITestGraphEnvironment
        {
            private readonly Pattern pattern;
            private EdgeLabeler<Graph, Node, Edge, Label> labelEdge = default!;
            private SeriesSubgraphLabeler<Graph, Node, Label> labelSeries = default!;
            private UniquelySeriesSubgraphLabeler<Graph, Node, Label> labelSeriesUniquely = default!;
            private ParallelSubgraphLabeler<Graph, Node, Label> labelParallel = default!;
            private KnotSubgraphLabeler<Graph, Node, Label> labelKnot = default!;
            private List<Action<MockEnv>> actionList = new List<Action<MockEnv>>();

            private int unexpectedLabelGroupSeq;
            private int unexpectedEdgeBase;
            private int unexpectedEdgeSeq;
            private int unexpectedNodeBase;
            private int unexpectedNodeSeq;
            private readonly Dictionary<int, Node> unexpectedNodes = new Dictionary<int, Node>();
            private readonly Dictionary<Edge, int> unexpectedEdges = new Dictionary<Edge, int>();
            public MockEnv(Pattern pattern, Action<MockEnv> registerer)
            {
                this.pattern = pattern;

                unexpectedNodeBase = unexpectedNodeSeq = pattern.Nodes.Max(e => e.Id) + 1;
                unexpectedEdgeBase = unexpectedEdgeSeq = pattern.Edges.Max(e => e.Id) + 1;
                unexpectedLabelGroupSeq = pattern.UnexpectedLabelGroupSeq;

                registerer(this);
            }

            public Node Node(int id)
            {
                if (unexpectedNodes.TryGetValue(id, out var node))
                {
                    return node;
                }
                return pattern.Node(id);
            }

            public Edge Edge(int from, int to, int group)
            {
                var kv = unexpectedEdges.Where(kv => kv.Key.FromNode.Id == from && kv.Key.ToNode.Id == to && kv.Value == group).FirstOrDefault();
                
                return kv.Key ?? pattern.Edge(from, to, group);
            }

            public Node CreateUnexpectedNode()
            {
                var node = new Node(unexpectedNodeSeq++);

                unexpectedNodes[node.Id] = node;

                return node;
            }

            public Edge CreateUnexpectedEdge(int fromId, int toId, int? group = null)
            {
                var edge = new Edge(Node(fromId), Node(toId), unexpectedEdgeSeq++);

                unexpectedEdges[edge] = group ?? unexpectedLabelGroupSeq--;

                return edge;
            }

            public Label CreateUnexpectedLabel()
            {
                return new Label();
            }

            public Label LabelEdge(Edge edge, Label? label = null) => LabelEdge(edge.FromNode.Id, edge, edge.ToNode.Id, label);

            public Label LabelEdge(int fromId, Edge edge, int toId, Label? label = null)
            {
                var fromNode = Node(fromId);
                var toNode = Node(toId);

                label ??= new Label { BaseEdge = edge };

                actionList.Add((MockEnv env) => env.labelEdge(default!, fromNode, edge, toNode, label));

                return label;
            }
        
            public Label LabelSeries(int startId, Label sublebel, int endId, Label? label = null)
            {
                var startNode = Node(startId);
                var endNode = Node(endId);

                label ??= new Label { BaseSubgraph = (startNode, endNode) };

                actionList.Add((MockEnv env) => env.labelSeries(default!, startNode, sublebel, endNode, label));

                return label;
            }
                
            
            public Label LabelSeriesUniquely(int startId, IEnumerable<Label> sublebels, int endId, Label? label = null)
            {
                var startNode = Node(startId);
                var endNode = Node(endId);

                label ??= new Label { BaseSubgraph = (startNode, endNode) };

                actionList.Add((MockEnv env) => env.labelSeriesUniquely(default!, startNode, sublebels, endNode, label));

                return label;
            }
            
            public Label LabelParallel(int startId, IEnumerable<Label> sublebels, int endId, Label? label = null)
            {
                var startNode = Node(startId);
                var endNode = Node(endId);

                label ??= new Label { BaseSubgraph = (startNode, endNode) };

                actionList.Add((MockEnv env) => env.labelParallel(default!, startNode, sublebels, endNode, label));

                return label;
            }
            
            public Label LabelKnot(int startId, IEnumerable<Label> sublebels, int endId, Label? label = null)
            {
                var startNode = Node(startId);
                var endNode = Node(endId);

                label ??= new Label { BaseSubgraph = (startNode, endNode) };

                actionList.Add((MockEnv env) => env.labelKnot(default!, startNode, sublebels, endNode, label));

                return label;
            }


            public void Classify(Graph graph, IEnumerable<Node> startNodes, EdgeLabeler<Graph, Node, Edge, Label>? labelEdge, SeriesSubgraphLabeler<Graph, Node, Label>? labelSeries, ParallelSubgraphLabeler<Graph, Node, Label>? labelParallel, KnotSubgraphLabeler<Graph, Node, Label>? labelKnot, CancellationToken cancellationToken = default)
            {
                this.labelEdge = labelEdge!;
                this.labelSeries = labelSeries!;
                this.labelParallel = labelParallel!;
                this.labelKnot = labelKnot!;

                foreach (var action in actionList)
                {
                    action(this);
                }
            }

            public void ClassifyIntegratedly(Graph graph, IEnumerable<Node> startNodes, EdgeLabeler<Graph, Node, Edge, Label>? labelEdge, SeriesSubgraphLabeler<Graph, Node, Label>? labelSeries, ParallelSubgraphLabeler<Graph, Node, Label>? labelParallel, KnotSubgraphLabeler<Graph, Node, Label>? labelKnot, CancellationToken cancellationToken = default)
            {
                this.labelEdge = labelEdge!;
                this.labelSeries = labelSeries!;
                this.labelParallel = labelParallel!;
                this.labelKnot = labelKnot!;


                foreach (var action in actionList)
                {
                    action(this);
                }
            }

            public void ClassifyUniquely(Graph graph, IEnumerable<Node> startNodes, EdgeLabeler<Graph, Node, Edge, Label>? labelEdge, UniquelySeriesSubgraphLabeler<Graph, Node, Label>? labelSeries, ParallelSubgraphLabeler<Graph, Node, Label>? labelParallel, KnotSubgraphLabeler<Graph, Node, Label>? labelKnot, CancellationToken cancellationToken = default)
            {
                this.labelEdge = labelEdge!;
                this.labelSeriesUniquely = labelSeries!;
                this.labelParallel = labelParallel!;
                this.labelKnot = labelKnot!;

                foreach (var action in actionList)
                {
                    action(this);
                }
            }
        }

        private MockEnv mockEnv = default!;

        private protected void TestAssertGrouping(Pattern pattern, Action<MockEnv> arranger)
        {
            mockEnv = new MockEnv(pattern, arranger);
            AssertGrouping(pattern);
        }

        private protected AssertionFailedException TestAssertGroupingWithFailure(Pattern pattern, Action<MockEnv> arranger)
        {
            return new Action(() => TestAssertGrouping(pattern, arranger))
                .Should().Throw<AssertionFailedException>()
                .Which;
        }

        private protected override ITestGraphEnvironment CreateEnvironment(Pattern pattern)
        {
            return mockEnv;
        }

        
        public class Test1 : VerifierTest
        {
            private Pattern pattern = Pattern.Generate(new Input[] {
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 0, to: 2, group: -2, kind: Ser(-1))
            }, 0).First();

            [Test]
            public void TestOk()
            {
                TestAssertGrouping(pattern, env =>
                {
                    var label = env.LabelEdge(pattern.Edge(0, 1, -1));
                    env.LabelEdge(pattern.Edge(1, 2, -1), label);
                    env.LabelSeries(0, label, 2);
                });
            }

            [Test]
            public void TestFail01()
            {
                Label label1 = default!;
                Label label2 = default!;

                TestAssertGroupingWithFailure(pattern, env =>
                {
                    label1 = env.LabelEdge(pattern.Edge(0, 1, -1));
                    label2 = env.LabelEdge(pattern.Edge(1, 2, -1));
                    env.LabelSeries(0, label1, 2);
                })
                .ShouldAllOf<MappingAssertion<int, Label>, int>(result => result.From,
                    (-1, result =>
                    {
                        result.Tos.Should().BeEquivalentTo(label1, label2);
                    })
                );
            }

            [Test]
            public void TestFail02()
            {
                Label label2 = default!;

                TestAssertGroupingWithFailure(pattern, env =>
                {
                    var label = env.LabelEdge(pattern.Edge(0, 1, -1));
                    env.LabelEdge(env.Edge(1, 2, -1), label);
                    label2 = env.LabelSeries(0, label, 2, label);
                })
                .ShouldAllOf<MappingAssertion<Label, int>, Label>(result => result.From,
                    (label2, result =>
                    {
                        result.Tos.Should().BeEquivalentTo(-1, -2);
                    }
                )
                );
            }

            [Test]
            public void TestFail03()
            {
                var exception = TestAssertGroupingWithFailure(pattern, env =>
                {
                    var label = env.LabelEdge(pattern.Edge(0, 1, -1));
                    env.LabelEdge(env.Edge(1, 2, -1), label);
                    env.LabelSeries(1, label, 2);
                    env.LabelEdge(env.CreateUnexpectedEdge(0, 2));
                });

                exception.ShouldAllOf<UnexpectedComponentAssertion<Edge>, (int startId, int endId)>(
                    result => (result.Pair.StartNode.Id, result.Pair.EndNode.Id),
                    ((startId: 0, endId: 2), result =>
                    {
                        result.Kind.Should().Be(GroupKind.Edge);
                    }
                )
                );

                exception.ShouldAllOf<UnexpectedComponentAssertion<UnorderedNTuple<Label>>, (int startId, int endId)>(
                    result => (result.Pair.StartNode.Id, result.Pair.EndNode.Id),
                    ((startId: 1, endId: 2), result =>
                    {
                        result.Kind.Should().Be(GroupKind.Series);
                    }
                )
                );
            }

            [Test]
            public void TestFail04()
            {
                var exception = TestAssertGroupingWithFailure(pattern, env =>
                {
                    env.LabelEdge(pattern.Edge(0, 1, -1));
                });

                exception.ShouldAllOf<ComponentNotLabeledAssertion<Edge>, (int startId, int endId)>(
                    result => (result.Pair.StartNode.Id, result.Pair.EndNode.Id),
                    ((startId: 1, endId: 2), result =>
                    {
                        result.ExpectedGroup.Should().Be(-1);
                    }
                )
                );

                exception.ShouldAllOf<ComponentNotLabeledAssertion<UnorderedNTuple<Label>>, (int startId, int endId)>(
                    result => (result.Pair.StartNode.Id, result.Pair.EndNode.Id),
                    ((startId: 0, endId: 2), result =>
                    {
                        result.ExpectedGroup.Should().Be(-2);
                    }
                )
                );
            }

            [Test]
            public void TestFail05()
            {
                Label label1 = default!;
                Label label2 = default!;
                Label label3 = default!;

                TestAssertGroupingWithFailure(pattern, env =>
                {
                    label1 = env.LabelEdge(pattern.Edge(0, 1, -1));
                    env.LabelEdge(pattern.Edge(1, 2, -1), label1);

                    label2 = env.CreateUnexpectedLabel();

                    label3 = env.LabelSeries(0, label2, 2);
                })
                .ShouldAllOf<SublabelsUnmatchAssertion, (int startId, int endId)> (
                    result => (result.Pair.StartNode.Id, result.Pair.EndNode.Id),
                    ((startId: 0, endId: 2), result =>
                    {
                        result.ExpectedGroups.Should().BeEquivalentTo(-1);
                        result.ActualGroups.Should().BeEquivalentTo(-10003);
                    }
                )
                );
            }
        }

        public class Test2 : VerifierTest
        {
            private Pattern pattern = Pattern.Generate(new Input[] {
                (from: 0, to: 1, group: -1),
                (from: 0, to: 1, group: -2),
                (from: 0, to: 1, group: -3, kind: Par(-1, -2))
            }, 0).First();

            [Test]
            public void TestOk()
            {
                TestAssertGrouping(pattern, env =>
                {
                    var label1 = env.LabelEdge(pattern.Edge(0, 1, -1));
                    var label2 = env.LabelEdge(pattern.Edge(0, 1, -2));
                    env.LabelParallel(0, new[] { label1, label2 }, 1);
                });
            }

            [Test]
            public void TestFail01()
            {
                TestAssertGroupingWithFailure(pattern, env =>
                {
                    var label1 = env.LabelEdge(pattern.Edge(0, 1, -1));
                    var label2 = env.LabelEdge(pattern.Edge(0, 1, -2));
                    env.LabelParallel(1, new[] { label1, label2 }, 0);
                })
                .ShouldAllOf<UnexpectedComponentAssertion<UnorderedNTuple<Label>>, (int startId, int endId)>(
                    result => (result.Pair.StartNode.Id, result.Pair.EndNode.Id),
                    ((startId: 1, endId: 0), result =>
                    {
                        result.Kind.Should().Be(GroupKind.Parallel);
                    }
                )
                );
            }

            [Test]
            public void TestFail02()
            {
                TestAssertGroupingWithFailure(pattern, env =>
                {
                    var label1 = env.LabelEdge(pattern.Edge(0, 1, -1));
                    var label2 = env.LabelEdge(pattern.Edge(0, 1, -2));
                })
                .ShouldAllOf<ComponentNotLabeledAssertion<UnorderedNTuple<Label>>, (int startId, int endId)>(
                    result => (result.Pair.StartNode.Id, result.Pair.EndNode.Id),
                    ((startId: 0, endId: 1), result =>
                    {
                        result.ExpectedGroup.Should().Be(-3);
                    }
                )
                );
            }

            [Test]
            public void TestFail03()
            {
                TestAssertGroupingWithFailure(pattern, env =>
                {
                    var label1 = env.LabelEdge(pattern.Edge(0, 1, -1));
                    var label2 = env.LabelEdge(pattern.Edge(0, 1, -2));
                    env.LabelParallel(0, new[] { label1 }, 1);
                })
                .ShouldAllOf<SublabelsUnmatchAssertion, (int startId, int endId)>(
                    result => (result.Pair.StartNode.Id, result.Pair.EndNode.Id),
                    ((startId: 0, endId: 1), result =>
                    {
                        result.ExpectedGroups.Should().BeEquivalentTo(-1, -2);
                        result.ActualGroups.Should().BeEquivalentTo(-1);
                    }
                )
                );
            }
        }

        public class Test3 : VerifierTest
        {
            private Pattern pattern = Pattern.Generate(new Input[] {
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -1),
                (from: 0, to: 2, group: -1, kind: Ser(-1)),
                (from: 0, to: 2, group: -2),
                (from: 0, to: 2, group: -3, kind: Par(-1, -2)),
            }, 0, variation: ClassificationVariation.Integrated).First();

            [Test]
            public void TestOk()
            {
                TestAssertGrouping(pattern, env =>
                {
                    var label1 = env.LabelEdge(env.Edge(0, 1, -1));
                    env.LabelEdge(env.Edge(1, 2, -1), label1);
                    var label2 = env.LabelEdge(env.Edge(0, 2, -2));
                    env.LabelSeries(0, label1, 2, label1);
                    env.LabelParallel(0, new[] { label1, label2 }, 2);
                });
            }

            [Test]
            public void TestFail01()
            {
                Label label1 = default!;
                Label label2 = default!;
                Label label3 = default!;

                TestAssertGroupingWithFailure(pattern, env =>
                {
                    label1 = env.LabelEdge(env.Edge(0, 1, -1));
                    env.LabelEdge(env.Edge(1, 2, -1), label1);
                    label2 = env.LabelEdge(env.Edge(0, 2, -2));
                    label3 = env.LabelSeries(0, label1, 2);
                    env.LabelParallel(0, new[] { label3, label2 }, 2);
                })
                .ShouldAllOf<MappingAssertion<int, Label>, int>(result => result.From,
                    (-1, result =>
                    {
                        result.Tos.Should().BeEquivalentTo(label1, label3);
                    }
                )
                );
            }
        }

        public class Test4 : VerifierTest
        {
            private Pattern pattern = Pattern.Generate(new Input[] {
                (from: 0, to: 1, group: -1),
                (from: 1, to: 2, group: -2),
                (from: 0, to: 2, group: -3, kind: Ser(-1, -2)),
                (from: 0, to: 2, group: -4),
                (from: 0, to: 2, group: -5, kind: Par(-3, -4)),
            }, 0, variation: ClassificationVariation.Unique).First();

            [Test]
            public void TestOk()
            {
                TestAssertGrouping(pattern, env =>
                {
                    var label1 = env.LabelEdge(env.Edge(0, 1, -1));
                    var label2 = env.LabelEdge(env.Edge(1, 2, -2));
                    var label3 = env.LabelSeriesUniquely(0, new[] { label1, label2 }, 2);
                    var label4 = env.LabelEdge(env.Edge(0, 2, -4));
                    env.LabelParallel(0, new[] { label3, label4 }, 2);
                });
            }

        }
    }
}
