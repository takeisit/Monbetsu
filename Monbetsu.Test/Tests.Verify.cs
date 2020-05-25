using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Monbetsu.Test
{
    public partial class Tests
    {
        private int assertGroupingCount = 0;

        static partial void AttachVisual(
            IReadOnlyDictionary<Edge, LabelGroup> expectedEdgeGroupDic,
            IReadOnlyDictionary<Edge, LabelGroup> resultEdgeGroupDic,
            IReadOnlyDictionary<(Node startNode, int subgroup, Node endNode), LabelGroup> expectedSerGroupDic,
            IReadOnlyDictionary<(Node startNode, int subgroup, Node endNode), LabelGroup> resultSerGroupDic,
            IReadOnlyDictionary<(Node startNode, Node endNode), LabelGroup> expectedParGroupDic,
            IReadOnlyDictionary<(Node startNode, Node endNode), LabelGroup> resultParGroupDic,
            IReadOnlyDictionary<(Node startNode, UnorderedNTuple<int> subgroups, Node endNode), LabelGroup> expectedKnotGroupDic,
            IReadOnlyDictionary<(Node startNode, UnorderedNTuple<int> subgroups, Node endNode), LabelGroup> resultKnotGroupDic,
            string id, List<string> assertionMessages);

        [SetUp]
        public void Setup()
        {
            assertGroupingCount = 1;
        }

        private void AssertGrouping(params Input[] inputs)
            => AssertGrouping(null, inputs.AsEnumerable());


        private void AssertGrouping(IEnumerable<Input> inputs)
            => AssertGrouping(null, inputs.AsEnumerable());

        private void AssertGrouping(ImplementationVersions? versionFilter, params Input[] inputs)
            => AssertGrouping(versionFilter, inputs.AsEnumerable());


        private void AssertGrouping(ImplementationVersions? versionFilter, IEnumerable<Input> inputs)
        {
            foreach (var pattern in Pattarn.Generate(inputs, assertGroupingCount++, versionFilter))
            {
                Debug.WriteLine($"{TestContext.CurrentContext.Test.MethodName} {pattern.Id}");

                var env = pattern.Version switch
                {
                    ImplementationVersions.Latest => new TestGraphEnvironment() as ITestGraphEnvironment,
                    ImplementationVersions.V03 => new v03.TestGraphEnvironment(),
                    _ => throw new Exception()
                };

                var graph = new Graph(pattern.Nodes, pattern.Edges);
                var edgeResults = new Dictionary<Edge, Label>();
                var serResults = new Dictionary<(Node startNode, Label sublabel, Node endNode), Label>();
                var parResults = new Dictionary<(Node startNode, Node endNode), Label>();
                var knotResults = new Dictionary<(Node startNode, IReadOnlyList<Label> sublabels, Node endNode), Label>();

                var labelI = new Dictionary<Label, LabelGroup>();
                var labelSeq = 
                    Math.Min(
                        Math.Min(
                            pattern.EdgeDic.Values.Select(lg => lg.Group).Min(),
                            pattern.KnotDic.Values.Select(lg => lg.Group).DefaultIfEmpty(0).Min()
                        ),
                        Math.Min(
                            pattern.SerDic.Values.Select(lg => lg.Group).DefaultIfEmpty(0).Min(),
                            pattern.ParDic.Values.Select(lg => lg.Group).DefaultIfEmpty(0).Min()
                        )
                    ) - 1;
                var subToParentLabels = new Dictionary<Label, Label>();
                
                var assertedMessages = new List<string>();


                env.Classify(graph, pattern.StartNodes,
                    (_, fromNode, edge, toNode, label) =>
                    {
                        edgeResults[edge] = label;

                        if (!pattern.EdgeDic.TryGetValue(edge, out var groupId))
                        {
                            groupId = LabelGroup.Edge(labelSeq--);
                        }

                        if (labelI.TryGetValue(label, out var existingId))
                        {
                            if (existingId.Group != groupId.Group)
                            {
                                assertedMessages.Add($"label is overrided {existingId} -> {groupId}.");
                            }
                            else
                            {
                                groupId = existingId;
                            }
                        }

                        labelI[label] = groupId;
                    },
                    (_, startNode, sublabel, endNode, label) =>
                    {
                        serResults[(startNode, sublabel, endNode)] = label;
                        subToParentLabels[sublabel] = label;

                        if (!pattern.SerDic.TryGetValue((startNode, labelI[sublabel].Group, endNode), out var groupId))
                        {
                            groupId = new LabelGroup(Array.Empty<int>(), GroupKind.Series) { Group = labelSeq-- };
                        }

                        if (labelI.TryGetValue(label, out var existingId))
                        {
                            if (existingId.Group != groupId.Group)
                            {
                                assertedMessages.Add($"label is overrided {existingId} -> {groupId}.");
                            }
                            else
                            {
                                groupId = existingId;
                            }
                        }

                        labelI[label] = groupId;
                    },
                    (_, startNode, sublabels, endNode, label) =>
                    {
                        parResults[(startNode, endNode)] = label;
                        foreach(var sublabel in sublabels)
                        {
                            subToParentLabels[sublabel] = label;
                        }

                        if (!pattern.ParDic.TryGetValue((startNode, endNode), out var groupId))
                        {
                            groupId = new LabelGroup(Array.Empty<int>(), GroupKind.Parallel) { Group = labelSeq-- };
                        }

                        if (labelI.TryGetValue(label, out var existingId))
                        {
                            if (existingId.Group != groupId.Group)
                            {
                                assertedMessages.Add($"label is overrided {existingId} -> {groupId}.");
                            }
                            else
                            {
                                groupId = existingId;
                            }
                        }

                        labelI[label] = groupId;
                    },
                    (_, startNode, sublabels, endNode, label) =>
                    {
                        knotResults[(startNode, sublabels.ToList(), endNode)] = label;
                        foreach (var sublabel in sublabels)
                        {
                            subToParentLabels[sublabel] = label;
                        }

                        if (!pattern.KnotDic.TryGetValue((startNode, new UnorderedNTuple<int>(sublabels.Select(l => labelI[l].Group)), endNode), out var groupId))
                        {
                            groupId = new LabelGroup(Array.Empty<int>(), GroupKind.Knot) { Group = labelSeq-- };
                        }

                        if (labelI.TryGetValue(label, out var existingId))
                        {
                            if (existingId.Group != groupId.Group)
                            {
                                assertedMessages.Add($"label is overrided {existingId} -> {groupId}.");
                            }
                            else
                            {
                                groupId = existingId;
                            }
                        }

                        labelI[label] = groupId;
                    });

                var expectedSublabelsLu = pattern.KnotDic.Values.Concat(pattern.ParDic.Values).Concat(pattern.SerDic.Values)
                    .ToLookup(lg => lg.Group);

                foreach (var l in subToParentLabels.ToLookup(sp => labelI[sp.Value], sp => labelI[sp.Key].Group))
                {
                    var expected = expectedSublabelsLu[l.Key.Group].SelectMany(lg => lg.Children).Distinct().OrderBy(_ => _);
                    var actual = l.OrderBy(_ => _);
                    if (!expected.SequenceEqual(actual))
                    {
                        assertedMessages.Add($"unexpected sublabels for {l.Key}. expected: {string.Join(", ", expected)}, actual: {string.Join(", ", actual)}");
                    }
                }


                var edgeResults2 = edgeResults.ToDictionary(kv => kv.Key, kv => labelI[kv.Value]);
                var serResults2 = serResults.ToDictionary(kv => (kv.Key.startNode, labelI[kv.Key.sublabel].Group, kv.Key.endNode), kv => labelI[kv.Value]);
                var parResults2 = parResults.ToDictionary(kv => kv.Key, kv => labelI[kv.Value]);
                var knotResults2 = knotResults.ToDictionary(kv => (kv.Key.startNode, new UnorderedNTuple<int>(kv.Key.sublabels.Select(l => labelI[l].Group)), kv.Key.endNode), kv => labelI[kv.Value]);

                foreach (var dup in labelI.GroupBy(kv => kv.Value).Where(g => g.Count() > 1))
                {
                    assertedMessages.Add($"{dup.Key} has multiple labels: {string.Join(", ", dup)}");
                }

                AttachVisual(
                    pattern.EdgeDic,
                    edgeResults2,
                    pattern.SerDic,
                    serResults2,
                    pattern.ParDic,
                    parResults2,
                    pattern.KnotDic,
                    knotResults2,
                    pattern.Id,
                    assertedMessages);

                if (assertedMessages.Count > 0)
                {
                    Assert.Fail(string.Join(Environment.NewLine, assertedMessages));
                }
            }
        }

        private static LabelGroup Knot(params int[] children) => new LabelGroup(children, GroupKind.Knot);
        private static LabelGroup Ser(params int[] children) => new LabelGroup(children, GroupKind.Series);
        private static LabelGroup Par(params int[] children) => new LabelGroup(children, GroupKind.Parallel);

        private static Input Sentinel() => new Input();
    }

    [DebuggerDisplay("{Kind}: {Group}")]
    class LabelGroup : IEquatable<LabelGroup>
    {
        public static LabelGroup Edge(int groupId = 0) => new LabelGroup(Array.Empty<int>(), GroupKind.Edge) { Group = groupId };
        public static LabelGroup Unknown(int groupId = 0) => new LabelGroup(Array.Empty<int>(), GroupKind.Unknown) { Group = groupId };

        public UnorderedNTuple<int> ToTuple() => new UnorderedNTuple<int>(Children);

        public override string ToString()
        {
            return $"{Kind.ToString()[0]}{Group}";
        }

        public override bool Equals(object? obj)
        {
            return obj is LabelGroup group && Equals(group);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Group);
        }

        public bool Equals([AllowNull] LabelGroup other)
        {
            if (other! == null!)
            {
                return false;
            }


            return Children.OrderBy(_ => _).SequenceEqual(other.Children.OrderBy(_ => _)) &&
                   Kind == other.Kind &&
                   Group == other.Group;
        }

        public IReadOnlyList<int> Children { get; }

        public GroupKind Kind { get; }

        public int Group { get; set; }

        public LabelGroup(IReadOnlyList<int> children, GroupKind kind)
        {
            Children = children ?? throw new ArgumentNullException(nameof(children));
            Kind = kind;
        }

        public static bool operator ==(LabelGroup left, LabelGroup right)
        {
            return EqualityComparer<LabelGroup>.Default.Equals(left, right);
        }

        public static bool operator !=(LabelGroup left, LabelGroup right)
        {
            return !(left == right);
        }
    }

    public enum GroupKind
    {
        Unknown,
        Edge,
        Knot,
        Series,
        Parallel
    }


    struct Input
    {
        
        public int From { get; }
        public int To { get; }
        public int Group { get; }

        public GroupKind Kind { get; }

        public LabelGroup Labels { get; }

        public Input(int from, int to, int group)
        {
            From = from;
            To = to;
            Group = group;
            Kind = GroupKind.Edge;
            Labels = LabelGroup.Edge();
            Labels.Group = group;
        }

        public Input(int from, int to, int group, LabelGroup labelGroup)
        {
            From = from;
            To = to;
            Group = group;
            Kind = labelGroup.Kind;
            Labels = labelGroup;
            Labels.Group = group;
        }

        public static implicit operator Input((int from, int to) tuple)
        {
            return new Input(tuple.from, tuple.to, 0);
        }

        public static implicit operator Input((int from, int to, int group) tuple)
        {
            return new Input(tuple.from, tuple.to, tuple.group);
        }

        public static implicit operator Input((int from, int to, int group, LabelGroup kind) tuple)
        {
            return new Input(tuple.from, tuple.to, tuple.group, tuple.kind);
        }
    }

    [Flags]
    public enum ImplementationVersions
    {
        Latest = 1 << 0,
        V03 = 1 << 1,

        All = -1
    }

    class Pattarn
    {
        public string Id { get; }
        public IReadOnlyList<Node> Nodes { get; }
        public IReadOnlyList<Edge> Edges { get; }

        public IReadOnlyList<Node> StartNodes { get;  }
        public IReadOnlyList<Node>? EndNodes { get; }

        public IEnumerable<(Node fromNode, Edge edge, Node toNode)> EdgeWithNodePair => Edges.Select(e => (e.FromNode, e, e.ToNode));

        public IReadOnlyDictionary<Edge, LabelGroup> EdgeDic { get; }
        public IReadOnlyDictionary<(Node startNode, int subgroup, Node endNode), LabelGroup> SerDic { get; }
        public IReadOnlyDictionary<(Node startNode, Node endNode), LabelGroup> ParDic { get; }
        public IReadOnlyDictionary<(Node startNode, UnorderedNTuple<int> subgroups, Node endNode), LabelGroup> KnotDic { get; }

        public ImplementationVersions Version { get; }

        public Pattarn(int id, ImplementationVersions version, IReadOnlyList<Node> nodes,
            IReadOnlyList<Node> startNodes, IReadOnlyList<Node>? endNodes,
            IReadOnlyDictionary<Edge, LabelGroup> edgeDic,
            IReadOnlyDictionary<(Node startNode, int subgroup, Node endNode), LabelGroup> serDic,
            IReadOnlyDictionary<(Node startNode, Node endNode), LabelGroup> parDic,
            IReadOnlyDictionary<(Node startNode, UnorderedNTuple<int> subgroups, Node endNode), LabelGroup> knotDic)
        {
            Id = $"{id}-{version}";
            Version = version;
            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            StartNodes = startNodes ?? throw new ArgumentNullException(nameof(startNodes));
            EndNodes = endNodes ?? throw new ArgumentNullException(nameof(endNodes));
            EdgeDic = edgeDic ?? throw new ArgumentNullException(nameof(edgeDic));
            SerDic = serDic ?? throw new ArgumentNullException(nameof(serDic));
            ParDic = parDic ?? throw new ArgumentNullException(nameof(parDic));
            KnotDic = knotDic ?? throw new ArgumentNullException(nameof(knotDic));
            Edges = EdgeDic.Keys.ToList();
        }


        public static IEnumerable<Pattarn> Generate(IEnumerable<Input> inputs, int id, ImplementationVersions? versionFilter = ImplementationVersions.Latest)
        {
            inputs = inputs.Where(i => i.Kind != GroupKind.Unknown);

            var rawEdgeList = inputs.Where(input => input.Kind == GroupKind.Edge).ToList();
            var rawSubgraphList = inputs.Where(input => input.Kind != GroupKind.Edge && input.Kind != GroupKind.Unknown).ToList();
            var nodeIdSeq = rawEdgeList.Select(e => Math.Max(e.From, e.To)).Max() + 1;
            var groupIdSeq = rawEdgeList.Select(e => e.Group).Min() - 1;
            var edgeIdSeq = 0;
            var nodes = rawEdgeList.SelectMany(e => new[] { e.From, e.To }).Distinct().ToDictionary(n => n, n => new Node(n));
            var edgeDic = rawEdgeList.ToDictionary(e => new Edge(nodes[e.From], nodes[e.To], ++edgeIdSeq), e => e.Labels);
            
            var parDic = rawSubgraphList.Where(e => e.Kind == GroupKind.Parallel).ToDictionary(e => (startNode: nodes[e.From], endNode: nodes[e.To]), e => e.Labels);
            var knotDic = rawSubgraphList.Where(e => e.Kind == GroupKind.Knot).ToDictionary(e => (startNode: nodes[e.From], subgroups: e.Labels.ToTuple(), endNode: nodes[e.To]), e => e.Labels);

            var serDic = rawSubgraphList.Where(e => e.Kind == GroupKind.Series).ToDictionary(e => (startNode: nodes[e.From], sub: e.Labels.Children[0], endNode: nodes[e.To]), e => e.Labels);

            var startNodes = nodes.Values.Except(edgeDic.Keys.Select(e => e.ToNode)).ToList();
            var endNodes = nodes.Values.Except(edgeDic.Keys.Select(e => e.FromNode)).ToList();

            foreach(var ver in Enum.GetValues(typeof(ImplementationVersions)).OfType<ImplementationVersions>().Except(new[] { ImplementationVersions.All }))
            {
                if (versionFilter == null || (ver & versionFilter.Value) != 0)
                {
                    yield return new Pattarn(id, ver, nodes.Values.ToList(), startNodes, endNodes, edgeDic, serDic, parDic, knotDic);
                }
            }
            
        }
    }
}
