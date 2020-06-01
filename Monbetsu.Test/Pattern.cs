using System;
using System.Collections.Generic;
using System.Linq;
using Monbetsu.Test.Utils;

namespace Monbetsu.Test
{
    class Pattern
    {
        public string Id { get; }
        public IReadOnlyList<Node> Nodes { get; }
        public IReadOnlyList<Edge> Edges { get; }

        public IReadOnlyList<Node> StartNodes { get;  }
        public IReadOnlyList<Node>? EndNodes { get; }

        public IEnumerable<NodePair<Node, Edge>> EdgeWithNodePair => Edges.Select(e => new NodePair<Node, Edge>(e.FromNode, e, e.ToNode));

        public IReadOnlyDictionary<Edge, LabelGroup> EdgeDic { get; }
        public IReadOnlyDictionary<(Node startNode, int subgroup, Node endNode), LabelGroup> SerDic { get; }
        public IReadOnlyDictionary<(Node startNode, Node endNode), LabelGroup> ParDic { get; }
        public IReadOnlyDictionary<(Node startNode, UnorderedNTuple<int> subgroups, Node endNode), LabelGroup> KnotDic { get; }

        public IReadOnlyDictionary<NodePair<Node, Edge>, LabelGroup> EdgeDic2 => EdgeDic.ToDictionary(kv => new NodePair<Node, Edge>(kv.Key.FromNode, kv.Key, kv.Key.ToNode), kv => kv.Value);
        public IReadOnlyDictionary<NodePair<Node, UnorderedNTuple<int>>, LabelGroup> SerDic2 { get; }
        public IReadOnlyDictionary<NodePair<Node, UnorderedNTuple<int>>, LabelGroup> ParDic2 { get; }
        public IReadOnlyDictionary<NodePair<Node, UnorderedNTuple<int>>, LabelGroup> KnotDic2 { get; }

        public IReadOnlyDictionary<LabelGroup, LabelGroup> LabelParents { get; }
        public ILookup<LabelGroup, LabelGroup> LabelChildren { get; }

        public ImplementationVersions Version { get; }
        public ClassificationVariation Variation { get; }

        public int UnexpectedLabelGroupSeq =>
                    Math.Min(
                        Math.Min(
                            EdgeDic2.Values.Select(lg => lg.Group).Min(),
                            KnotDic2.Values.Select(lg => lg.Group).DefaultIfEmpty(0).Min()
                        ),
                        Math.Min(
                            SerDic2.Values.Select(lg => lg.Group).DefaultIfEmpty(0).Min(),
                            ParDic2.Values.Select(lg => lg.Group).DefaultIfEmpty(0).Min()
                        )
                    ) - 1;



        public Pattern(int id, ImplementationVersions version, ClassificationVariation variation,
            IReadOnlyList<Node> nodes,
            IReadOnlyList<Node> startNodes, IReadOnlyList<Node>? endNodes,
            IReadOnlyDictionary<Edge, LabelGroup> edgeDic,
            IReadOnlyDictionary<(Node startNode, int subgroup, Node endNode), LabelGroup> serDic,
            IReadOnlyDictionary<(Node startNode, Node endNode), LabelGroup> parDic,
            IReadOnlyDictionary<(Node startNode, UnorderedNTuple<int> subgroups, Node endNode), LabelGroup> knotDic,
            IReadOnlyDictionary<NodePair<Node, UnorderedNTuple<int>>, LabelGroup> serDic2,
            IReadOnlyDictionary<NodePair<Node, UnorderedNTuple<int>>, LabelGroup> parDic2,
            IReadOnlyDictionary<NodePair<Node, UnorderedNTuple<int>>, LabelGroup> knotDic2,
            IReadOnlyDictionary<LabelGroup, LabelGroup> labelParents)
        {
            Id = $"{id}-{version}-{variation}";
            Version = version;
            Variation = variation;
            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            StartNodes = startNodes ?? throw new ArgumentNullException(nameof(startNodes));
            EndNodes = endNodes ?? throw new ArgumentNullException(nameof(endNodes));
            EdgeDic = edgeDic ?? throw new ArgumentNullException(nameof(edgeDic));
            SerDic = serDic ?? throw new ArgumentNullException(nameof(serDic));
            ParDic = parDic ?? throw new ArgumentNullException(nameof(parDic));
            KnotDic = knotDic ?? throw new ArgumentNullException(nameof(knotDic));
            SerDic2 = serDic2 ?? throw new ArgumentNullException(nameof(serDic2));
            ParDic2 = parDic2 ?? throw new ArgumentNullException(nameof(parDic2));
            KnotDic2 = knotDic2 ?? throw new ArgumentNullException(nameof(knotDic2));
            LabelParents = labelParents ?? throw new ArgumentNullException(nameof(labelParents));
            LabelChildren = labelParents.ToLookup(kv => kv.Value, kv => kv.Key);
            Edges = EdgeDic.Keys.ToList();
        }

        
        public Node Node(int id) => Nodes.Where(n => n.Id == id).First();

        public Edge Edge(int from, int to, int group) => EdgeDic2.Where(e => e.Key.StartNode.Id == from && e.Key.EndNode.Id == to && e.Value.Group == group).First().Key.Via;

        public static IEnumerable<Pattern> Generate(IEnumerable<Input> inputs, int id, ImplementationVersions? versionFilter = ImplementationVersions.Latest, ClassificationVariation variation = ClassificationVariation.Default)
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

            var serDic2 = rawSubgraphList.Where(e => e.Kind == GroupKind.Series).ToDictionary(e => new NodePair<Node, UnorderedNTuple<int>>(startNode: nodes[e.From], via: e.Labels.ToTuple(), endNode: nodes[e.To]), e => e.Labels);
            var parDic2 = rawSubgraphList.Where(e => e.Kind == GroupKind.Parallel).ToDictionary(e => new NodePair<Node, UnorderedNTuple<int>>(startNode: nodes[e.From], via: e.Labels.ToTuple(), endNode: nodes[e.To]), e => e.Labels);
            var knotDic2 = rawSubgraphList.Where(e => e.Kind == GroupKind.Knot).ToDictionary(e => new NodePair<Node, UnorderedNTuple<int>>(startNode: nodes[e.From], via: e.Labels.ToTuple(), endNode: nodes[e.To]), e => e.Labels);

            var knotDic = rawSubgraphList.Where(e => e.Kind == GroupKind.Knot).ToDictionary(e => (startNode: nodes[e.From], subgroups: e.Labels.ToTuple(), endNode: nodes[e.To]), e => e.Labels);

            var serDic = rawSubgraphList.Where(e => e.Kind == GroupKind.Series).ToDictionary(e => (startNode: nodes[e.From], sub: e.Labels.Children[0], endNode: nodes[e.To]), e => e.Labels);

            var startNodes = nodes.Values.Except(edgeDic.Keys.Select(e => e.ToNode)).ToList();
            var endNodes = nodes.Values.Except(edgeDic.Keys.Select(e => e.FromNode)).ToList();

            var labelGroupLu = edgeDic.Values.Concat(parDic2.Values).Concat(serDic2.Values).Concat(knotDic.Values).ToLookup(lg => lg.Group);
            var labelParents = new Dictionary<LabelGroup, LabelGroup>();

            foreach(var labelGroups in labelGroupLu)
            {
                var seriesLabel = labelGroups.SingleOrDefault(lg => lg.Kind == GroupKind.Series);
                if (seriesLabel != null && labelGroups.Any(_ => _ != seriesLabel))
                {
                    foreach (var child in labelGroups.Where(_ => _ != seriesLabel))
                    {
                        labelParents[child] = seriesLabel;

                        foreach (var grandchild in child.Children.SelectMany(gc => labelGroupLu[gc]))
                        {
                            labelParents[grandchild] = child;
                        }
                    }
                }
                else
                {
                    var childrenLu = labelGroups.First().Children.SelectMany(c => labelGroupLu[c]).ToLookup(lg => lg.Group);
                    foreach (var children in childrenLu)
                    {
                        seriesLabel = children.SingleOrDefault(lg => lg.Kind == GroupKind.Series);
                        if (seriesLabel != null && labelGroups.Any(_ => _ != seriesLabel))
                        {
                            labelParents[seriesLabel] = labelGroups.First();
                        }
                        else
                        {
                            foreach (var child in children)
                            {
                                labelParents[child] = labelGroups.First();
                            }
                        }
                    }
                }
            }

            foreach(var ver in Enum.GetValues(typeof(ImplementationVersions)).OfType<ImplementationVersions>().Except(new[] { ImplementationVersions.All }))
            {
                if (versionFilter == null || (ver & versionFilter.Value) != 0)
                {
                    yield return new Pattern(id, ver, variation, nodes.Values.ToList(), startNodes, endNodes, edgeDic, serDic, parDic, knotDic, serDic2, parDic2, knotDic2, labelParents);
                }
            }
            
        }
    }
}
