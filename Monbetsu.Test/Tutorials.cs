using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;


namespace Monbetsu.Tutorials.Tutorial1
{
    using Monbetsu;

    class Node
    {
        public int Id { get; set; }
        public List<Edge> Ins { get; set; } = new List<Edge>();
        public List<Edge> Outs { get; set; } = new List<Edge>();

        public override string ToString() => $"{Id}";
    }

    class Edge
    {
        public Node FromNode { get; }
        public Node ToNode { get; }

        public Edge(Node fromNode, Node toNode)
        {
            FromNode = fromNode ?? throw new ArgumentNullException(nameof(fromNode));
            ToNode = toNode ?? throw new ArgumentNullException(nameof(toNode));
        }

        public override string ToString() => $"({FromNode}, {ToNode})";
    }

    class Label
    {
        public string Name { get; }

        public Label(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }

    class GraphEnv
    {
        private static Label CreateLabelFromEdge(Node fromNode, Edge edge, Node toNode)
            => new Label($"Label:{fromNode}-{edge}->{toNode}");

        private static Label CreateLabelFromSubgraph(Node startNode, Node endNode)
            => new Label($"Label:{startNode}-..->{endNode}");

        private static IEnumerable<(Edge edge, Node toNode)> GetOutflows(Node fromNode)
            => fromNode.Outs.Select(e => (e, e.ToNode));

        
        private List<Node> SetUpDAG()
        {
            var nodes = Enumerable.Range(0, 7).Select(n => new Node { Id = n }).ToList();
            var edges = new[]
            {
                (from: 0, to: 1),
                (from: 1, to: 2),
                (from: 1, to: 3),
                (from: 2, to: 3),
                (from: 2, to: 4),
                (from: 3, to: 4),
                (from: 4, to: 6),
                (from: 0, to: 5),
                (from: 5, to: 6),
                (from: 0, to: 6)
            }.Select(e => new Edge(nodes[e.from], nodes[e.to])).ToList();

            foreach (var n in nodes)
            {
                n.Ins = edges.Where(e => e.ToNode == n).ToList();
                n.Outs = edges.Where(e => e.FromNode == n).ToList();
            }
            return nodes;
        }

        [Test]
        public void RunDelegateBased()
        {
            var nodes = SetUpDAG();
            MonbetsuClassifier.Classify<Node, Edge, Label>(
                // start nodes of the DAG
                new[] { nodes[0] }, 
                // callback to get forward edges and to-nodes of a from-node.
                GetOutflows,
                // callback to create a label object from a edge.
                CreateLabelFromEdge,
                // callback to create a label object from a subgraph.
                CreateLabelFromSubgraph,
                (Node fromNode, Edge edge, Node toNode, Label label) =>
                {
                    // This callback is called for each edges labeling.
                    TestContext.WriteLine($"{edge} is labeled as {label}.");
                },
                (Node startNode, Label sublabel, Node endNode, Label label) =>
                {
                    // This callback is called for each Serieses labeling.
                    TestContext.WriteLine($"series subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabel: {sublabel}");
                },
                (Node startNode, IEnumerable<Label> sublabels, Node endNode, Label label) =>
                {
                    // This callback is called for each Parallels labeling.
                    TestContext.WriteLine($"parallel subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                },
                (Node startNode, IEnumerable<Label> sublabels, Node endNode, Label label) =>
                {
                    // This callback is called for each Knots labeling.
                    TestContext.WriteLine($"knot subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                });
        }

        [Test]
        public void RunInterfaceBasedWithCyclicCheck()
        {
            var nodes = SetUpDAG();

            // Interfaces have methods correspond to delegates for delegate-based one, and grouped by the kind of them. 
            var structure = new GraphStructure();
            var factory = new LabelFactory();
            var labeler = new Labeler();

            try
            {
                MonbetsuClassifier.Classify(new[] { nodes[0] },
                    structure,
                    factory,
                    labeler);
            }
            catch(CyclicException<Node, Edge> exception)
            {
                // When the input graph have cycle, this exception is thrown.
                TestContext.WriteLine(exception);
            }

            foreach(var record in labeler.Records)
            {
                TestContext.WriteLine(record);
            }
        }

        [Test]
        public void RunInterfaceBasedAsOtherVariations()
        {
            var nodes = SetUpDAG();

            var structure = new GraphStructure();
            var factory = new LabelFactory();
            var labeler = new Labeler();

            MonbetsuClassifier.Integratedly.Classify(new[] { nodes[0] },
                structure,
                factory,
                labeler);

            TestContext.WriteLine("result of Integrated ");
            foreach (var record in labeler.Records)
            {
                TestContext.WriteLine(record);
            }

            labeler.Records.Clear();

            MonbetsuClassifier.Uniquely.Classify(new[] { nodes[0] },
                structure,
                factory,
                labeler);

            TestContext.WriteLine("result of Unique");
            foreach (var record in labeler.Records)
            {
                TestContext.WriteLine(record);
            }
        }

        class GraphStructure : IGraphStructure<Node, Edge>
        {
            public IEnumerable<(Edge, Node)> GetOutflows(Node node) => GraphEnv.GetOutflows(node);
        }

        class LabelFactory : ILabelFactory<Node, Edge, Label>
        {
            private int idSeq = 0;

            public Label CreateFromEdge(Node fromNode, Edge edge, Node toNode) => CreateFromSubgraph(fromNode, toNode);
            public Label CreateFromSubgraph(Node startNode, Node endNode) => new Label($"Label:{idSeq++}");
        }

        class Labeler : ILabeler<Node, Edge, Label>, IUniquelyLabeler<Node, Edge, Label>
        {
            public List<(Node from, Node to, Label label, string kind, string subs)> Records { get; } = new List<(Node from, Node to, Label label, string kind, string subs)>();

            private void Record(Node from, Node to, Label label, string kind, string subs) => Records.Add((from, to, label, kind, subs));
            public void LabelEdge(Node fromNode, Edge edge, Node toNode, Label label) => Record(fromNode, toNode, label, "edge", edge.ToString());
            public void LabelKnotSubgraph(Node startNode, IEnumerable<Label> sublebels, Node endNode, Label label) => Record(startNode, endNode, label, "knot", string.Join(", ", sublebels));
            public void LabelParallelSubgraph(Node startNode, IEnumerable<Label> sublebels, Node endNode, Label label) => Record(startNode, endNode, label, "parallel", string.Join(", ", sublebels));

            // called only when variation of Series is Default or Integrated.
            public void LabelSeriesSubgraph(Node startNode, Label sublebel, Node endNode, Label label) => Record(startNode, endNode, label, "series", sublebel.ToString());


            // called only when variation of Series is Unique.
            public void LabelSeriesSubgraph(Node startNode, IEnumerable<Label> sublebels, Node endNode, Label label) => Record(startNode, endNode, label, "series", string.Join(", ", sublebels));
        }
    }
}

namespace Monbetsu.Tutorials.Tutorial2
{
    using Monbetsu;

    class Graph
    {
        public List<(int from, int to)> Edges { get; set; } = new List<(int from, int to)>();
    
        public IEnumerable<((int from, int to) edge, int toNode)> GetOutflows(int fromNode)
            => Edges.Where(edge => edge.from == fromNode).Select(edge => (edge, edge.to));
    }

    class GraphEnv
    {
        private Graph SetUpDAG()
        {
            var graph = new Graph();
            graph.Edges.AddRange(new[]
            {
                (from: 0, to: 1),
                (from: 1, to: 2),
                (from: 1, to: 3),
                (from: 2, to: 3),
                (from: 2, to: 4),
                (from: 3, to: 4),
                (from: 4, to: 6),
                (from: 0, to: 5),
                (from: 5, to: 6),
                (from: 0, to: 6)
            });
            return graph;
        }

        [Test]
        public void RunWithGraphArgument()
        {
            var graph = SetUpDAG();
            MonbetsuClassifier.Classify<Graph, int, (int from, int to), string>(
                // a DAG
                graph,
                // start nodes of the DAG
                new[] { 0 },
                // callback to get forward edges and to-nodes of a from-node.
                (g, n) => g.GetOutflows(n),
                // callback to create a label object from a edge.
                (Graph graph, int fromNode, (int from, int to) edge, int toNode) => $"Label:{edge}",
                // callback to create a label object from a subgraph.
                (Graph graph, int startNode, int endNode) => $"Label:{startNode}-..->{endNode}",
                (Graph graph, int from, (int from, int to) edge, int to, string label) =>
                {
                    // This callback is called for each edge labeling.
                    TestContext.WriteLine($"{edge} is labeled as {label}.");
                },
                (Graph graph, int startNode, string sublabel, int endNode, string label) =>
                {
                    // This callback is called for each series labeling.
                    TestContext.WriteLine($"series subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabel: {sublabel}");
                },
                (Graph graph, int startNode, IEnumerable<string> sublabels, int endNode, string label) =>
                {
                    // This callback is called for each parallel labeling.
                    TestContext.WriteLine($"parallel subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                },
                (Graph graph, int startNode, IEnumerable<string> sublabels, int endNode, string label) =>
                {
                    // This callback is called for each knot labeling.
                    TestContext.WriteLine($"knot subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                });
        }

        [Test]
        public void RunWithGraphArgumentAsUnique()
        {
            var graph = SetUpDAG();
            MonbetsuClassifier.Uniquely.Classify<Graph, int, (int from, int to), string>(graph,
                new[] { 0 },
                (g, n) => g.GetOutflows(n),
                (Graph graph, int fromNode, (int from, int to) edge, int toNode) => $"Label:{edge}",
                (Graph graph, int startNode, int endNode) => $"Label:{startNode}-..->{endNode}",
                (Graph graph, int from, (int from, int to) edge, int to, string label) =>
                {
                    TestContext.WriteLine($"{edge} is labeled as {label}.");
                },
                (Graph graph, int startNode, IEnumerable<string> sublabels, int endNode, string label) =>
                {
                    TestContext.WriteLine($"series subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                },
                (Graph graph, int startNode, IEnumerable<string> sublabels, int endNode, string label) =>
                {
                    TestContext.WriteLine($"parallel subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                },
                (Graph graph, int startNode, IEnumerable<string> sublabels, int endNode, string label) =>
                {
                    TestContext.WriteLine($"knot subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                });
        }

        [Test]
        public void RunWithGraphArgumentAsIntegrated()
        {
            var graph = SetUpDAG();
            MonbetsuClassifier.Integratedly.Classify<Graph, int, (int from, int to), string>(graph,
                new[] { 0 },
                (g, n) => g.GetOutflows(n),
                (Graph graph, int fromNode, (int from, int to) edge, int toNode) => $"Label:{edge}",
                (Graph graph, int startNode, int endNode) => $"Label:{startNode}-..->{endNode}",
                (Graph graph, int from, (int from, int to) edge, int to, string label) =>
                {
                    TestContext.WriteLine($"{edge} is labeled as {label}.");
                },
                (Graph graph, int startNode, string sublabel, int endNode, string label) =>
                {
                    TestContext.WriteLine($"series subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabel: {sublabel}");
                },
                (Graph graph, int startNode, IEnumerable<string> sublabels, int endNode, string label) =>
                {
                    TestContext.WriteLine($"parallel subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                },
                (Graph graph, int startNode, IEnumerable<string> sublabels, int endNode, string label) =>
                {
                    TestContext.WriteLine($"knot subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    TestContext.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                });
        }
    }
}
