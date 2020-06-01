using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Monbetsu.Test.Utils
{
    public partial class GraphGenerator
    {
        public class Graph
        {
            public IReadOnlyList<Node> Nodes { get; }

            public IReadOnlyList<Node> StartNodes { get; }
            public IReadOnlyList<Node> EndNodes { get; }

            public int TotalEdgeCount { get; }

            

            public Graph(IReadOnlyList<Node> nodes, int? totalEdgeCount = null)
            {
                Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
                
                StartNodes = nodes.Except(nodes.SelectMany(n => n.Nexts)).ToList();
                EndNodes = nodes.Where(n => n.Nexts.Count == 0).ToList();

                TotalEdgeCount = totalEdgeCount ?? nodes.Sum(n => n.Nexts.Count);
            }

            public string GetSummary()
            {
                return @$"Graph Summary    total node#: {Nodes.Count}    total edge#: {TotalEdgeCount}    start node#: {StartNodes.Count}";
            }
        }

        public class Node
        {
            public int Id { get; }
            public IReadOnlyList<Node> Nexts { get; } = new List<Node>();

            public IEnumerable<(Edge edge, Node to)> Outflows => Nexts.Select(n => (new Edge(this, n), n));

            public Node(int id)
            {
                Id = id;
            }

            public override string ToString() => Id.ToString();
        }

        public struct Edge
        {
            public Node From { get; }
            public Node To { get; }

            public Edge(Node from, Node to)
            {
                From = from ?? throw new ArgumentNullException(nameof(from));
                To = to ?? throw new ArgumentNullException(nameof(to));
            }

            public override string ToString() => $"{From}->{To}";
        }

        public int Trial { get; set; } = 10;

        public Range NumberOfLayers { get; set; } = 2..20;

        public Range NumberOfEdges { get; set; } = 1..2000;
        public Range NumberOfNodes { get; set; } = 2..1000;

        public Range NumberOfEdgesPerNode { get; set; } = 1..10;

        private int Pick(Random random, Range range) => random.Next(range.Start.Value, range.End.Value);
        private bool Contains(Range range, int value) => range.Start.Value <= value && value < range.End.Value;

        private T Pick<T>(IReadOnlyList<T> list, Random random) => list[random.Next(list.Count)];
        
        public Graph Generate(Random random, int nodeIdSeq = 0)
        {
            var layerCount = Pick(random, NumberOfLayers);

            var nodeCount = Pick(random, NumberOfNodes);

            var ls = Enumerable.Range(0, layerCount).Concat(Enumerable.Range(0, Math.Max(0, nodeCount - layerCount)).Select(_ => random.Next(layerCount))).OrderBy(_ => _).ToList();
            var nodeCountsPerLayer = ls.ToLookup(l => l).ToDictionary(g => g.Key, g => g.Count());

            //var trial = 0;
            //while (!Contains(NumberOfLayers, nodeCountsPerLayer.Count) || nodeCountsPerLayer[0] == 0 || nodeCountsPerLayer.ContainsKey(layerCount - 1))
            //{
            //    ls = Enumerable.Range(0, nodeCount).Select(_ => random.Next(layerCount)).OrderBy(_ => _).ToList();
            //    nodeCountsPerLayer = ls.ToLookup(l => l).ToDictionary(g => g.Key, g => g.Count());

            //    if (trial++ >= Trial)
            //    {
            //        throw new Exception("some of constraints can not be satisfied.");
            //    }
            //}


            var ordered = nodeCountsPerLayer.ToDictionary(kv => kv.Key, kv => nodeCountsPerLayer.Where(kv2 => kv2.Key <= kv.Key).Sum(_ => _.Value));
            
            var nodeEdges = Enumerable.Range(0, nodeCount - nodeCountsPerLayer[layerCount - 1]).Select(_ => Pick(random, NumberOfEdgesPerNode)).ToList();
            var edgeCount = nodeEdges.Sum();

            if (edgeCount >= NumberOfEdges.End.Value)
            {
                var candidates = nodeEdges.Select((e, i) => (e, i)).Where(t => t.e > NumberOfEdgesPerNode.Start.Value).ToList();

                while (candidates.Count > 0 && edgeCount >= NumberOfEdges.End.Value)
                {
                    var target = random.Next(candidates.Count);
                    nodeEdges[candidates[target].i]--;
                    candidates[target] = (candidates[target].e - 1, candidates[target].i);
                    edgeCount--;
                    if (candidates[target].e == NumberOfEdgesPerNode.Start.Value)
                    {
                        candidates.RemoveAt(target);
                    }
                }

                if (edgeCount >= NumberOfEdges.End.Value)
                {
                    throw new Exception("some of constraints can not be satisfied.");
                }
            }

            if (edgeCount < NumberOfEdges.Start.Value)
            {
                var candidates = nodeEdges.Select((e, i) => (e, i)).Where(t => t.e < NumberOfEdgesPerNode.End.Value - 1).ToList();

                while (candidates.Count > 0 && edgeCount < NumberOfEdges.Start.Value)
                {
                    var target = random.Next(candidates.Count);
                    nodeEdges[candidates[target].i]++;
                    candidates[target] = (candidates[target].e + 1, candidates[target].i);
                    edgeCount++;
                    if (candidates[target].e == NumberOfEdgesPerNode.End.Value - 1)
                    {
                        candidates.RemoveAt(target);
                    }
                }

                if (edgeCount < NumberOfEdges.Start.Value)
                {
                    throw new Exception("some of constraints can not be satisfied.");
                }
            }

            var nodeWithIndexes = ls.Select((l, i) => (node: new Node(nodeIdSeq++), index: i)).ToList();

            foreach(var (node, index, ne) in nodeWithIndexes.Zip(nodeEdges, (n, ne) => (n.node, n.index, ne)))
            {
                (node.Nexts as List<Node>)!.AddRange(Enumerable.Range(0, ne).Select(n => nodeWithIndexes[random.Next(ordered[ls[index]], nodeWithIndexes.Count)].node));
            }

            return new Graph(nodeWithIndexes.Select(_ => _.node).ToList(), edgeCount);
        }

        public Graph GenerateNestedly(Random random, int maxStartNodes = 10, int maxEndNodes = 10, int initialGrows = 0, int grows = 0, int maxSplit = 3, int nodeIdSeq = 0)
        {
            var startNodes = Enumerable.Range(0, random.Next(maxStartNodes) + 1).Select(_ => new Node(nodeIdSeq++)).ToList();
            var endNodes = Enumerable.Range(0, random.Next(maxEndNodes) + 1).Select(_ => new Node(nodeIdSeq++)).ToList();

            void MakeSub(Node startNode, Node endNode)
            {
                var childGenerator = new GraphGenerator()
                {
                    NumberOfEdges = this.NumberOfEdges,
                    NumberOfEdgesPerNode = this.NumberOfEdgesPerNode,
                    NumberOfLayers = this.NumberOfLayers,
                    NumberOfNodes = this.NumberOfNodes

                };
                var childGraph = childGenerator.Generate(random, nodeIdSeq);

                (startNode.Nexts as List<Node>)!.AddRange(childGraph.StartNodes);
                foreach(var prev in childGraph.EndNodes)
                {
                    (prev.Nexts as List<Node>)!.Add(endNode);
                }

                nodeIdSeq = childGraph.Nodes.Max(n => n.Id) + 1;
            }

            void Split(Node node)
            {
                if (node.Nexts.Count > 0)
                {
                    var num = random.Next(maxSplit) + 1;
                    var nextAt = random.Next(node.Nexts.Count);
                    var next = node.Nexts[nextAt];

                    var newNexts = Enumerable.Range(0, num).Select(n => new Node(nodeIdSeq++)).ToList();

                    (node.Nexts as List<Node>)![nextAt] = newNexts[0];

                    for(var i = 0; i < newNexts.Count - 1; i++)
                    {
                        (newNexts[i].Nexts as List<Node>)!.Add(newNexts[i + 1]);
                    }

                    (newNexts.Last().Nexts as List<Node>)!.Add(next);
                }
            }

            void Cut(Node node)
            {
                if (node.Nexts.Count > 0)
                {
                    var num = random.Next(maxSplit) + 1;
                    var nextAt = random.Next(node.Nexts.Count);
                    
                    (node.Nexts as List<Node>)!.RemoveAt(nextAt);
                }
            }

            void Bypass(Node startNode, Node endNode)
            {
                (startNode.Nexts as List<Node>)!.Add(endNode);
            }

            if (initialGrows == 0)
            {
                initialGrows = random.Next(Math.Max(startNodes.Count, endNodes.Count) * 3);
            }
            foreach(var _ in Enumerable.Range(0, initialGrows))
            {
                MakeSub(Pick(startNodes, random), Pick(endNodes, random));
            }

            List<Node> Refresh(IEnumerable<Node> startNodes)
            {
                var list = new List<Node>();
                var stack = new Stack<(Node node, bool flag)>();
                var seens = new Dictionary<Node, bool>();
            
                foreach (var n in startNodes)
                {
                    stack.Push((n, false));
                }

                while (stack.Count > 0)
                {
                    var (node, flag) = stack.Pop();

                    if (flag)
                    {
                        list.Add(node);
                        seens[node] = true;
                    }
                    else 
                    {
                        stack.Push((node, true));
                        seens[node] = false;
                        for (var i =0; i < node.Nexts.Count; i++)
                        {
                            var next = node.Nexts[i];
                            if (seens.TryGetValue(next, out var visited))
                            {
                                if (!visited)
                                {
                                    (node.Nexts as List<Node>)!.RemoveAt(i--);
                                }
                            }
                            else
                            {
                                stack.Push((next, false));
                            }
                        }
                    }
                }

                list.Reverse();
                return list;
            }

            var nodes = Refresh(startNodes);
            if (grows == 0)
            {
                grows = random.Next(Math.Max(startNodes.Count, endNodes.Count) * 5);
            }
            foreach (var _ in Enumerable.Range(0, grows))
            {
                switch(random.Next(3))
                {
                    case 0:
                        Split(Pick(nodes, random));
                        break;
                    case 1:
                        if (nodes.Count >= 2)
                        {
                            var s = random.Next(nodes.Count - 1);
                            var e = random.Next(nodes.Count - s - 1) + s + 1;

                            Bypass(nodes[s], nodes[e]);
                        }
                        break;
                    case 2:
                        Cut(Pick(nodes, random));
                        break;
                    default:
                        if (nodes.Count >= 2)
                        {
                            var s = random.Next(nodes.Count - 1);
                            var e = random.Next(nodes.Count - s - 1) + s + 1;

                            MakeSub(nodes[s], nodes[e]);
                        }
                        break;
                }

                nodes = Refresh(nodes.Except(nodes.SelectMany(n => n.Nexts)).ToList());
            }


            return new Graph(nodes);
        }
    }
}
