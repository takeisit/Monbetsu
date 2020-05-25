using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Monbetsu.Test
{
    class Graph
    {
        public IReadOnlyList<Node> Nodes { get; }
        public IReadOnlyList<Edge> Edges { get; }

        public Graph(IReadOnlyList<Node> nodes, IReadOnlyList<Edge> edges)
        {
            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            Edges = edges ?? throw new ArgumentNullException(nameof(edges));
        }
    }

    class Node : IComparable<Node>
    {
        public int Id { get; }

        public Node(int id)
        {
            Id = id;
        }

        public override string ToString() => Id.ToString();

        public int CompareTo([AllowNull] Node other) => Id.CompareTo(other!.Id);
    }

    class Edge
    {
        public Node FromNode { get; }
        public Node ToNode { get; }

        public int Id { get; }
        public Edge(Node fromNode, Node toNode, int id = default)
        {
            FromNode = fromNode ?? throw new ArgumentNullException(nameof(fromNode));
            ToNode = toNode ?? throw new ArgumentNullException(nameof(toNode));
            Id = id;
        }

        public override string ToString() => $"{FromNode} -> {ToNode} {(Id != 0 ? $"id={Id}" : "")}";
    }

    class Label
    {
        public Edge? BaseEdge { get; set; } = default;
        public (Node startNode, Node endNode)? BaseSubgraph { get; set; } = default;

        public GroupKind Kind { get; set; }

        public override string ToString()
        {
            if (BaseEdge != null)
            {
                return BaseEdge.ToString();
            }
            else if (BaseSubgraph != null)
            {
                return $"({BaseSubgraph.Value.startNode} -> {BaseSubgraph.Value.endNode})";
            }
            return "";
        }
    }

    class RecordLabel : Label
    {
        public string LabelText { get; set; } = "";

        public override string ToString() => LabelText;
    }

    class TestGraphEnvironment : Monbetsu.MonbetsuClassifier<Graph, Node, Edge, Label>, ITestGraphEnvironment
    {
        protected virtual Label CreateLabelFromEdge(Graph graph, Node fromNode, Edge edge, Node toNode)
        {
            return new Label { BaseEdge = edge, Kind = GroupKind.Edge };
        }

        protected virtual Label CreateLabelFromSubgraph(Graph graph, Node fromNode, Node toNode)
        {
            return new Label { BaseSubgraph = (fromNode, toNode) };
        }

        protected override IEnumerable<(Edge edge, Node toNode)> GetOutflows(Graph graph, Node fromNode)
            => graph.Edges.Where(e => e.FromNode == fromNode).Select(e => (e, e.ToNode));

        public void Classify(Graph graph, IEnumerable<Node> startNodes, EdgeLabeler<Graph, Node, Edge, Label>? labelEdge, SeriesSubgraphLabeler<Graph, Node, Label>? labelSeries, ParallelSubgraphLabeler<Graph, Node, Label>? labelParallel, KnotSubgraphLabeler<Graph, Node, Label>? labelKnot, CancellationToken cancellationToken = default)
        {
            Classify(graph, startNodes, GetOutflows, CreateLabelFromEdge, CreateLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }
    }

    class RecordGraphEnvironment : TestGraphEnvironment
    {
        private int labelCounter;
        protected override Label CreateLabelFromEdge(Graph graph, Node fromNode, Edge edge, Node toNode)
        {
            return new RecordLabel { BaseEdge = edge, Kind = GroupKind.Edge, LabelText = $"L{labelCounter++}" };
        }

        protected override Label CreateLabelFromSubgraph(Graph graph, Node fromNode, Node toNode)
        {
            return new RecordLabel { BaseSubgraph = (fromNode, toNode), LabelText = $"L{labelCounter++}" };
        }
    }

    interface ITestGraphEnvironment
    {
        void Classify(Graph graph, IEnumerable<Node> startNodes, EdgeLabeler<Graph, Node, Edge, Label>? labelEdge, SeriesSubgraphLabeler<Graph, Node, Label>? labelSeries, ParallelSubgraphLabeler<Graph, Node, Label>? labelParallel, KnotSubgraphLabeler<Graph, Node, Label>? labelKnot, CancellationToken cancellationToken = default);
    }

    namespace v01
    {
        class TestGraphEnvironment : Monbetsu.x01.GraphEnvironment<Graph, Node, Edge, Label>
        {
            protected override Label CreateLabelFromEdge(Graph graph, Node fromNode, Edge edge, Node toNode)
            {
                return new Label { BaseEdge = edge, Kind = GroupKind.Edge };
            }

            protected override Label CreateLabelFromSubgraph(Graph graph, Node fromNode, Node toNode)
            {
                return new Label { BaseSubgraph = (fromNode, toNode) };
            }

            protected override IEnumerable<(Edge edge, Node fromNode)> GetInflows(Graph graph, Node toNode)
                => graph.Edges.Where(e => e.ToNode == toNode).Select(e => (e, e.FromNode));

            protected override IEnumerable<(Edge edge, Node toNode)> GetOutflows(Graph graph, Node fromNode)
                => graph.Edges.Where(e => e.FromNode == fromNode).Select(e => (e, e.ToNode));
        }

        class RecordGraphEnvironment : TestGraphEnvironment
        {
            private int labelCounter;
            protected override Label CreateLabelFromEdge(Graph graph, Node fromNode, Edge edge, Node toNode)
            {
                return new RecordLabel { BaseEdge = edge, Kind = GroupKind.Edge, LabelText = $"L{labelCounter++}" };
            }

            protected override Label CreateLabelFromSubgraph(Graph graph, Node fromNode, Node toNode)
            {
                return new RecordLabel { BaseSubgraph = (fromNode, toNode), LabelText = $"L{labelCounter++}" };
            }
        }
    }

    namespace v03
    {
        class TestGraphEnvironment : Monbetsu.v03.MonbetsuClassifier<Graph, Node, Edge, Label>, ITestGraphEnvironment
        {
            protected virtual Label CreateLabelFromEdge(Graph graph, Node fromNode, Edge edge, Node toNode)
            {
                return new Label { BaseEdge = edge, Kind = GroupKind.Edge };
            }

            protected virtual Label CreateLabelFromSubgraph(Graph graph, Node fromNode, Node toNode)
            {
                return new Label { BaseSubgraph = (fromNode, toNode) };
            }

            protected override IEnumerable<(Edge edge, Node toNode)> GetOutflows(Graph graph, Node fromNode)
                => graph.Edges.Where(e => e.FromNode == fromNode).Select(e => (e, e.ToNode));

            void ITestGraphEnvironment.Classify(Graph graph, IEnumerable<Node> startNodes, EdgeLabeler<Graph, Node, Edge, Label>? labelEdge, SeriesSubgraphLabeler<Graph, Node, Label>? labelSeries, ParallelSubgraphLabeler<Graph, Node, Label>? labelParallel, KnotSubgraphLabeler<Graph, Node, Label>? labelKnot, CancellationToken cancellationToken)
            {
                Classify(graph, startNodes, GetOutflows, CreateLabelFromEdge, CreateLabelFromSubgraph,
                    (g, f, e, t, l) => labelEdge?.Invoke(g, f, e, t, l),
                    (g, f, e, t, l) => labelSeries?.Invoke(g, f, e, t, l),
                    (g, f, e, t, l) => labelParallel?.Invoke(g, f, e, t, l),
                    (g, f, e, t, l) => labelKnot?.Invoke(g, f, e, t, l),
                    cancellationToken);
            }
        }

        class RecordGraphEnvironment : TestGraphEnvironment
        {
            private int labelCounter;
            protected override Label CreateLabelFromEdge(Graph graph, Node fromNode, Edge edge, Node toNode)
            {
                return new RecordLabel { BaseEdge = edge, Kind = GroupKind.Edge, LabelText = $"L{labelCounter++}" };
            }

            protected override Label CreateLabelFromSubgraph(Graph graph, Node fromNode, Node toNode)
            {
                return new RecordLabel { BaseSubgraph = (fromNode, toNode), LabelText = $"L{labelCounter++}" };
            }
        }
    }
}
