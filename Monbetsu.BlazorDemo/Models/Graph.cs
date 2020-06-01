using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Monbetsu.BlazorDemo.Models
{
    class Graph : MonbetsuClassifier<string, Edge, Label>
    {
        

        private static readonly Regex EdgeRegex = new Regex(@"\s*(\S+)\s*(->\s*(\S+)\s*)+");

        public static readonly Graph Empty = new Graph(Array.Empty<Edge>(), Array.Empty<string>());

        public IReadOnlyList<Edge> Edges { get; }

        public IReadOnlyList<string> Nodes { get; }

        public IReadOnlyList<string> StartNodes { get; }
        
        private readonly Dictionary<Edge, Label> edgeResults = new Dictionary<Edge, Label>();
        private readonly List<LabeledSubgraph> subgraphResults = new List<LabeledSubgraph>();

        public IReadOnlyDictionary<Edge, Label> EdgeResults => edgeResults;
        public IReadOnlyList<LabeledSubgraph> SubgraphResults => subgraphResults;

        private int labelSeq;

        public Graph(IReadOnlyList<Edge> edges, IReadOnlyCollection<string> startNodes)
        {
            Edges = edges ?? throw new ArgumentNullException(nameof(edges));
            Nodes = Edges.SelectMany(e => new[] { e.From, e.To }).Distinct().OrderBy(_ => _).ToList();

            StartNodes = startNodes.Count == 0 ? Nodes.Except(Edges.Select(e => e.To)).ToList() : startNodes.ToList();
        }

        public static Graph From(string code)
        {
            var (edges, startNodes) = Parse(code);
            return new Graph(edges, startNodes);
        }

        private static (List<Edge> edges, HashSet<string> startNodes) Parse(string code)
        {
            var startNodes = new HashSet<string>();
            var edges = new List<Edge>();
            var lines = code.Split('\n');
            
            var commentsSkipped = lines.Select(line => line.Split("//")[0]);

            foreach(var line in commentsSkipped)
            {
                var match = EdgeRegex.Match(line);
                if (match.Success)
                {
                    var from = match.Groups[1].Value;
                    if (from.StartsWith("^"))
                    {
                        from = from.Substring(1);
                        startNodes.Add(from);
                    }


                    foreach(var to in match.Groups[3].Captures.Select(cap => cap.Value))
                    {
                        string to2 = to;
                        if (to2.StartsWith("^"))
                        {
                            to2 = to.Substring(1);
                            startNodes.Add(to2);
                        }

                        edges.Add(new Edge(from, to2));
                        from = to2;
                    }
                }
            }

            return (edges, startNodes);
        }

        protected override IEnumerable<(Edge edge, string toNode)> GetOutflows(string fromNode)
            => Edges.Where(edge => edge.From == fromNode).Select(edge => (edge, edge.To));

        private Label CreateLabelFromEdge(string fromNode, Edge edge, string toNode)
            => new Label(labelSeq++);

        private Label CreateLabelFromSubgraph(string startNode, string endNode)
            => new Label(labelSeq++);

        public void Classify(Variation variation = Variation.Default, CancellationToken cancellationToken = default)
        {
            labelSeq = 0;
            edgeResults.Clear();
            subgraphResults.Clear();

            switch(variation)
            {
                case Variation.Unique:
                    ClassifyUniquely(StartNodes,
                        CreateLabelFromEdge,
                        CreateLabelFromSubgraph,
                        (string fromNode, Edge edge, string toNode, Label label) =>
                        {
                            edgeResults[edge] = label;
                        },
                        (string startNode, IEnumerable<Label> sublabels, string endNode, Label label) =>
                        {
                            subgraphResults.Add(new LabeledSubgraph(startNode, sublabels.ToList(), endNode, label, SubgraphKind.Series));
                        },
                        (string startNode, IEnumerable<Label> sublabels, string endNode, Label label) =>
                        {
                            subgraphResults.Add(new LabeledSubgraph(startNode, sublabels.ToList(), endNode, label, SubgraphKind.Parallel));
                        },
                        (string startNode, IEnumerable<Label> sublabels, string endNode, Label label) =>
                        {
                            subgraphResults.Add(new LabeledSubgraph(startNode, sublabels.ToList(), endNode, label, SubgraphKind.Knot));
                        }, cancellationToken);
                    break;
                case Variation.Integrated:
                    ClassifyIntegratedly(StartNodes,
                        CreateLabelFromEdge,
                        CreateLabelFromSubgraph,
                        (string fromNode, Edge edge, string toNode, Label label) =>
                        {
                            edgeResults[edge] = label;
                        },
                        (string startNode, Label sublabel, string endNode, Label label) =>
                        {
                            subgraphResults.Add(new LabeledSubgraph(startNode, new List<Label> { sublabel }, endNode, label, SubgraphKind.Series));
                        },
                        (string startNode, IEnumerable<Label> sublabels, string endNode, Label label) =>
                        {
                            subgraphResults.Add(new LabeledSubgraph(startNode, sublabels.ToList(), endNode, label, SubgraphKind.Parallel));
                        },
                        (string startNode, IEnumerable<Label> sublabels, string endNode, Label label) =>
                        {
                            subgraphResults.Add(new LabeledSubgraph(startNode, sublabels.ToList(), endNode, label, SubgraphKind.Knot));
                        }, cancellationToken);
                    break;
                default:
                    Classify(StartNodes,
                        CreateLabelFromEdge,
                        CreateLabelFromSubgraph,
                        (string fromNode, Edge edge, string toNode, Label label) =>
                        {
                            edgeResults[edge] = label;
                        },
                        (string startNode, Label sublabel, string endNode, Label label) =>
                        {
                            subgraphResults.Add(new LabeledSubgraph(startNode, new List<Label> { sublabel }, endNode, label, SubgraphKind.Series));
                        },
                        (string startNode, IEnumerable<Label> sublabels, string endNode, Label label) =>
                        {
                            subgraphResults.Add(new LabeledSubgraph(startNode, sublabels.ToList(), endNode, label, SubgraphKind.Parallel));
                        },
                        (string startNode, IEnumerable<Label> sublabels, string endNode, Label label) =>
                        {
                            subgraphResults.Add(new LabeledSubgraph(startNode, sublabels.ToList(), endNode, label, SubgraphKind.Knot));
                        }, cancellationToken);
                    break;
            }
            
        }
    }

    public class Edge
    {
        public string From { get; }
        public string To { get; }

        public Edge(string from, string to)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
        }
    }

    public enum SubgraphKind
    {
        Edge,
        Series,
        Parallel,
        Knot
    }

    public enum Variation
    {
        Default,
        Unique,
        Integrated
    }

    public class Label
    {
        public int Id { get; }

        public Label(int id)
        {
            Id = id;
        }

        public override string ToString() => $"L{Id}";
    }

    class LabeledSubgraph
    {
        public string StartNode { get;}

        public IReadOnlyList<Label> Sublabels { get; }
        public string EndNode { get; }

        public Label Label { get; }
        public SubgraphKind Kind { get; }

        public LabeledSubgraph(string startNode, IReadOnlyList<Label> sublabels, string endNode, Label label, SubgraphKind kind)
        {
            StartNode = startNode ?? throw new ArgumentNullException(nameof(startNode));
            EndNode = endNode ?? throw new ArgumentNullException(nameof(endNode));
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Sublabels = sublabels ?? throw new ArgumentNullException(nameof(sublabels));
            Kind = kind;
        }
    }
}
