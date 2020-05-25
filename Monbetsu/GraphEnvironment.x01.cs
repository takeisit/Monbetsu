// Copyright (c) takeisit. All Rights Reserved.
// https://github.com/takeisit/Monbetsu/
// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Monbetsu.x01
{

    public abstract partial class GraphEnvironment<TGraph, TNode, TEdge, TLabel>
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull
    {
        public delegate void LabelEdge(TNode fromNode, TEdge edge, TNode toNode, TLabel label);
        public delegate void LabelSeriesSubgraph(TNode startNode, TLabel sublebel, TNode endNode, TLabel label);
        public delegate void LabelParallelSubgraph(TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);
        public delegate void LabelKnotSubgraph(TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);

        protected abstract IEnumerable<(TEdge edge, TNode fromNode)> GetInflows(TGraph graph, TNode toNode);
        protected abstract IEnumerable<(TEdge edge, TNode toNode)> GetOutflows(TGraph graph, TNode fromNode);

        protected abstract TLabel CreateLabelFromEdge(TGraph graph, TNode fromNode, TEdge edge, TNode toNode);
        protected abstract TLabel CreateLabelFromSubgraph(TGraph graph, TNode startNode, TNode endNode);

        public void Classify(TGraph graph, IEnumerable<TNode> startNodes, LabelEdge? labelEdge, LabelSeriesSubgraph? labelSeries, LabelParallelSubgraph? labelParallel, LabelKnotSubgraph? labelKnot, CancellationToken cancellationToken = default)
        {
            if (startNodes?.Any() != true)
            {
                return;
            }

            labelEdge ??= (_0, _1, _2, _3) => { };
            labelSeries ??= (_0, _1, _2, _3) => { };
            labelParallel ??= (_0, _1, _2, _3) => { }; 
            labelKnot ??= (_0, _1, _2, _3) => { };

            var frontier = new Dictionary<TNode, Item>();
            var layereds = new List<Item>();
            var layeredDepth = -1;
            var interior = new Dictionary<TNode, Item>();
            Item item = default!;

            
            void OnNodeVisited(TNode node, int depth, int order, bool isLast)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!frontier.TryGetValue(node, out item!))
                {
                    item = new Item(node);
                }
                else
                {
                    frontier.Remove(node);
                }

                interior[node] = item;

                item.Depth = depth;
                
                if (depth != layeredDepth)
                {
                    layeredDepth = depth;
                    layereds.Clear();
                }

                while (item.Parents.Count > 1)
                {
                    do
                    {
                        BundleParallels(item, in graph, labelParallel!);
                    } while (item.Parents.Count > 1 && SettleSeries(item, in graph, labelSeries!));

                    if (item.Parents.Count > 1 && TieKnots(item, in graph, labelKnot!, frontier, layereds))
                    {
                        if (item.Parents.Count > 1)
                        {
                            SettleSeries(item, in graph, labelSeries!);
                        }
                    }
                    else
                    {
                        break;
                    }

                    // TODO sweep elements of the item's Ancestors which won't be a start node of any knots.
                }

                if (isLast && item.Parents.Count == 1)
                {
                    SettleSeries(item, in graph, labelSeries!);
                }

                layereds.Add(item);
            }


            void OnEdgeVisited(TNode parent, TEdge edge, TNode child, bool isSingle)
            {
                var parentItem = item;

                if (!frontier.TryGetValue(child, out var childItem))
                {
                    frontier[child] = childItem = new Item(child);
                }

                childItem.Ancestors.UnionWith(parentItem.Ancestors);
                childItem.Ancestors.Add(parentItem);

                TLabel label;
                if (isSingle && parentItem.Parents.Count == 1)
                {
                    label = parentItem.Parents[0].label;
                }
                else
                {
                    label = CreateLabelFromEdge(graph, parent, edge, child);
                }

                parentItem.ChildrenCount++;
                childItem.Parents.Add((parentItem, label));

                labelEdge!.Invoke(parent, edge, child, label);
            }

            void OnSweepableNodeVisited(TNode node, int depth)
            {
                if (interior.TryGetValue(node, out var sweepable))
                {
                    interior.Remove(node);
                    sweepable.ClearAncestors();
                }
            }

            TraverseTopologically(startNodes, OnNodeVisited, OnEdgeVisited, (_0, _1, _2, _3) => { }, (_0, _1) => { }, OnSweepableNodeVisited, GetInflows, GetOutflows, in graph);
        }

        private void BundleParallels(Item item, in TGraph graph, LabelParallelSubgraph labelParallel)
        {
            foreach (var g in item.Parents.GroupBy(p => p.item))
            {
                var count = g.Count();
                if (count > 1)
                {
                    TLabel label;
                    var parent = g.Key;

                    parent.ChildrenCount -= count - 1;
                    if (parent.IsSerial)
                    {
                        label = parent.Parents[0].label;
                    }
                    else
                    {
                        label = CreateLabelFromSubgraph(graph, parent.Node, item.Node);
                    }

                    labelParallel.Invoke(parent.Node, g.Select(p => p.label), item.Node, label);

                    item.Parents.RemoveAll(p => p.item == parent);
                    item.Parents.Add((parent, label));                    
                }
            }
        }

        private bool SettleSeries(Item item, in TGraph graph, LabelSeriesSubgraph labelSeries)
        {
            var isModified = false;
            for (var i = 0; i < item.Parents.Count; i++)
            {
                var p = item.Parents[i];
                if (p.item.IsSerial)
                {
                    do
                    {
                        item.Ancestors.Remove(p.item);
                        p = p.item.Parents[0];
                    } while (p.item.IsSerial);

                    var label = CreateLabelFromSubgraph(graph, p.item.Node, item.Node);

                    labelSeries!.Invoke(p.item.Node, item.Parents[i].label, item.Node, label);

                    item.Parents[i] = (p.item, label);

                    isModified = true;
                }
            }
            return isModified;
        }

        private bool TieKnots(Item item, in TGraph graph, LabelKnotSubgraph labelKnot, Dictionary<TNode, Item> nexts, List<Item> prevs)
        {
            static ILookup<int, int> SplitToSubknots(List<(int index, HashSet<Item> ancestors)> flows)
            {
                var disjointSet = new DisjointSet(flows.Count);
                var ancestorSlots = new Dictionary<Item, int>();
                for (var i = 0; i < flows.Count; i++)
                {
                    foreach (var ancestor in flows[i].ancestors)
                    {
                        if (ancestorSlots.TryGetValue(ancestor, out var group))
                        {
                            disjointSet.Unite(i, group);
                        }
                        else
                        {
                            ancestorSlots[ancestor] = i;
                        }
                    }
                }

                return disjointSet.ToLookup();
            }

            var isModified = false;

            foreach (var g in item.Parents.SelectMany((p, i) => p.item.Ancestors.Select(ancestor => (ancestor, index: i))).ToLookup(p => p.ancestor, p => p.index).OrderByDescending(g => g.Key.Depth))
            {
                if (g.Count() == 1)
                {
                    continue;
                }

                var start = g.Key;
                var flowsToEnd = new List<(int index, HashSet<Item> ancestors)>();
                foreach (var i in g)
                {
                    var parent = item.Parents[i];

                    var ancestorsFromStart = new HashSet<Item>(parent.item.Ancestors.Append(parent.item).Where(ancestor => ancestor.Depth > start.Depth));
                    var noFlowFromOthers = ancestorsFromStart.All(ancestor => ancestor.Parents.All(gp => gp.item.Depth > start.Depth || gp.item == start));
                    if (noFlowFromOthers)
                    {
                        var noFlowToOthers = ancestorsFromStart.All(ancestor => !nexts.Any(next => next.Value.Ancestors.Contains(ancestor)) && !prevs.Any(prev => prev.Ancestors.Contains(ancestor)));
                        if (noFlowToOthers)
                        {
                            flowsToEnd.Add((i, ancestorsFromStart));
                        }
                    }
                }

                if (flowsToEnd.Count > 1)
                {
                    var processedIndices = new List<int>(flowsToEnd.Count);
                    foreach (var group in SplitToSubknots(flowsToEnd).Where(g => g.Take(2).Count() > 1))
                    {
                        var backStack = new Stack<Item>();
                        var sublabels = new HashSet<TLabel>();
                        foreach (var i in group)
                        {
                            var p = item.Parents[flowsToEnd[i].index];

                            backStack.Push(p.item);
                            sublabels.Add(p.label);
                            processedIndices.Add(flowsToEnd[i].index);
                        }

                        while (backStack.Count > 0)
                        {
                            var ancestor = backStack.Pop();

                            if (item.Ancestors!.Remove(ancestor))
                            {
                                foreach (var (parent, sublabel) in ancestor.Parents)
                                {
                                    sublabels.Add(sublabel);
                                    if (parent != start)
                                    {
                                        backStack.Push(parent);
                                    }
                                    else
                                    {
                                        start.ChildrenCount--;
                                    }
                                }
                            }
                        }

                        TLabel label;

                        start.ChildrenCount++;
                        if (start.IsSerial)
                        {
                            label = start.Parents[0].label;
                        }
                        else
                        {
                            label = CreateLabelFromSubgraph(graph, start.Node, item.Node);
                        }
                        labelKnot!.Invoke(start.Node, sublabels, item.Node, label);

                        item.Parents.Add((start, label));

                        isModified = true;
                    }

                    var pad = 0;
                    processedIndices.Sort();
                    foreach (var i in processedIndices)
                    {
                        item.Parents.RemoveAt(i - pad++);
                    }

                    if (isModified)
                    {
                        break;
                    }
                }
            }

            return isModified;
        }

        private static void TraverseTopologically(
                IEnumerable<TNode> startNodes,
                Action<TNode, int, int, bool> nodeVisitor,
                Action<TNode, TEdge, TNode, bool> edgeVisitor,
                Action<TNode, int, int, int> postNodeVisitor,
                Action<TNode, int> sweepedParentNodeVisitor,
                Action<TNode, int> sweepedGrandparentNodeVisitor,
                Func<TGraph, TNode, IEnumerable<(TEdge edge, TNode fromNode)>> inflows,
                Func<TGraph, TNode, IEnumerable<(TEdge edge, TNode toNode)>> outflows,
                in TGraph graph,
                bool strictlySweepAll = false
                )
        {
            var sweepee = new Dictionary<TNode, (int depth, int count)>();
            var g = graph;

            TraverseTopologically(startNodes, nodeVisitor, edgeVisitor,
                (node, depth, order, childCount) =>
                {
                    sweepee[node] = (depth, childCount);
                    postNodeVisitor(node, depth, order, childCount);
                },
                (node, depth) =>
                {
                    sweepedParentNodeVisitor(node, depth);
                    foreach (var (_, fromNode) in inflows(g, node))
                    {
                        if (sweepee.TryGetValue(fromNode, out var p))
                        {
                            var (d, count) = p;
                            sweepee[fromNode] = (d, --count);
                            if (count == 0)
                            {
                                sweepee.Remove(fromNode);
                                sweepedGrandparentNodeVisitor(fromNode, d);
                            }
                        }
                    }
                },
                inflows, outflows,
                in graph,
                strictlySweepAll
                );

            if (strictlySweepAll)
            {
                foreach (var (node, d) in sweepee.Where(kv => kv.Value.depth >= 0).OrderBy(kv => kv.Value.depth).Select(kv => (kv.Key, kv.Value.depth)))
                {
                    sweepedGrandparentNodeVisitor(node, d);
                }
            }
        }

        private static void TraverseTopologically(
                IEnumerable<TNode> startNodes,
                Action<TNode, int, int, bool> nodeVisitor,
                Action<TNode, TEdge, TNode, bool> edgeVisitor,
                Action<TNode, int, int, int> postNodeVisitor,
                Action<TNode, int> sweepedParentNodeVisitor,
                Func<TGraph, TNode, IEnumerable<(TEdge edge, TNode fromNode)>> inflows,
                Func<TGraph, TNode, IEnumerable<(TEdge edge, TNode toNode)>> outflows,
                in TGraph graph,
                bool strictlySweepAll = false
                )
        {
            var queue = new HashSet<TNode>(startNodes);
            
            var list = new List<TNode>();
            var nexts = new HashSet<TNode>();
            var yieldeds = new Dictionary<TNode, (int depth, int count)>();
            var depth = -1;
            var order = 0;
            var tails = new HashSet<TNode>();

            void TrySweep(in TGraph g, in TNode node)
            {
                foreach (var (_, fromNode) in inflows(g, node))
                {
                    var (d, count) = yieldeds[fromNode];
                    yieldeds[fromNode] = (d, --count);
                    if (count == 0)
                    {
                        yieldeds.Remove(fromNode);

                        if (d >= 0)
                        {
                            sweepedParentNodeVisitor(fromNode, d);
                        }
                    }
                }
            }

            while (queue.Count > 0)
            {
                depth = checked(depth + 1);

                foreach (var node in queue)
                {
                    var isYieldedAll = true;

                    foreach (var (_, fromNode) in inflows(graph, node))
                    {
                        if (!yieldeds.ContainsKey(fromNode))
                        {
                            isYieldedAll = false;
                            break;
                        }
                    }

                    if (isYieldedAll)
                    {
                        list.Add(node);
                    }
                    else
                    {
                        nexts.Add(node);
                    }
                }

                if (list.Count == 0)
                {
                    throw new CyclicException(nexts);
                }

            
                for (var i = 0; i < list.Count; i++)
                {
                    var node = list[i];

                    var childCount = 0;
                    
                    using var it = outflows(graph, node).GetEnumerator();
                    if (it.MoveNext())
                    {
                        nodeVisitor(node, depth, order, false);

                        var (edge, toNode) = it.Current;

                        nexts.Add(toNode);

                        childCount = 1;
                        if (it.MoveNext())
                        {
                            edgeVisitor(node, edge, toNode, false);

                            do
                            {
                                childCount++;
                                (edge, toNode) = it.Current;
                                nexts.Add(toNode);
                                edgeVisitor(node, edge, toNode, false);
                            }
                            while (it.MoveNext());
                        }
                        else
                        {
                            edgeVisitor(node, edge, toNode, true);
                        }

                        postNodeVisitor(node, depth, order, childCount);
                            
                        TrySweep(in graph, in node);
                        order++;

                        yieldeds[node] = (depth, childCount);
                    }
                    else
                    {
                        tails.Add(node);
                    }
                }
                                
                queue.Clear();

                var t = queue;
                queue = nexts;
                nexts = t;

                list.Clear();
            }

            foreach (var tail in tails)
            {
                nodeVisitor(tail, depth, order, true);
                postNodeVisitor(tail, depth, order++, 0);
                
                TrySweep(in graph, in tail);
            }

            if (strictlySweepAll)
            {
                foreach(var (node, d) in yieldeds.Where(kv => kv.Value.depth >= 0).OrderBy(kv => kv.Value.depth).Select(kv => (kv.Key, kv.Value.depth)))
                {
                    sweepedParentNodeVisitor(node, d);
                }

                foreach (var tail in tails)
                {
                    sweepedParentNodeVisitor(tail, depth);
                }
            }
        }


        [DebuggerDisplay("{Node} parent#={Parents.Count} ancestor#={Ancestors?.Count} child#={ChildrenCount}")]
        class Item
        {
            private static readonly HashSet<Item> Consumed = new HashSet<Item>();

            public TNode Node { get; }

            public int Depth { get; set; } = int.MaxValue;
            
            public int ChildrenCount { get; set; } = 0;

            public bool IsSerial => Parents.Count == 1 && ChildrenCount == 1;

            public HashSet<Item> Ancestors { get; private set; } = new HashSet<Item>();
            public List<(Item item, TLabel label)> Parents { get; } = new List<(Item item, TLabel label)>();

            public Item(TNode raw)
            {
                Node = raw;
            }

            public void ClearAncestors() => Ancestors = Consumed;

            public override string ToString()
            {
                return $"{Node} parent#={Parents.Count} ancestor#={Ancestors?.Count} child#={ChildrenCount}";
            }
        }

        class DisjointSet
        {
            private readonly (int group, int count)[] array;
            public DisjointSet(int length)
            {
                array = new (int group, int count)[length];
                for (var i = 0; i < length; i++)
                {
                    array[i] = (i, 1);
                }
            }

            public bool Unite(int a, int b)
            {
                a = Root(a);
                b = Root(b);

                if (a == b)
                {
                    return false;
                }

                (a, b) = array[a].count < array[b].count ? (b, a) : (a, b);

                array[a] = (a, array[a].count + array[b].count);
                array[b] = (a, array[b].count);

                return true;
            }

            public int Root(int i)
            {
                while (array[i].group != i)
                {
                    i = array[i].group;
                }
                return i;
            }

            public bool IsUnited(int a, int b) => Root(a) == Root(b);

            public ILookup<int, int> ToLookup()
            {
                return array.Select((a, i) => (a, i)).ToLookup(o => Root(o.a.group), o => o.i);
            }
        }

        public class CyclicException : MonbetuException
        {
            public IReadOnlyCollection<TNode> UnresolvableNodes { get; }
            internal CyclicException(IReadOnlyCollection<TNode> unresolvableNodes)
                : base("Some cycles of path, wrong start nodes, or edges which are unreachable from any of the start nodes are detected.")
            {
                UnresolvableNodes = unresolvableNodes;
            }
        }
    }



    public abstract class GraphEnvironment<TNode, TEdge, TLabel> : GraphEnvironment<Unit, TNode, TEdge, TLabel>
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull
    {
        protected sealed override IEnumerable<(TEdge edge, TNode fromNode)> GetInflows(Unit _, TNode toNode) => GetInflows(toNode);
        protected sealed override IEnumerable<(TEdge edge, TNode toNode)> GetOutflows(Unit _, TNode fromNode) => GetOutflows(fromNode);
        protected sealed override TLabel CreateLabelFromEdge(Unit _, TNode fromNode, TEdge edge, TNode toNode) => CreateLabelFromEdge(fromNode, edge, toNode);
        protected sealed override TLabel CreateLabelFromSubgraph(Unit _, TNode startNode, TNode endNode) => CreateLabelFromSubgraph(startNode, endNode);

        protected abstract IEnumerable<(TEdge edge, TNode fromNode)> GetInflows(TNode toNode);
        protected abstract IEnumerable<(TEdge edge, TNode toNode)> GetOutflows(TNode fromNode);

        protected abstract TLabel CreateLabelFromEdge(TNode fromNode, TEdge edge, TNode toNode);
        protected abstract TLabel CreateLabelFromSubgraph(TNode startNode, TNode endNode);

        public void Classify(IEnumerable<TNode> startNodes, LabelEdge? labelEdge, LabelSeriesSubgraph? labelSeries, LabelParallelSubgraph? labelParallel, LabelKnotSubgraph? labelComplex, CancellationToken cancellationToken = default)
            => Classify(default, startNodes, labelEdge, labelSeries, labelParallel, labelComplex, cancellationToken);
    }

    public struct Unit { }

    public class MonbetuException : Exception
    {
        protected internal MonbetuException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
    }

    

    
}
