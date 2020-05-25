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

        [Test]
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

        [Test]
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

namespace Monbetsu.Docs
{
    using System.IO;
    using System.Text.RegularExpressions;
    using NUnit.Framework;
    using Monbetsu.Test;

    [Ignore("only for docs")]
    public class Tutorial
    {
        private static readonly Regex PlaceholderRegex = new Regex(
            @"^```csharp\s+(?<file>.+)\s*" +
            @"^namespace\s+(?<namespace>.+)\s*" +
            @"^{\s*" +
            @"^}\s*" +
            @"^```\s*$",
            RegexOptions.Multiline);

        private static readonly Regex FinderRegex = new Regex(
            @"^namespace\s+(?<namespace>.+)\s*" +
            @"^{\s*" +
            @"(.|\n)*?" +
            @"^}\s*$",
            RegexOptions.Multiline);

        [Test]
        public void EmbedToReadMe()
        {
            var templatePath = Path.Combine(PathUtil.SolutionDir, "docs", "README.template.md");
            var outputPath = Path.Combine(PathUtil.SolutionDir, "README.md");

            var text = File.ReadAllText(templatePath);

            var embedded = PlaceholderRegex.Replace(text, match =>
            {
                var from = match.Groups["file"].Value.Trim();
                var ns = match.Groups["namespace"].Value.Trim();
                var path = Path.GetFullPath(from, Path.GetDirectoryName(templatePath)!);

                if (path.StartsWith(PathUtil.SolutionDir))
                {
                    var source = File.ReadAllText(path);

                    foreach(var match2 in FinderRegex.Matches(source).OfType<Match>())
                    {
                        var ns2 = match2.Groups["namespace"].Value.Trim();

                        if (ns == ns2)
                        {
                            var part = Regex.Replace(match2.Value, @"^\s*\[Test\]\s*$", "", RegexOptions.Multiline);

                            return $@"```csharp
{part}
```";
                        }
                    }
                }

                return match.Value;
            });

            embedded = Regex.Replace(embedded, "\r*\n", "\n", RegexOptions.Singleline);

            File.WriteAllText(outputPath, embedded);
        }
    }

    [Ignore("only for docs")]
    public class Demo
    {
        private void PublishDemo(string destinationPath)
        {
            var demoProjDirPath = PathUtil.GetSolutionItemPath("Monbetsu.BlazorDemo");
            var publishDirPath = Path.Combine(demoProjDirPath, "temp-publish");

            Directory.CreateDirectory(publishDirPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = PathUtil.Arguments("publish", "-o", publishDirPath, "-c", "Release"),
                WorkingDirectory = demoProjDirPath,
            };

            Process.Start(startInfo).WaitForExit();

            //startInfo.FileName = "xcopy";
            //startInfo.Arguments = PathUtil.Arguments(Path.Combine(publishDirPath, "wwwroot", "*"), docsPath, "/Y", "/S");
            //Process.Start(startInfo).WaitForExit();

            PathUtil.CopyDicrectory(Path.Combine(publishDirPath, "wwwroot"), destinationPath);

            Directory.Delete(publishDirPath, true);
        }

        [Test]
        public void PublishToDocs()
        {
            var docsPath = PathUtil.GetSolutionItemPath("docs");


            PublishDemo(docsPath);
        }

        [Test]
        public void PublishToPagesTest()
        {
            // docker run --rm --volume="$(PWD):/srv/jekyll" -it -p 4000:4000 jekyll/jekyll jekyll serve -b /Monbetsu

            File.Copy(PathUtil.GetSolutionItemPath("docs", "_config.yml"), Path.Combine(PathUtil.LocalJekyllPath, "_config.yml"), true);

            PublishDemo(PathUtil.LocalJekyllPath);

            File.Copy(PathUtil.GetSolutionItemPath("README.md"), Path.Combine(PathUtil.LocalJekyllPath, "README.md"), true);
            
            Directory.CreateDirectory(Path.Combine(PathUtil.LocalJekyllPath, "docs", "images"));
            PathUtil.CopyDicrectory(PathUtil.GetSolutionItemPath("docs", "images"), Path.Combine(PathUtil.LocalJekyllPath, "docs", "images")); ;
        }
    }
}
