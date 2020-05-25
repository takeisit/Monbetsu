using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monbetsu.Test
{
    public class TopologicalSortTest
    {
        class Result
        {
            public virtual int NumberOfResult => 1;

            public virtual IEnumerable<Result> ActualDescendants
            {
                get
                {
                    yield return this;
                }
            }

            public virtual (IEnumerable<Result> next, Result[] expected) Match(IEnumerable<Result> actual)
            {
                var first = actual.First();
                if (first.Equals(this))
                {
                    return (actual.Skip(1), Array.Empty<Result>());
                }
                else
                {
                    return (actual, new[] { this });
                }
            }

            
        }

        class NodeVisitor : Result, IEquatable<NodeVisitor?>
        {
            public int Node { get; }
            public int Depth { get; }
            public int Order { get; }
            public bool IsLast { get; }

            public NodeVisitor(int node, int depth, int order, bool isLast)
            {
                Node = node;
                Depth = depth;
                Order = order;
                IsLast = isLast;
            }

            public override string ToString() => $"\nNode {Node}, depth:{Depth}, order:{Order}, isLast:{IsLast}";

            public override bool Equals(object? obj)
            {
                return Equals(obj as NodeVisitor);
            }

            public bool Equals(NodeVisitor? other)
            {
                return other != null &&
                       Node == other.Node &&
                       Depth == other.Depth &&
                       //Order == other.Order &&
                       IsLast == other.IsLast;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Node, Depth, /*Order,*/ IsLast);
            }

        }

        class EdgeVisitor : Result, IEquatable<EdgeVisitor?>
        {
            public int Parent { get; }
            public (int from, int to) Edge { get; }
            public int Child { get; }
            public bool IsSingle { get; }

            public EdgeVisitor(int parent, (int from, int to) edge, int child, bool isSingle)
            {
                Parent = parent;
                Edge = edge;
                Child = child;
                IsSingle = isSingle;
            }

            public override string ToString() => $"\nEdge {Parent} -> {Child} {Edge}, isSingle:{IsSingle}";

            public override bool Equals(object? obj)
            {
                return Equals(obj as EdgeVisitor);
            }

            public bool Equals(EdgeVisitor? other)
            {
                return other != null &&
                       Parent == other.Parent &&
                       Edge.Equals(other.Edge) &&
                       Child == other.Child &&
                       IsSingle == other.IsSingle;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Parent, Edge, Child, IsSingle);
            }
        }
        
        class PostNodeVisitor : Result, IEquatable<PostNodeVisitor?>
        {
            public int Node { get; }
            public int Depth { get; }
            public int Order { get; }
            public int ChildCount { get; }

            public PostNodeVisitor(int node, int depth, int order, int childCount)
            {
                Node = node;
                Depth = depth;
                Order = order;
                ChildCount = childCount;
            }

            public override string ToString() => $"\nPostNode {Node}, depth:{Depth}, order:{Order}, childCount:{ChildCount}";

            public override bool Equals(object? obj)
            {
                return Equals(obj as PostNodeVisitor);
            }

            public bool Equals(PostNodeVisitor? other)
            {
                return other != null &&
                       Node == other.Node &&
                       Depth == other.Depth &&
                       //Order == other.Order &&
                       ChildCount == other.ChildCount;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Node, Depth, /*Order,*/ ChildCount);
            }

        }
        
        class SweepedParentNodeVisitor : Result, IEquatable<SweepedParentNodeVisitor?>
        {
            public int Node { get; }
            public int Depth { get; }

            public SweepedParentNodeVisitor(int node, int depth)
            {
                Node = node;
                Depth = depth;
            }

            public override string ToString() => $"\nSweepedParent {Node}, depth:{Depth}";

            public override bool Equals(object? obj)
            {
                return Equals(obj as SweepedParentNodeVisitor);
            }

            public bool Equals(SweepedParentNodeVisitor? other)
            {
                return other != null &&
                       Node == other.Node &&
                       Depth == other.Depth;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Node, Depth);
            }
        }
            

        class SweepedGrandparentNodeVisitor : Result, IEquatable<SweepedGrandparentNodeVisitor?>
        {
            public int Node { get; }
            public int Depth { get; }

            public SweepedGrandparentNodeVisitor(int node, int depth)
            {
                Node = node;
                Depth = depth;
            }

            public override string ToString() => $"\nSweepedGrandparent {Node}, depth:{Depth}";
            public override bool Equals(object? obj)
            {
                return Equals(obj as SweepedGrandparentNodeVisitor);
            }

            public bool Equals(SweepedGrandparentNodeVisitor? other)
            {
                return other != null &&
                       Node == other.Node &&
                       Depth == other.Depth;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Node, Depth);
            }
        }

        class OrderedResult : Result
        {
            public override int NumberOfResult => Children.Count;
            public IReadOnlyList<Result> Children { get; }

            public override IEnumerable<Result> ActualDescendants => Children.SelectMany(c => c.ActualDescendants);

            public OrderedResult(params Result[] children)
            {
                Children = children;
            }

            public override int GetHashCode() => Children.Count;

            public override string ToString()
            {
                return $@"
O({string.Join("", Children)}
)";
            }


            public override (IEnumerable<Result> next, Result[] expected) Match(IEnumerable<Result> actual)
            {
                foreach(var child in Children)
                {
                    var (next, inners) = child.Match(actual);
                    if (inners.Length > 0)
                    {
                        return (actual, inners.Append(child).ToArray());
                    }
                    actual = next;
                }

                return (actual, Array.Empty<Result>());
            }
        }

        class UnorderedResult : Result
        {
            public override int NumberOfResult => Children.Count;
            public override IEnumerable<Result> ActualDescendants => Children.SelectMany(c => c.ActualDescendants);

            public IReadOnlyList<Result> Children { get; }

            public UnorderedResult(params Result[] children)
            {
                Children = children;
            }

            public override string ToString()
            {
                return $@"
U({string.Join("", Children)}
)";
            }

            public override (IEnumerable<Result> next, Result[] expected) Match(IEnumerable<Result> actual)
            {
                var matched = new HashSet<int>();
                var anchor = actual;
                var localMatched = true;

                while (matched.Count < Children.Count && localMatched)
                {
                    localMatched = false;
                    for (var i = 0; i < Children.Count; i++)
                    {
                        if (matched.Contains(i))
                        {
                            continue;
                        }

                        var child = Children[i];

                        var head = child;
                        var (next, inners) = child.Match(actual);

                        if (next != actual && inners.Length == 0)
                        {
                            actual = next;
                            matched.Add(i);
                            localMatched = true;
                        }
                    }
                }

                if (matched.Count == Children.Count)
                {
                    return (actual, Array.Empty<Result>());
                }

                return (anchor, new[] { this });
            }
        }

        class EndResult : Result
        {
            public override string ToString() => "\n";
        }

        private static readonly EndResult End = new EndResult();

        class Case
        {
            public  IEnumerable<int> StartNodes { get; set; } = Enumerable.Empty<int>();
            
            public IEnumerable<(int from, int to)> Edges { get; set; } = Enumerable.Empty<(int from, int to)>();


            public Case WithStartNodes(params int[] startNodes)
            {
                StartNodes = startNodes;
                return this;
            }


            public Case WithEdges(params (int from, int to)[] edges)
            {
                Edges = edges;
                return this;
            }


            public IEnumerable<Result> Act()
            {
                var graph = default(x02.Unit);
                var results = new List<Result>();

                x02.GraphEnvironment<int, (int from, int to), x02.Unit>.PipeTraverseTopologically(StartNodes, 
                    (node, depth, order, isLast) => results.Add(new NodeVisitor(node, depth, order, isLast)),
                    (parent, edge, child, isSingle) => results.Add(new EdgeVisitor(parent, edge, child, isSingle)),
                    (node, depth, order, childCount) => results.Add(new PostNodeVisitor(node, depth, order, childCount)),
                    (node, depth) => results.Add(new SweepedParentNodeVisitor(node, depth)),
                    (node, depth) => results.Add(new SweepedGrandparentNodeVisitor(node, depth)),
                    (_, node) => Edges.Where(e => e.to == node).Select(e => (e, e.from)),
                    (_, node) => Edges.Where(e => e.from == node).Select(e => (e, e.to)),
                    in graph,
                    true
                    );

                return results;
            }


            public void ActAssert(params Result[] expected)
            {
                var graph = default(x02.Unit);
                var results = new List<Result>();

                x02.GraphEnvironment<int, (int from, int to), x02.Unit>.PipeTraverseTopologically(StartNodes,
                    (node, depth, order, isLast) => results.Add(new NodeVisitor(node, depth, order, isLast)),
                    (parent, edge, child, isSingle) => results.Add(new EdgeVisitor(parent, edge, child, isSingle)),
                    (node, depth, order, childCount) => results.Add(new PostNodeVisitor(node, depth, order, childCount)),
                    (node, depth) => results.Add(new SweepedParentNodeVisitor(node, depth)),
                    (node, depth) => results.Add(new SweepedGrandparentNodeVisitor(node, depth)),
                    (_, node) => Edges.Where(e => e.to == node).Select(e => (e, e.from)),
                    (_, node) => Edges.Where(e => e.from == node).Select(e => (e, e.to)),
                    in graph,
                    true
                    );

                var ordered = O(expected);
                var (consumed, inners) = ordered.Match(results);
                if (consumed.Any())
                {
                    var expected_ = inners.FirstOrDefault();
                    var actual_ = consumed.Take(expected_?.ActualDescendants.Count() ?? 0);
                    actual_.Should().Equal(new[] { expected_ }, $"{string.Join("", results)}");
                }
            }
        }
        private static Result O(params Result[] results) => new OrderedResult(results);
        private static Result U(params Result[] results) => new UnorderedResult(results);

        [Test]
        public void Test0()
        {
            new Case()
            .Act().Should().BeEmpty();
        }

        [Test]
        public void Test1()
        {
            new Case()
            .WithStartNodes(0)
            .Act().Should().Equal(new Result[]{
                new NodeVisitor(0, 0, 0, true),
                new PostNodeVisitor(0, 0, 0, 0),

                new SweepedParentNodeVisitor(0, 0),
                new SweepedGrandparentNodeVisitor(0, 0)
                });
        }

        [Test]
        public void Test2()
        {
            new Case()
            .WithStartNodes(0)
            .WithEdges(
                (from: 0, to: 1)   
            )
            .Act().Should().Equal(new Result[]{
                new NodeVisitor(0, 0, 0, false),
                new EdgeVisitor(0, (0, 1), 1, true),
                new PostNodeVisitor(0, 0, 0, 1),

                new NodeVisitor(1, 1, 1, true),
                new PostNodeVisitor(1, 1, 1, 0),

                new SweepedParentNodeVisitor(0, 0),
                new SweepedParentNodeVisitor(1, 1),

                new SweepedGrandparentNodeVisitor(0, 0),
                new SweepedGrandparentNodeVisitor(1, 1)
            });
        }

        [Test]
        public void Test3()
        {
            new Case()
            .WithStartNodes(0, 1)
            .Act().Should().Equal(new Result[]{
                new NodeVisitor(0, 0, 0, true),
                new PostNodeVisitor(0, 0, 0, 0),

                new NodeVisitor(1, 0, 1, true),
                new PostNodeVisitor(1, 0, 1, 0),

                new SweepedParentNodeVisitor(0, 0),
                new SweepedParentNodeVisitor(1, 0),

                new SweepedGrandparentNodeVisitor(0, 0),
                new SweepedGrandparentNodeVisitor(1, 0)
            });
        }

        [Test]
        public void Test4()
        {
            new Case()
            .WithStartNodes(0, 2)
            .WithEdges(
                (from: 0, to: 1),
                (from: 2, to: 3)
            )
            .ActAssert(
                U(
                    O(
                        new NodeVisitor(0, 0, 0, false),
                        new EdgeVisitor(0, (0, 1), 1, true),
                        new PostNodeVisitor(0, 0, 0, 1)
                    ),
                    O(
                        new NodeVisitor(2, 0, 1, false),
                        new EdgeVisitor(2, (2, 3), 3, true),
                        new PostNodeVisitor(2, 0, 1, 1)
                    )
                ),
                U(
                   O(
                       new NodeVisitor(1, 1, 2, true),
                       new PostNodeVisitor(1, 1, 2, 0),

                       new SweepedParentNodeVisitor(0, 0)
                   ),
                   O(
                       new NodeVisitor(3, 1, 3, true),
                       new PostNodeVisitor(3, 1, 3, 0),

                       new SweepedParentNodeVisitor(2, 0)
                   )
                ),

                U(
                    O(
                        new SweepedParentNodeVisitor(1, 1),
                        new SweepedGrandparentNodeVisitor(0, 0)
                    ),
                    O(
                        new SweepedParentNodeVisitor(3, 1),
                        new SweepedGrandparentNodeVisitor(2, 0)
                    )
                ),
                
                U(
                    new SweepedGrandparentNodeVisitor(1, 1),

                    new SweepedGrandparentNodeVisitor(3, 1)
                )
            );
        }

        [Test]
        public void Test5()
        {
            new Case()
            .WithStartNodes(0, 1)
            .WithEdges(
                (from: 0, to: 2),
                (from: 1, to: 2),
                (from: 2, to: 3),
                (from: 2, to: 4)
            )
            .ActAssert(
                U(
                    O(
                        new NodeVisitor(0, 0, 0, false),
                        new EdgeVisitor(0, (0, 2), 2, true),
                        new PostNodeVisitor(0, 0, 0, 1)
                    ),
                    O(
                        new NodeVisitor(1, 0, 1, false),
                        new EdgeVisitor(1, (1, 2), 2, true),
                        new PostNodeVisitor(1, 0, 1, 1)
                    )
                ),
                new NodeVisitor(2, 1, 2, false),
                new EdgeVisitor(2, (2, 3), 3, false),
                new EdgeVisitor(2, (2, 4), 4, false),
                new PostNodeVisitor(2, 1, 2, 2),

                U(
                    new SweepedParentNodeVisitor(0, 0),
                    new SweepedParentNodeVisitor(1, 0)
                ),
                U(
                   O(
                       new NodeVisitor(3, 2, 3, true),
                       new PostNodeVisitor(3, 2, 3, 0)
                   ),
                   O(
                       new NodeVisitor(4, 2, 4, true),
                       new PostNodeVisitor(4, 2, 4, 0)
                   )
                ),
                new SweepedParentNodeVisitor(2, 1),
                U(
                    new SweepedGrandparentNodeVisitor(0, 0),
                    new SweepedGrandparentNodeVisitor(1, 0)
                ),
                U(
                    new SweepedParentNodeVisitor(3, 2),
                    new SweepedParentNodeVisitor(4, 2)
                ),
                new SweepedGrandparentNodeVisitor(2, 1),
                U(
                    new SweepedGrandparentNodeVisitor(3, 2),
                    new SweepedGrandparentNodeVisitor(4, 2)
                )
            );
        }

        [Test]
        public void Test5a()
        {
            new Case()
            .WithStartNodes(0, 1, 2, 3, 4)
            .WithEdges(
                (from: 0, to: 2),
                (from: 1, to: 2),
                (from: 2, to: 3),
                (from: 2, to: 4)
            )
            .ActAssert(
                U(
                    O(
                        new NodeVisitor(0, 0, 0, false),
                        new EdgeVisitor(0, (0, 2), 2, true),
                        new PostNodeVisitor(0, 0, 0, 1)
                    ),
                    O(
                        new NodeVisitor(1, 0, 1, false),
                        new EdgeVisitor(1, (1, 2), 2, true),
                        new PostNodeVisitor(1, 0, 1, 1)
                    )
                ),
                new NodeVisitor(2, 1, 2, false),
                new EdgeVisitor(2, (2, 3), 3, false),
                new EdgeVisitor(2, (2, 4), 4, false),
                new PostNodeVisitor(2, 1, 2, 2),

                U(
                    new SweepedParentNodeVisitor(0, 0),
                    new SweepedParentNodeVisitor(1, 0)
                ),
                U(
                   O(
                       new NodeVisitor(3, 2, 3, true),
                       new PostNodeVisitor(3, 2, 3, 0)
                   ),
                   O(
                       new NodeVisitor(4, 2, 4, true),
                       new PostNodeVisitor(4, 2, 4, 0)
                   )
                ),
                new SweepedParentNodeVisitor(2, 1),
                U(
                    new SweepedGrandparentNodeVisitor(0, 0),
                    new SweepedGrandparentNodeVisitor(1, 0)
                ),
                U(
                    new SweepedParentNodeVisitor(3, 2),
                    new SweepedParentNodeVisitor(4, 2)
                ),
                new SweepedGrandparentNodeVisitor(2, 1),
                U(
                    new SweepedGrandparentNodeVisitor(3, 2),
                    new SweepedGrandparentNodeVisitor(4, 2)
                )
            );
        }

        

        [Test]
        public void Test8()
        {
            new Case()
            .WithStartNodes(0)
            .WithEdges(
                (from: 0, to: 1),
                (from: 0, to: 2),
                (from: 2, to: 3),
                (from: 2, to: 4),
                (from: 4, to: 5),
                (from: 4, to: 6),
                (from: 6, to: 7),
                (from: 6, to: 8)
            )
            .ActAssert(
                new NodeVisitor(0, 0, 0, false),
                new EdgeVisitor(0, (0, 1), 1, false),
                new EdgeVisitor(0, (0, 2), 2, false),
                new PostNodeVisitor(0, 0, 0, 2),

                new NodeVisitor(2, 1, 0, false),
                new EdgeVisitor(2, (2, 3), 3, false),
                new EdgeVisitor(2, (2, 4), 4, false),
                new PostNodeVisitor(2, 1, 0, 2),

                new NodeVisitor(4, 2, 0, false),
                new EdgeVisitor(4, (4, 5), 5, false),
                new EdgeVisitor(4, (4, 6), 6, false),
                new PostNodeVisitor(4, 2, 0, 2),

                new NodeVisitor(6, 3, 0, false),
                new EdgeVisitor(6, (6, 7), 7, false),
                new EdgeVisitor(6, (6, 8), 8, false),
                new PostNodeVisitor(6, 3, 0, 2),

                U(
                    O(
                        new NodeVisitor(1, 4, 0, true),
                        new PostNodeVisitor(1, 4, 0, 0),
                        new SweepedParentNodeVisitor(0, 0)
                    ),

                    O(
                        new NodeVisitor(3, 4, 0, true),
                        new PostNodeVisitor(3, 4, 0, 0),
                        new SweepedParentNodeVisitor(2, 1)//,
                    ),

                    O(
                        new NodeVisitor(5, 4, 0, true),
                        new PostNodeVisitor(5, 4, 0, 0),
                        new SweepedParentNodeVisitor(4, 2)//,
                    ),

                    O(
                        U(
                            O(
                                new NodeVisitor(7, 4, 0, true),
                                new PostNodeVisitor(7, 4, 0, 0)
                            ),

                            O(
                                new NodeVisitor(8, 4, 0, true),
                                new PostNodeVisitor(8, 4, 0, 0)
                            )
                        ),
                        new SweepedParentNodeVisitor(6, 3),
                        
                        U(
                            O(
                                new SweepedParentNodeVisitor(1, 4),
                                new SweepedGrandparentNodeVisitor(0, 0)
                            ),
                            O(
                                new SweepedParentNodeVisitor(3, 4),
                                new SweepedGrandparentNodeVisitor(2, 1)
                            ),
                            O(
                                new SweepedParentNodeVisitor(5, 4),
                                new SweepedGrandparentNodeVisitor(4, 2)
                            ),
                            new SweepedParentNodeVisitor(7, 4),
                            new SweepedParentNodeVisitor(8, 4)
                        ),

                        new SweepedGrandparentNodeVisitor(6, 3),

                        U(
                            new SweepedGrandparentNodeVisitor(1, 4),
                            new SweepedGrandparentNodeVisitor(3, 4),
                            new SweepedGrandparentNodeVisitor(5, 4),
                            new SweepedGrandparentNodeVisitor(7, 4),
                            new SweepedGrandparentNodeVisitor(8, 4)
                        )
                    )
                )
            );
        }


        [Test]
        public void TestCyclic1()
        {
            Assert.Catch<x02.GraphEnvironment<int, (int from, int to), x02.Unit>.CyclicException>(() =>
                new Case()
                .WithStartNodes(0)
                .WithEdges(
                    (from: 0, to: 0)
                )
                .Act()
            ).UnresolvableNodes.Should().BeEquivalentTo(0);

            Assert.Catch<x02.GraphEnvironment<int, (int from, int to), x02.Unit>.CyclicException>(() =>
                new Case()
                .WithStartNodes(0)
                .WithEdges(
                    (from: 0, to: 1),
                    (from: 1, to: 0)
                )
                .Act()
            ).UnresolvableNodes.Should().BeEquivalentTo(0);

            Assert.Catch<x02.GraphEnvironment<int, (int from, int to), x02.Unit>.CyclicException>(() =>
                new Case()
                .WithStartNodes(0)
                .WithEdges(
                    (from: 0, to: 1),
                    (from: 1, to: 2),
                    (from: 2, to: 0)
                )
                .Act()
            ).UnresolvableNodes.Should().BeEquivalentTo(0);

            Assert.Catch<x02.GraphEnvironment<int, (int from, int to), x02.Unit>.CyclicException>(() =>
                new Case()
                .WithStartNodes(0)
                .WithEdges(
                    (from: 0, to: 1),
                    (from: 1, to: 2),
                    (from: 2, to: 2)
                )
                .Act()
            ).UnresolvableNodes.Should().BeEquivalentTo(2);
        }


        [Test]
        public void TestCyclic2()
        {
            Assert.Catch<x02.GraphEnvironment<int, (int from, int to), x02.Unit>.CyclicException>(() =>
                new Case()
                .WithStartNodes(2)
                .WithEdges(
                    (from: 0, to: 1),
                    (from: 1, to: 2),
                    (from: 2, to: 3),
                    (from: 3, to: 4),
                    (from: 1, to: 3),
                    (from: 0, to: 4)
                )
                .Act());
        }

        [Test]
        public void TestCyclic3()
        {
            Assert.Catch<x02.GraphEnvironment<int, (int from, int to), x02.Unit>.CyclicException>(() =>
                new Case()
                .WithStartNodes(2)
                .WithEdges(
                    (from: 0, to: 2),
                    (from: 1, to: 2),
                    (from: 2, to: 3),
                    (from: 2, to: 4)
                )
                .Act());
        }

    }
}
