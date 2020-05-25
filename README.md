# Monbetsu #
Monbetsu is a classifier for edges/subgraphs in a DAG.
It makes hierarchical groups so that thier components (edge or subgraph) are connected with no branchs,
and labels them respectively.
The components of each groups share the same label.

For example, Monbetsu labels the edges/subgraphs in the below DAG  
![original graph](./docs/images/example01-original-graph.png)   
as the following:  
![labeled graph](./docs/images/example01-labeled-graph.png)   

The targets of labeling are as follows:

- Edge: each edges of the graph.
- Subgraphs: they have single start node, single end node, and multiple labeled edges/sub-subgraphs.
  - Series: their components share a label and sequentially connected. They are greedly grouped so that any of the componsents are not Series.  
    ![series](./docs/images/subgraph-series.png)   
  - Parallel: the labels of their components share a pair of start/end node. They are greedly grouped so that any of the componsents are not Parallel.  
    ![parallel](./docs/images/subgraph-parallel.png)   
  - Knot: otherwise.  
    ![knot](./docs/images/subgraph-knot.png)   

  The other kinds of subgraph which have multiple start/end nodes are not labeled.

The labeling is hierarchical and bottom-up (i.e. edges -> smaller subgraphs -> larger subgraphs).  
The following animation illustrates the steps of labeling.  
![steps](./docs/images/example01-steps.gif)   


## Installation ##
Copy [MonbetsuClassifier.cs](./Monbetsu/MonbetsuClassifier.cs) to your project. (Monbetsu is not in NuGet.)

#### Prerequisites ####
- C# 8.0 or above
- one of the following
  - .NET Standard  2.0 or above
  - .NET Core 2.0 or above
  - .NET Framework 4.7.1 or above
  
If your project uses the older version, some modifications (e.g. adding reference to System.ValueTuple) would be required.

## Usage ##

Call one of ``Classify`` methods declared in class ``MonbetsuClassifier``, ``MonbetsuClassifier`3``, or ``MonbetsuClassifier`4``.
The arguments of the overloads are combinations of:

- with or without Graph object
- interface-base or delegate-base


#### Example 1 ####
```csharp
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
            => new Label($"Label:{edge}");

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

        public void RunDelegateVersion()
        {
            var nodes = SetUpDAG();
            MonbetsuClassifier.Classify<Node, Edge, Label>(new[] { nodes[0] }, 
                GetOutflows,
                CreateLabelFromEdge,
                CreateLabelFromSubgraph,
                (Node fromNode, Edge edge, Node toNode, Label label) =>
                {
                    // This callback is called for each edge labeling.
                    Debug.WriteLine($"{edge} is labeled as {label}.");
                },
                (Node startNode, Label sublabel, Node endNode, Label label) =>
                {
                    // This callback is called for each series labeling.
                    Debug.WriteLine($"series subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    Debug.WriteLine($"  sublabel: {sublabel}");
                },
                (Node startNode, IEnumerable<Label> sublabels, Node endNode, Label label) =>
                {
                    // This callback is called for each parallel labeling.
                    Debug.WriteLine($"parallel subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    Debug.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                },
                (Node startNode, IEnumerable<Label> sublabels, Node endNode, Label label) =>
                {
                    // This callback is called for each knot labeling.
                    Debug.WriteLine($"knot subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    Debug.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                });
        }

        public void RunInterfaceVersion()
        {
            var nodes = SetUpDAG();

            var structure = new GraphStructure();
            var factory = new LabelFactory();
            var labeler = new Labeler();

            MonbetsuClassifier.Classify(new[] { nodes[0] },
                structure,
                factory,
                labeler);

            foreach(var record in labeler.Records)
            {
                Debug.WriteLine(record);
            }
        }

        class GraphStructure : IGraphStructure<Node, Edge>
        {
            public IEnumerable<(Edge, Node)> GetOutflows(Node node) => GraphEnv.GetOutflows(node);
        }

        class LabelFactory : ILabelFactory<Node, Edge, Label>
        {
            public Label CreateFromEdge(Node fromNode, Edge edge, Node toNode) => CreateFromSubgraph(fromNode, toNode);
            public Label CreateFromSubgraph(Node startNode, Node endNode) => new Label($"Label:{startNode}->{endNode}");
        }

        class Labeler : ILabeler<Node, Edge, Label>
        {
            public List<(Node from, Node to, Label label)> Records { get; } = new List<(Node from, Node to, Label label)>();

            private void Record(Node from, Node to, Label label) => Records.Add((from, to, label));
            public void LabelEdge(Node fromNode, Edge edge, Node toNode, Label label) => Record(fromNode, toNode, label);
            public void LabelKnotSubgraph(Node startNode, IEnumerable<Label> sublebels, Node endNode, Label label) => Record(startNode, endNode, label);
            public void LabelParallelSubgraph(Node startNode, IEnumerable<Label> sublebels, Node endNode, Label label) => Record(startNode, endNode, label);
            public void LabelSeriesSubgraph(Node startNode, Label sublebel, Node endNode, Label label) => Record(startNode, endNode, label);
        }
    }
}

```
#### Example 2 ####
```csharp
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

        public void RunWithGraphArgument()
        {
            var graph = SetUpDAG();
            MonbetsuClassifier.Classify<Graph, int, (int from, int to), string>(graph,
                new[] { 0 },
                (g, n) => g.GetOutflows(n),
                (Graph graph, int fromNode, (int from, int to) edge, int toNode) => $"LE:{edge}",
                (Graph graph, int startNode, int endNode) => $"LS:{startNode}-..->{endNode}",
                (Graph graph, int from, (int from, int to) edge, int to, string label) =>
                {
                    // This callback is called for each edge labeling.
                    Debug.WriteLine($"{edge} is labeled as {label}.");
                },
                (Graph graph, int startNode, string sublabel, int endNode, string label) =>
                {
                    // This callback is called for each series labeling.
                    Debug.WriteLine($"series subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    Debug.WriteLine($"  sublabel: {sublabel}");
                },
                (Graph graph, int startNode, IEnumerable<string> sublabels, int endNode, string label) =>
                {
                    // This callback is called for each parallel labeling.
                    Debug.WriteLine($"parallel subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    Debug.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                },
                (Graph graph, int startNode, IEnumerable<string> sublabels, int endNode, string label) =>
                {
                    // This callback is called for each knot labeling.
                    Debug.WriteLine($"knot subgraph ({startNode} -..-> {endNode}) is labeled as {label}.");
                    Debug.WriteLine($"  sublabels: {string.Join(", ", sublabels)}");
                });
        }
    }
}

```
## Demo ##
Online demo page implemented by Blazor is [here](https://takeisit.github.io/Monbetsu/).
