using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Monbetsu.Test
{

    public partial class Tests
    {

        static partial void AttachVisual(
            IReadOnlyDictionary<Edge, LabelGroup> expectedEdgeGroupDic,
            IReadOnlyDictionary<Edge, LabelGroup> resultEdgeGroupDic,
            IReadOnlyDictionary<(Node startNode, int subgroup, Node endNode), LabelGroup> expectedSerGroupDic,
            IReadOnlyDictionary<(Node startNode, int subgroup, Node endNode), LabelGroup> resultSerGroupDic,
            IReadOnlyDictionary<(Node startNode, Node endNode), LabelGroup> expectedParGroupDic,
            IReadOnlyDictionary<(Node startNode, Node endNode), LabelGroup> resultParGroupDic,
            IReadOnlyDictionary<(Node startNode, UnorderedNTuple<int> subgroups, Node endNode), LabelGroup> expectedKnotGroupDic,
            IReadOnlyDictionary<(Node startNode, UnorderedNTuple<int> subgroups, Node endNode), LabelGroup> resultKnotGroupDic,
            string id, List<string> assertionMessages)
        {
            string MakeDot()
            {
                var rootGraph = new Dot.Digraph { Id = TestContext.CurrentContext.Test.MethodName };

                foreach (var kv in resultEdgeGroupDic)
                {
                    var edge = kv.Key;
                    var actualGroup = kv.Value;

                    Dot.IColor? color = null; 
                    if (!expectedEdgeGroupDic.TryGetValue(edge, out var expectedGroup))
                    {
                        assertionMessages.Add($"unexpected label for {edge} was found.");
                        color = Dot.NamedColor.Red;
                    }
                    else if (expectedGroup.Group != actualGroup.Group)
                    {
                        assertionMessages.Add($"expected label for {edge} is {expectedGroup}, but be {actualGroup}.");
                        color = Dot.NamedColor.Red;
                    }

                    rootGraph.Edges.Add(new Dot.Edge
                    {
                        From = edge.FromNode.Id, To = edge.ToNode.Id,
                        Label = $"E:{expectedGroup}/A:{actualGroup}",
                        Color = color
                    });
                }
                foreach(var kv in expectedEdgeGroupDic)
                {
                    var edge = kv.Key;
                    if (!expectedEdgeGroupDic.TryGetValue(edge, out var expectedGroup))
                    {
                        assertionMessages.Add($"expected label for {edge} was not found.");
                        rootGraph.Edges.Add(new Dot.Edge
                        {
                            From = edge.FromNode.Id,
                            To = edge.ToNode.Id,
                            Label = $"E:{expectedGroup}/A:X",
                            Color = Dot.NamedColor.Red
                        });
                    }
                }


                foreach (var kv in expectedKnotGroupDic)
                {
                    var (fromNode, subgroups, toNode) = kv.Key;
                    var expectedGroup = kv.Value;

                    Dot.IColor? color = Dot.NamedColor.Blue;
                    if (!resultKnotGroupDic.TryGetValue(kv.Key, out var actualGroup))
                    {
                        assertionMessages.Add($"expected label for ({fromNode.Id} -> {toNode.Id}) was not found.");
                        color = Dot.NamedColor.Red;
                    }
                    else if (expectedGroup.Group != actualGroup.Group)
                    {
                        assertionMessages.Add($"expected label for ({fromNode.Id} -> {toNode.Id}) is {expectedGroup}, but be {actualGroup}.");
                        color = Dot.NamedColor.Red;
                    }

                    rootGraph.Edges.Add(new Dot.Edge
                    {
                        From = fromNode.Id,
                        To = toNode.Id,
                        Label = $"E:{expectedGroup}/A:{actualGroup}",
                        Color = color,
                        Style = Dot.EdgeStyle.dashed
                    });
                }

                foreach (var kv in resultKnotGroupDic)
                {
                    var (fromNode, subgroups, toNode) = kv.Key;
                    
                    if (!expectedKnotGroupDic.TryGetValue(kv.Key, out var actualGroup))
                    {
                        assertionMessages.Add($"unexpected label for ({fromNode.Id} -> {toNode.Id}) was found.");

                        rootGraph.Edges.Add(new Dot.Edge
                        {
                            From = fromNode.Id,
                            To = toNode.Id,
                            Label = $"E:X/A:Kx",
                            Color = Dot.NamedColor.Red,
                            Style = Dot.EdgeStyle.dashed
                        });
                    }
                }

                foreach (var kv in expectedSerGroupDic)
                {
                    var (fromNode, sub, toNode) = kv.Key;
                    var expectedGroup = kv.Value;

                    var color = Dot.NamedColor.Green;
                    if (!resultSerGroupDic.TryGetValue(kv.Key, out var actualGroup))
                    {
                        assertionMessages.Add($"expected label for ({fromNode.Id} -> {toNode.Id}) was not found.");
                        color = Dot.NamedColor.Red;
                    }
                    else if (expectedGroup.Group != actualGroup.Group)
                    {
                        assertionMessages.Add($"expected label for ({fromNode.Id} -> {toNode.Id}) is {expectedGroup}, but be {actualGroup}.");
                        color = Dot.NamedColor.Red;
                    }

                    rootGraph.Edges.Add(new Dot.Edge
                    {
                        From = fromNode.Id,
                        To = toNode.Id,
                        Label = $"E:{expectedGroup}/A:{actualGroup}",
                        Color = color,
                        Style = Dot.EdgeStyle.dashed
                    });
                }

                foreach (var kv in resultSerGroupDic)
                {
                    var (fromNode, sub, toNode) = kv.Key;

                    if (!expectedSerGroupDic.TryGetValue(kv.Key, out var actualGroup))
                    {
                        assertionMessages.Add($"unexpected label for ({fromNode.Id} -> {toNode.Id}) was found.");

                        rootGraph.Edges.Add(new Dot.Edge
                        {
                            From = fromNode.Id,
                            To = toNode.Id,
                            Label = $"E:X/A:Sx",
                            Color = Dot.NamedColor.Red,
                            Style = Dot.EdgeStyle.dashed
                        });
                    }
                }

                foreach (var kv in expectedParGroupDic)
                {
                    var (fromNode, toNode) = kv.Key;
                    var expectedGroup = kv.Value;

                    var color = Dot.NamedColor.Gray;
                    if (!resultParGroupDic.TryGetValue(kv.Key, out var actualGroup))
                    {
                        assertionMessages.Add($"expected label for ({fromNode.Id} -> {toNode.Id}) was not found.");
                        color = Dot.NamedColor.Red;
                    }
                    else if (expectedGroup.Group != actualGroup.Group)
                    {
                        assertionMessages.Add($"expected label for ({fromNode.Id} -> {toNode.Id}) is {expectedGroup}, but be {actualGroup}.");
                        color = Dot.NamedColor.Red;
                    }

                    rootGraph.Edges.Add(new Dot.Edge
                    {
                        From = fromNode.Id,
                        To = toNode.Id,
                        Label = $"E:{expectedGroup}/A:{actualGroup}",
                        Color = color,
                        Style = Dot.EdgeStyle.dashed,
                        ArrowHead = Dot.ArrowType.empty
                    });
                }

                foreach (var kv in resultParGroupDic)
                {
                    var (fromNode, toNode) = kv.Key;

                    if (!expectedParGroupDic.TryGetValue(kv.Key, out var actualGroup))
                    {
                        assertionMessages.Add($"unexpected label for ({fromNode.Id} -> {toNode.Id}) was found.");
                        
                        rootGraph.Edges.Add(new Dot.Edge
                        {
                            From = fromNode.Id,
                            To = toNode.Id,
                            Label = $"E:X/A:Px",
                            Color = Dot.NamedColor.Red,
                            Style = Dot.EdgeStyle.dashed,
                            ArrowHead = Dot.ArrowType.empty
                        });
                    }
                }

                if (assertionMessages.Count > 0)
                {
                    var asserted = string.Join("\n", assertionMessages);

                    rootGraph.Nodes.Add(new Dot.Node
                    {
                        Id = "RESULT",
                        Color = Dot.NamedColor.Red,
                        Label = asserted,
                        Shape = Dot.PolygonBasedShape.Box
                    });

                    var tailNodes = expectedEdgeGroupDic.Keys.Select(k => k.ToNode).Except(expectedEdgeGroupDic.Keys.Select(k => k.FromNode));
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

                return rootGraph.Build();
            }

            var dotCode = MakeDot();

            var outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "images");

            Directory.CreateDirectory(outputDir);

            var outputPath = Path.Combine(outputDir, $"{TestContext.CurrentContext.Test.Name}-{id}.png");

            Visualizer.GenerateImageFromDot(dotCode, outputPath);
        }


    }
}
