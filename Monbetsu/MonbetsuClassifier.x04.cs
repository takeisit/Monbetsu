// Copyright (c) takeisit. All Rights Reserved.
// https://github.com/takeisit/Monbetsu/
// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Monbetsu.x04
{
    using Internals;

    public static partial class MonbetsuClassifier
    {
        public static void Classify<TGraph, TNode, TEdge, TLabel>(TGraph graph, IEnumerable<TNode> startNodes, IGraphStructure<TGraph, TNode, TEdge> graphStructure, ILabelFactory<TGraph, TNode, TEdge, TLabel> labelFactoy, ILabeler<TGraph, TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
            where TGraph : notnull
            where TNode : notnull
            where TEdge : notnull
            where TLabel : notnull
        {
            MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.Classify(graph, startNodes, graphStructure, labelFactoy, labeler, cancellationToken);
        }

        public static void Classify<TGraph, TNode, TEdge, TLabel>(TGraph graph, IEnumerable<TNode> startNodes, FlowGetter<TGraph, TNode, TEdge> getOutflows, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
            where TGraph : notnull
            where TNode : notnull
            where TEdge : notnull
            where TLabel : notnull
        {
            MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.Classify(graph, startNodes, getOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }

        public static void Classify<TNode, TEdge, TLabel>(IEnumerable<TNode> startNodes, IGraphStructure<TNode, TEdge> graphStructure, ILabelFactory<TNode, TEdge, TLabel> labelFactoy, ILabeler<TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
            where TNode : notnull
            where TEdge : notnull
            where TLabel : notnull
        {
            MonbetsuClassifier<TNode, TEdge, TLabel>.Classify(startNodes, graphStructure, labelFactoy, labeler, cancellationToken);
        }

        public static void Classify<TNode, TEdge, TLabel>(IEnumerable<TNode> startNodes, FlowGetter<TNode, TEdge> getOutflows, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
            where TNode : notnull
            where TEdge : notnull
            where TLabel : notnull
        {
            MonbetsuClassifier<TNode, TEdge, TLabel>.Classify(startNodes, getOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }
    }

    public interface IGraphStructure<TGraph, TNode, TEdge>
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
    {
        IEnumerable<(TEdge, TNode)> GetOutFlows(TGraph graph, TNode node);
    }

    public interface IGraphStructure<TNode, TEdge>
        where TNode : notnull
        where TEdge : notnull
    {
        IEnumerable<(TEdge, TNode)> GetOutflows(TNode node);
    }

    public interface ILabelFactory<TGraph, TNode, TEdge, TLabel>
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull
    {
        TLabel CreateFromEdge(TGraph graph, TNode fromNode, TEdge edge, TNode toNode);
        TLabel CreateFromSubgraph(TGraph graph, TNode startNode, TNode endNode);
    }

    public interface ILabelFactory<TNode, TEdge, TLabel>
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull
    {
        TLabel CreateFromEdge(TNode fromNode, TEdge edge, TNode toNode);
        TLabel CreateFromSubgraph(TNode startNode, TNode endNode);
    }


    public interface ILabeler<TGraph, TNode, TEdge, TLabel>
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull
    {
        void LabelEdge(TGraph graph, TNode fromNode, TEdge edge, TNode toNode, TLabel label);
        void LabelSeriesSubgraph(TGraph graph, TNode startNode, TLabel sublebel, TNode endNode, TLabel label);
        void LabelParallelSubgraph(TGraph graph, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);
        void LabelKnotSubgraph(TGraph graph, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);
    }

    public interface ILabeler<TNode, TEdge, TLabel>
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull
    {
        void LabelEdge(TNode fromNode, TEdge edge, TNode toNode, TLabel label);
        void LabelSeriesSubgraph(TNode startNode, TLabel sublebel, TNode endNode, TLabel label);
        void LabelParallelSubgraph(TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);
        void LabelKnotSubgraph(TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);
    }

    public delegate void EdgeLabeler<TGraph, TNode, TEdge, TLabel>(TGraph graph, TNode fromNode, TEdge edge, TNode toNode, TLabel label)
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull;

    public delegate void EdgeLabeler<TNode, TEdge, TLabel>(TNode fromNode, TEdge edge, TNode toNode, TLabel label)
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull;

    public delegate void SeriesSubgraphLabeler<TGraph, TNode, TLabel>(TGraph graph, TNode startNode, TLabel sublebel, TNode endNode, TLabel label)
        where TGraph : notnull
        where TNode : notnull
        where TLabel : notnull;

    public delegate void SeriesSubgraphLabeler<TNode, TLabel>(TNode startNode, TLabel sublebel, TNode endNode, TLabel label)
        where TNode : notnull
        where TLabel : notnull;

    public delegate void ParallelSubgraphLabeler<TGraph, TNode, TLabel>(TGraph graph, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label)
        where TGraph : notnull
        where TNode : notnull
        where TLabel : notnull;

    public delegate void ParallelSubgraphLabeler<TNode, TLabel>(TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label)
        where TNode : notnull
        where TLabel : notnull;

    public delegate void KnotSubgraphLabeler<TGraph, TNode, TLabel>(TGraph graph, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label)
        where TGraph : notnull
        where TNode : notnull
        where TLabel : notnull;

    public delegate void KnotSubgraphLabeler<TNode, TLabel>(TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label)
        where TNode : notnull
        where TLabel : notnull;

    public delegate IEnumerable<(TEdge, TNode)> FlowGetter<TGraph, TNode, TEdge>(TGraph graph, TNode node)
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull;

    public delegate IEnumerable<(TEdge, TNode)> FlowGetter<TNode, TEdge>(TNode node)
        where TNode : notnull
        where TEdge : notnull;

    public delegate TLabel EdgeLabelCreator<TGraph, TNode, TEdge, TLabel>(TGraph graph, TNode fromNode, TEdge edge, TNode toNode)
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull;

    public delegate TLabel EdgeLabelCreator<TNode, TEdge, TLabel>(TNode fromNode, TEdge edge, TNode toNode)
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull;

    public delegate TLabel SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel>(TGraph graph, TNode startNode, TNode endNode)
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull;

    public delegate TLabel SubgraphLabelCreator<TNode, TEdge, TLabel>(TNode startNode, TNode endNode)
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull;

    public abstract partial class MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull
    {

        protected abstract IEnumerable<(TEdge edge, TNode toNode)> GetOutflows(TGraph graph, TNode fromNode);

        public void Classify(TGraph graph, IEnumerable<TNode> startNodes, ILabelFactory<TGraph, TNode, TEdge, TLabel> labelFactory, ILabeler<TGraph, TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            Classify(graph, startNodes, GetOutflows, labelFactory.CreateFromEdge, labelFactory.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public void Classify(TGraph graph, IEnumerable<TNode> startNodes, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            Classify(graph, startNodes, GetOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }

        public static void Classify(TGraph graph, IEnumerable<TNode> startNodes, IGraphStructure<TGraph, TNode, TEdge> graphStructure, ILabelFactory<TGraph, TNode, TEdge, TLabel> labelFactoy, ILabeler<TGraph, TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            Classify(graph, startNodes, graphStructure.GetOutFlows, labelFactoy.CreateFromEdge, labelFactoy.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public static void Classify(TGraph graph, IEnumerable<TNode> startNodes, FlowGetter<TGraph, TNode, TEdge> getOutflows, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            var labeler = new Labeler(graph, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot);
            var (list, maxParentCount) = BuildTopology(in graph, getOutflows, startNodes, cancellationToken);
            var pool = new Pool(maxParentCount);

            var order = list.Count - 1;
            for (var i = 0; i < list.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = list[i];
                item.Order = order--;
                item.TwistDescendants();
            }


            for (var i = list.Count - 1; i >= 0; i--)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = list[i];

                item.ReduceDescendants();
                item.TwistAncestors();
            }

#if MONBETSU_DEBUG_DUMP
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                item.ReduceAncestors();

                Debug.WriteLine(list[i].Dump());
            }
#endif

            for (var i = list.Count - 1; i >= 0; i--)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = list[i];
#if !MONBETSU_DEBUG_DUMP
                item.ReduceAncestors();
#endif

                item.LabelEdges(labeler);

                if (item.Parents.Count > 1)
                {
                    item.SettleSeries(labeler);
                    item.TryMerge(labeler, pool);
                }

                if (item.Children.Count == 0 && item.Parents.Count == 1)
                {
                    item.SettleSeries(labeler);
                }
            }
        }

        private static (IReadOnlyList<Item> list, int maxParentCount) BuildTopology(in TGraph graph, FlowGetter<TGraph, TNode, TEdge> getOutflows, IEnumerable<TNode> startNodes, CancellationToken cancellationToken = default)
        {
            var stack = new Stack<(MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.Item item, bool post)>();
            var seens = new Dictionary<TNode, Item>();
            var list = new List<Item>();
            var localSeens = new HashSet<Item>();
            var maxParentCount = 0;

            foreach (var startNode in startNodes)
            {
                if (!seens.TryGetValue(startNode, out var item))
                {
                    seens[startNode] = item = new Item(startNode);
                    stack.Push((item, false));
                }
            }

            while (stack.Count > 0)
            {
                var (parent, post) = stack.Pop();

                if (!post && !parent.IsStatusVisited)
                {
                    stack.Push((parent, true));

                    parent.MarkAsPrevisited();

                    foreach (var (edge, toNode) in getOutflows(graph, parent.Node))
                    {
                        if (!seens.TryGetValue(toNode, out var child))
                        {
                            seens[toNode] = child = new Item(toNode);
                        }
                        else if (child.IsStatusPrevisited)
                        {
                            throw new CyclicException<TNode>(child.Node);
                        }

                        child.Parents.Add(new ItemWithEdge(parent, edge));
                        maxParentCount = Math.Max(maxParentCount, child.Parents.Count);
                        parent.Children.Add(child);

                        if (localSeens.Add(child))
                        {
                            stack.Push((child, false));
                        }
                    }
                    localSeens.Clear();
                }
                else if (parent.IsStatusPrevisited)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    parent.MarkAsVisited();
                    list.Add(parent);
                }
            }

            seens.Clear();

            return (list, maxParentCount);
        }

        class Labeler
        {
            private readonly TGraph graph;
            private readonly EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge;
            private readonly SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph;
            private readonly EdgeLabeler<TGraph, TNode, TEdge, TLabel> labelEdge;
            private readonly SeriesSubgraphLabeler<TGraph, TNode, TLabel> labelSeries;
            private readonly ParallelSubgraphLabeler<TGraph, TNode, TLabel> labelParallel;
            private readonly KnotSubgraphLabeler<TGraph, TNode, TLabel> labelKnot;

            public Labeler(
                TGraph graph,
                EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge,
                SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph,
                EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge,
                SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, 
                ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, 
                KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot)
            {
                this.graph = graph;
                this.createLabelFromEdge = createLabelFromEdge ?? throw new ArgumentNullException(nameof(createLabelFromEdge));
                this.createLabelFromSubgraph = createLabelFromSubgraph ?? throw new ArgumentNullException(nameof(createLabelFromSubgraph));
                this.labelEdge = labelEdge ?? ((_0, _1, _2, _3, _4) => { });
                this.labelSeries = labelSeries ?? ((_0, _1, _2, _3, _4) => { });
                this.labelParallel = labelParallel ?? ((_0, _1, _2, _3, _4) => { });
                this.labelKnot = labelKnot ?? ((_0, _1, _2, _3, _4) => { });
            }

            internal IItemWithLabel LabelEdge(IItemWithLabel parent, Item child)
            {
                if (parent is ItemWithEdge rawEdgeContainer)
                {
                    var edge = rawEdgeContainer.Edge;

                    TLabel label;
                    if (parent.Item.IsSerial)
                    {
                        label = parent.Item.Parents[0].Label;
                    }
                    else
                    {
                        label = createLabelFromEdge(graph, parent.Item.Node, edge, child.Node);
                    }
                    var newParent = new ItemWithLabel(parent.Item, label);
                    labelEdge(graph, parent.Item.Node, edge, child.Node, label);
                    return newParent;
                }
                else
                {
                    return parent;
                }
            }

            internal IItemWithLabel LabelSeries(Item start, TLabel sublabel, Item end)
            {
                var label = createLabelFromSubgraph(graph, start.Node, end.Node);
                labelSeries(graph, start.Node, sublabel, end.Node, label);

                return new ItemWithLabel(start, label);
            }

            internal (IItemWithLabel newParent, bool isLabelInherited) LabelParallel(Item parent, IEnumerable<TLabel> sublabels, Item child, bool isLabelInheritable)
            {
                TLabel label;
                var isLabelInherited = false;
                if (parent.Parents.Count == 1 && isLabelInheritable)
                {
                    label = parent.Parents[0].Label;
                    isLabelInherited = true;
                }
                else
                {
                    label = createLabelFromSubgraph(graph, parent.Node, child.Node);
                }

                labelParallel(graph, parent.Node, sublabels, child.Node, label);

                return (new ItemWithLabel(parent, label), isLabelInherited);
            }

            internal (IItemWithLabel newParent, bool isLabelInherited) LabelKnot(Item parent, IEnumerable<TLabel> sublabels, Item child, bool isLabelInheritable)
            {
                TLabel label;
                var isLabelInherited = false;
                if (parent.Parents.Count == 1 && isLabelInheritable)
                {
                    label = parent.Parents[0].Label;
                    isLabelInherited = true;
                }
                else
                {
                    label = createLabelFromSubgraph(graph, parent.Node, child.Node);
                }

                labelKnot(graph, parent.Node, sublabels, child.Node, label);

                return (new ItemWithLabel(parent, label), isLabelInherited);
            }
        }

        interface IItemWithLabel
        {
            Item Item { get; }
            TLabel Label { get; }
        }

        [DebuggerDisplay("Edge {Item.Node}: {Edge}")]
        class ItemWithEdge : IItemWithLabel
        {
            public Item Item { get; }
            public TEdge Edge { get; }

            public TLabel Label => throw new InvalidOperationException();

            public ItemWithEdge(Item item, TEdge edge)
            {
                Item = item;
                Edge = edge;
            }

        }

        [DebuggerDisplay("Subgraph To {Item.Node}: {Label}")]
        class ItemWithLabel : IItemWithLabel
        {
            public Item Item { get; }
            public TLabel Label { get; }

            public ItemWithLabel(Item item, TLabel label)
            {
                Item = item;
                Label = label;
            }
        }

        enum CheckpointLevel
        {
            None,
            Subcommon,
            Common
        }


        interface IItemComposite : IReadOnlyCollection<Item>
        {
            Item First { get; }

            IItemComposite Reduce();
            
            bool IntersectsWith(IItemComposite other);
        }

        class EmptyItemComposite : IItemComposite
        {
            internal static readonly EmptyItemComposite Instance = new EmptyItemComposite();

            public int Count => 0;

            public Item First => throw new InvalidOperationException("no element");

            public IEnumerator<Item> GetEnumerator()
            {
                yield break;
            }

            public IItemComposite Reduce() => this;
            public bool IntersectsWith(IItemComposite other) => false;

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        class SubcommonItemsComposite : HashSet<Item>, IItemComposite
        {
            public IItemComposite Reduce() => Count == 0 ? EmptyItemComposite.Instance : Count == 1 ? this.First() as IItemComposite : this;
            public Item First => this.First();

            public bool IntersectsWith(IItemComposite other)
            {
                return other.Count switch
                {
                    0 => false,
                    1 => Contains(other.First),
                    _ => (other as HashSet<Item>)!.IntersectsWith(this)
                };
            }
        }

        class CommonItemsComposite : HashSet<Item>, IItemComposite
        {
            internal IItemComposite? Nearest { get; set; } = null;

            public Item First => this.First();

            public CommonItemsComposite() { }

            public CommonItemsComposite(IEnumerable<Item> collection, Item nearest) : base(collection)
            {
                this.Nearest = nearest;
                Add(nearest);
            }


            public IItemComposite Reduce() => Nearest ?? EmptyItemComposite.Instance;

            public bool IntersectsWith(IItemComposite other) => throw new InvalidOperationException("must be reduced");
        }


        [DebuggerDisplay("{Node} parent#={Parents.Count} ancestor#={CommonAncestors.Count}+{SubcommonAncestors.Count} child#={Children.Count} descendant#={CommonDescendants.Count}+{SubcommonDescendants.Count}, Order={Order}")]
        class Item : IItemComposite
        {
            internal static readonly IItemComposite Previsited = new EmptyItemComposite();
            internal static readonly IItemComposite Visited = new EmptyItemComposite();

            public TNode Node { get; }

            public IItemComposite CommonDescendants { get; private set; } = EmptyItemComposite.Instance;
            public IItemComposite SubcommonDescendants { get; private set; } = EmptyItemComposite.Instance;
            
            public List<IItemWithLabel> Parents { get; } = new List<IItemWithLabel>();
            public List<Item> Children { get; } = new List<Item>();

            public IItemComposite CommonAncestors { get; private set; } = EmptyItemComposite.Instance;
            public IItemComposite SubcommonAncestors { get; private set; } = EmptyItemComposite.Instance;

            public IItemComposite CommonDescendants2 => CommonDescendants;
            public IItemComposite SubcommonDescendants2 => SubcommonDescendants;

            public IItemComposite CommonAncestors2 => CommonAncestors;
            public IItemComposite SubcommonAncestors2 => SubcommonAncestors;


            public int Order { get; set; }

            public int Depth { get; set; } = -1;
            public int ReversedDepth { get; set; } = -1;

            public bool IsSerial => Parents.Count == 1 && Children.Count == 1;
            
            public bool IsStatusVisited => SubcommonAncestors == Visited;
            
            public bool IsStatusPrevisited => SubcommonAncestors == Previsited;

            int IReadOnlyCollection<Item>.Count => 1;
            Item IItemComposite.First => this;

            public Item(TNode node)
            {
                Node = node;
            }

            internal void MarkAsVisited()
            {
                SubcommonAncestors = Visited;
            }

            internal void MarkAsPrevisited()
            {
                SubcommonAncestors = Previsited;
            }

            IItemComposite IItemComposite.Reduce() => this;
            bool IItemComposite.IntersectsWith(IItemComposite other)
            {
                return other.Count switch
                {
                    0 => false,
                    1 => this == other.First,
                    _ => (other as HashSet<Item>)!.Contains(this)
                };
            }


            internal string Dump()
            {
                string J(IEnumerable<MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.Item> source)
                {
                    if (!typeof(IComparable<TNode>).IsAssignableFrom(typeof(TNode)))
                    {
                        return string.Join(",", source.Select(_ => _.Node));
                    }
                    else
                    {
                        return string.Join(",", source.Select(_ => _.Node).OrderBy(_ => _));
                    }
                }

                return $"{Node}: Common={J(CommonAncestors)}/{J(CommonDescendants)}, Subcommon={J(SubcommonAncestors)}/{J(SubcommonDescendants)}";
            }

            internal void TwistAncestors()
            {
                var maxDepth = -1;
                using var it = Parents.Select(c => c.Item).GetEnumerator();
                if (it.MoveNext())
                {
                    var first = it.Current;
                    maxDepth = first.Depth;
                    if (it.MoveNext())
                    {
                        var counter = first.CommonAncestors.ToDictionary(item => item, _ => 1);
                        if (!counter.ContainsKey(first))
                        {
                            counter[first] = 1;
                        }

                        do
                        {
                            maxDepth = Math.Max(maxDepth, it.Current.Depth);
                            foreach (var item in it.Current.CommonAncestors.Append(it.Current))
                            {
                                counter.TryGetValue(item, out var existing);
                                counter[item] = existing + 1;
                            }
                        }
                        while (it.MoveNext());

                        var common = new CommonItemsComposite();
                        var subcommon = new SubcommonItemsComposite();
                        var nearest = default(Item);

                        foreach (var kv in counter)
                        {
                            if (kv.Value == Parents.Count)
                            {
                                if (nearest == null || nearest.Order < kv.Key.Order)
                                {
                                    nearest = kv.Key;
                                }
                                common.Add(kv.Key);
                            }
                            else
                            {
                                if (kv.Value > 1)
                                {
                                    subcommon.Add(kv.Key);
                                }
                            }
                        }

                        common.Nearest = nearest;
                        CommonAncestors = common;
                        SubcommonAncestors = subcommon;
                    }
                    else if (first.Children.Count > 1)
                    {
                        CommonAncestors = new CommonItemsComposite(first.CommonAncestors, first);
                    }
                    else
                    {
                        CommonAncestors = first.CommonAncestors;
                    }
                }
                Depth = maxDepth + 1;
            }

            internal void TwistDescendants()
            {
                var maxReversedDepth = -1;
                using var it = Children.GetEnumerator();
                if (it.MoveNext())
                {
                    var first = it.Current;
                    maxReversedDepth = first.ReversedDepth;
                    if (it.MoveNext())
                    {
                        var counter = first.CommonDescendants.ToDictionary(item => item, _ => 1);
                        if (!counter.ContainsKey(first))
                        {
                            counter[first] = 1;
                        }

                        do
                        {
                            maxReversedDepth = Math.Max(maxReversedDepth, it.Current.ReversedDepth);
                            foreach (var item in it.Current.CommonDescendants.Append(it.Current))
                            {
                                counter.TryGetValue(item, out var existing);
                                counter[item] = existing + 1;
                            }
                        }
                        while (it.MoveNext());

                        var common = new CommonItemsComposite();
                        var subcommon = new SubcommonItemsComposite();
                        var nearest = default(Item);

                        foreach (var kv in counter)
                        {
                            if (kv.Value == Children.Count)
                            {
                                if (nearest == null || nearest.Order > kv.Key.Order)
                                {
                                    nearest = kv.Key;
                                }

                                common.Add(kv.Key);
                            }
                            else
                            {
                                if (kv.Value > 1)
                                {
                                    subcommon.Add(kv.Key);
                                }
                            }
                        }

                        common.Nearest = nearest;
                        CommonDescendants = common;
                        SubcommonDescendants = subcommon;
                    }
                    else if (first.Parents.Count > 1)
                    {
                        CommonDescendants = new CommonItemsComposite(first.CommonDescendants, first);
                    }
                    else
                    {
                        CommonDescendants = first.CommonDescendants;
                    }
                }
                ReversedDepth = maxReversedDepth + 1;
            }

            private CheckpointLevel GetAncestorCheckpointLevelFor(Item other)
            {
                if (CommonAncestors2.IntersectsWith(other))
                {
                    return CheckpointLevel.Common;
                }
                else if (SubcommonAncestors2.IntersectsWith(other))
                {
                    return CheckpointLevel.Subcommon;
                }
                return CheckpointLevel.None;
            }

            internal void LabelEdges(Labeler labeler)
            {
                for (var p = 0; p < Parents.Count; p++)
                {
                    Parents[p] = labeler.LabelEdge(Parents[p], this);
                }
            }

            internal bool SettleSeries(Labeler labeler)
            {
                var isModified = false;

                for (var i = 0; i < Parents.Count; i++)
                {
                    var p = Parents[i].Item;
                    if (p.IsSerial)
                    {
                        SettleSeries(labeler, i);

                        isModified = true;
                    }
                }
                return isModified;
            }

            private IItemWithLabel SettleSeries(Labeler labeler, int parentIndex)
            {
                var p = Parents[parentIndex].Item;
                Item pp;
                do
                {
                    pp = p;
                    p = p.Parents[0].Item;
                } while (p.IsSerial);

                Parents[parentIndex] = labeler.LabelSeries(p, Parents[parentIndex].Label, this);      // TODO update itself to reduce allocation
                p.Children[p.Children.IndexOf(pp)] = this;

                return Parents[parentIndex];
            }

            private (bool onPath, CheckpointLevel checkpointLevel) IsOnMergablePath(Item item, int sourceIndex, Pool pool)
            {
                if (pool.Drops[sourceIndex])
                {
                    return (false, CheckpointLevel.None);
                }

                var checkpointLevel = CheckpointLevel.None;
                if (!item.CommonDescendants2.IntersectsWith(this))
                {
                    if (item.SubcommonDescendants2.IntersectsWith(this))
                    {
                        checkpointLevel = GetAncestorCheckpointLevelFor(item);
                    }

                    if (checkpointLevel == CheckpointLevel.None)
                    {
                        pool.Drops[sourceIndex] = true;
                        return (false, CheckpointLevel.None);
                    }
                }
                else
                {
                    checkpointLevel = GetAncestorCheckpointLevelFor(item);
                }

                if (checkpointLevel == CheckpointLevel.None && !CommonAncestors2.IntersectsWith(item.CommonAncestors2) && !SubcommonAncestors2.IntersectsWith(item.CommonAncestors2))
                {
                    pool.Drops[sourceIndex] = true;
                    return (false, CheckpointLevel.None);
                }

                return (true, checkpointLevel);
            }

            private void EnqueueIfOnPath(Item child, IItemWithLabel parent, int sourceIndex, Pool pool)
            {
                var (onPath, checkpointLevel) = IsOnMergablePath(parent.Item, sourceIndex, pool);

                if (onPath)
                {
                    if (checkpointLevel > CheckpointLevel.None)
                    {
                        if (!pool.ChildrenSlot.TryGetValue(parent.Item, out var children))
                        {
                            pool.ChildrenSlot[parent.Item] = children = new HashSet<(Item child, int sourceIndex)>();
                        }
                        children.Add((child, sourceIndex));
                    }

                    pool.Sublabels[sourceIndex] ??= new List<IItemWithLabel>();
                    pool.Sublabels[sourceIndex]!.Add(parent);
                    pool.LatestChildren[sourceIndex] = child;

                    if (!pool.Slot.TryGetValue(parent.Item, out var sources))
                    {
                        pool.Slot[parent.Item] = sources = new IndexSet();
                        pool.Queue.Add((parent, sourceIndex, checkpointLevel));
                    }
                    sources.Add(sourceIndex);
                }
            }

            internal bool TryMerge(Labeler labeler, Pool pool)
            {
                var isModified = false;
                
                pool.Reset(Parents.Count);

                bool TieKnots(Item start, int sourceIndex, IndexSet sources)
                {
                    var isModifiedLocal = false;
                    var baseParentCount = Parents.Count - pool.MergedSourceIndices.Count;

                    foreach (var ss in pool.DisjointSet.Groups(2))
                    {
                        if (ss.All(s => !pool.Drops[s] && pool.LatestChildren[s]!.CommonAncestors2.IntersectsWith(start)))
                        {
                            var ssCount = 0;
                            var surrogateIndex = -1;
                            foreach (var s in ss)
                            {
                                pool.MergedSourceIndices.Add(s);
                                ssCount++;

                                if (s == sourceIndex || surrogateIndex < 0)
                                {
                                    surrogateIndex = s;
                                }
                            }

                            isModifiedLocal = true;
                            isModified = true;

                            var children = new HashSet<Item>(pool.ChildrenSlot[start].Where(cs => ss.Contains(cs.sourceIndex)).Select(cs => cs.child));

                            var (knot, isLabelInherited) = labeler.LabelKnot(start, ss.SelectMany(s => pool.Sublabels[s]).Select(l => l.Label), this, start.Children.Count == children.Count);
                            var isSeriesSettlable = isLabelInherited && (ssCount < baseParentCount || Children.Count == 0);
                            
                            Parents[surrogateIndex] = knot;

                            var sublabel = pool.Sublabels[surrogateIndex]!;

                            start.Children.RemoveWhereContainsUnstable(children);
                            start.Children.Add(this);

                            foreach (var s in ss)
                            {
                                pool.DisjointSet.UnsafeIndepend(s);
                                pool.Sublabels[s]!.Clear();

                                if (s != surrogateIndex)
                                {
                                    sources.Remove(s);
                                }
                            }
                            pool.MergedSourceIndices.Remove(surrogateIndex);

                            if (isSeriesSettlable)
                            {
                                sublabel.Add(SettleSeries(labeler, surrogateIndex));
                            }
                            else
                            {
                                sublabel.Add(knot);
                            }                            
                        }
                    }
    
                    return isModifiedLocal;
                }

                bool BundleParallel(Item start, int sourceIndex, IndexSet sources)
                {
                    var baseParentCount = Parents.Count - pool.MergedSourceIndices.Count;

                    var parallelCandidates = pool.ParallelSourceIndices;
                    parallelCandidates.Clear();
                    var surrogateIndex = -1;
                    for (var i = 0; i < Parents.Count; i++)
                    {
                        if (pool.MergedSourceIndices.Contains(i) || Parents[i].Item != start)
                        {
                            continue;
                        }

                        parallelCandidates.Add(i);

                        if (i == sourceIndex || surrogateIndex < 0)
                        {
                            surrogateIndex = i;
                        }
                    }

                    if (parallelCandidates.Count > 1)
                    {
                        var (par, isLabelInherited) = labeler.LabelParallel(start, parallelCandidates.Select(s => Parents[s].Label), this, start.Children.Count == parallelCandidates.Count);
                        var isSeriesSettlable = isLabelInherited && (parallelCandidates.Count < baseParentCount || Children.Count == 0);

                        Parents[surrogateIndex] = par;

                        foreach (var s in parallelCandidates)
                        {
                            pool.DisjointSet.UnsafeIndepend(s);
                            pool.Sublabels[s]!.Clear();

                            if (s != surrogateIndex)
                            {
                                sources.Remove(s);
                                pool.MergedSourceIndices.Add(s);
                            }
                        }
                        start.Children.RemoveAllUnstable(this);
                        start.Children.Add(this);

                        var sublabel = pool.Sublabels[surrogateIndex]!;

                        if (isSeriesSettlable)
                        {
                            sublabel.Add(SettleSeries(labeler, surrogateIndex));
                        }
                        else
                        {
                            sublabel.Add(par);
                        }
                        isModified = true;
                        return true;
                    }

                    return false;
                }

                for (var i = 0; i < Parents.Count; i++)
                {
                    EnqueueIfOnPath(this, Parents[i], i, pool);
                }

                while(pool.Queue.Count > 0)
                {
                    var (parent, sourceIndex, checkpointLevel) = pool.Queue.Take();
                    var sources = pool.Slot[parent.Item];

                    if (checkpointLevel > CheckpointLevel.None)
                    {
                        var isMerged = TieKnots(parent.Item, sourceIndex, sources);
                        isMerged |= BundleParallel(parent.Item, sourceIndex, sources);
                        if (isMerged && checkpointLevel == CheckpointLevel.Common)
                        {
                            break;
                        }
                        pool.ChildrenSlot.Remove(parent.Item);
                    }

                    foreach (var s in sources)
                    {
                        pool.DisjointSet.Unite(sourceIndex, s);
                    }

                    pool.Slot.Remove(parent.Item);

                    for (var i = 0; i < parent.Item.Parents.Count; i++)
                    {
                        EnqueueIfOnPath(parent.Item, parent.Item.Parents[i], sourceIndex, pool);
                    }
                }

                Parents.RemoveWhereContainsIndex(pool.MergedSourceIndices);

                return isModified;
            }

            IEnumerator<Item> IEnumerable<Item>.GetEnumerator()
            {
                yield return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield return this;
            }

            internal void ReduceDescendants()
            {
                CommonDescendants = CommonDescendants.Reduce();
                SubcommonDescendants = SubcommonDescendants.Reduce();
            }

            internal void ReduceAncestors()
            {
                CommonAncestors = CommonAncestors.Reduce();
                SubcommonAncestors = SubcommonAncestors.Reduce();
            }
        }

        class Pool
        {
            internal PriorityQueue<(IItemWithLabel parent, int sourceIndex, CheckpointLevel checkpointLevel)> Queue { get; } = new PriorityQueue<(IItemWithLabel parent, int sourceIndex, CheckpointLevel checkpointLevel)>(Comparer<(MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.IItemWithLabel parent, int sourceIndex, CheckpointLevel checkpointLevel)>.Create((x, y) => y.parent.Item.Order.CompareTo(x.parent.Item.Order)));
            internal DisjointSet DisjointSet { get; }

            internal IndexSet MergedSourceIndices { get; }
            internal IndexSet ParallelSourceIndices { get; }
            internal Dictionary<Item, IndexSet> Slot { get; } = new Dictionary<Item, IndexSet>();
            internal Dictionary<Item, HashSet<(Item child, int sourceIndex)>> ChildrenSlot { get; } = new Dictionary<Item, HashSet<(Item child, int sourceIndex)>>();
            internal List<IItemWithLabel>?[] Sublabels { get; }
            internal Item?[] LatestChildren { get; }
            internal bool[] Drops { get; }

            internal Pool(int maxParentCount)
            {
                DisjointSet = new DisjointSet(0, maxParentCount);
                MergedSourceIndices = new IndexSet(maxParentCount);
                ParallelSourceIndices = new IndexSet(maxParentCount);

                Sublabels = new List<IItemWithLabel>?[maxParentCount];
                LatestChildren = new Item?[maxParentCount];
                Drops = new bool[maxParentCount];
            }

            internal void Reset(int currentParentCount)
            {
                Queue.Clear();
                DisjointSet.Reset(currentParentCount);
                MergedSourceIndices.Clear();
                ParallelSourceIndices.Clear();
                Slot.Clear();
                ChildrenSlot.Clear();

                for (var i = 0; i < currentParentCount; i++)
                {
                    Sublabels[i] = null;
                    LatestChildren[i] = null;
                    Drops[i] = false;
                }
            }
        }
    }


    public abstract partial class MonbetsuClassifier<TNode, TEdge, TLabel>
            where TNode : notnull
            where TEdge : notnull
            where TLabel : notnull
    {
        protected abstract IEnumerable<(TEdge edge, TNode toNode)> GetOutflows(TNode fromNode);
    
        public void Classify(IEnumerable<TNode> startNodes, ILabelFactory<TNode, TEdge, TLabel> labelFactory, ILabeler<TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            Classify(startNodes, GetOutflows, labelFactory.CreateFromEdge, labelFactory.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public void Classify(IEnumerable<TNode> startNodes, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            Classify(startNodes, GetOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }

        public static void Classify(IEnumerable<TNode> startNodes, IGraphStructure<TNode, TEdge> graphStructure, ILabelFactory<TNode, TEdge, TLabel> labelFactoy, ILabeler<TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            Classify(startNodes, graphStructure.GetOutflows, labelFactoy.CreateFromEdge, labelFactoy.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public static void Classify(IEnumerable<TNode> startNodes, FlowGetter<TNode, TEdge> getOutflows, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            EdgeLabeler<Unit, TNode, TEdge, TLabel> edge = labelEdge switch
            {
                null => (_, f, s, t, l) => { },
                _ => (_, f, s, t, l) => labelEdge(f, s, t, l)
            };


            SeriesSubgraphLabeler<Unit, TNode, TLabel> series = labelSeries switch
            {
                null => (_, f, s, t, l) => { },
                _ => (_, f, s, t, l) => labelSeries(f, s, t, l)
            };

            ParallelSubgraphLabeler<Unit, TNode, TLabel> parallel = labelParallel switch
            {
                null => (_, f, s, t, l) => { },
                _ => (_, f, s, t, l) => labelParallel(f, s, t, l)
            };

            KnotSubgraphLabeler<Unit, TNode, TLabel>? knot = labelKnot switch
            {
                null => (_, f, s, t, l) => { },
                _ => (_, f, s, t, l) => labelKnot(f, s, t, l)
            };

            MonbetsuClassifier<Unit, TNode, TEdge, TLabel>.Classify(default, startNodes,
                (_, n) => getOutflows(n),
                (_, f, e, t) => createLabelFromEdge(f, e, t),
                (_, s, e) => createLabelFromSubgraph(s, e),
                edge,
                series,
                parallel,
                knot,
                cancellationToken
                );
        }
    }

    

    public class MonbetuException : Exception
    {
        protected internal MonbetuException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
    }

    public class CyclicException : MonbetuException
    {
        protected internal CyclicException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
    }

    public class CyclicException<TNode> : CyclicException
        where TNode : notnull
    {
        public IReadOnlyCollection<TNode> UnresolvableNodes { get; }

        internal CyclicException(TNode unresolvableNode)
            : base($"A cycle is detected in the graph. One of nodes on the cyclic path is [{unresolvableNode}].")
        {
            UnresolvableNodes = new List<TNode> { unresolvableNode };
        }

        internal CyclicException(IReadOnlyCollection<TNode> unresolvableNodes)
            : base($"Some cycles are detected in the graph. Some of nodes on the cyclic path are [{string.Join(", ", unresolvableNodes.Take(3))}, ..].")
        {
            UnresolvableNodes = unresolvableNodes;
        }
    }

    namespace Internals
    {
        public struct Unit { }

        class DisjointSet
        {
            private readonly (int group, int count)[] array;
            private int length;
            public DisjointSet(int length, int capacity)
            {
                this.length = length;
                array = new (int group, int count)[capacity];
                for (var i = 0; i < capacity; i++)
                {
                    array[i] = (i, 1);
                }
            }

            public void Reset(int length)
            {
                for (var i = 0; i < this.length; i++)
                {
                    array[i] = (i, 1);
                }
                this.length = length;
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

            internal void UnsafeIndepend(int n)
            {
                array[n] = (n, 1);
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
                return array.Take(length).Select((a, i) => (a, i)).ToLookup(o => Root(o.a.group), o => o.i);
            }

            public IEnumerable<IEnumerable<int>> Groups(int membersAtLeast)
            {
                var dic = new Dictionary<int, (int[] buffer, int count)>();
                for (var i = 0; i < length; i++)
                {
                    var root = Root(i);
                    if (root != i)
                    {
                        array[i] = (root, 1);
                    }

                    if (array[root].count >= membersAtLeast)
                    {
                        if (!dic.TryGetValue(root, out var t))
                        {
                            t = (new int[array[root].count], 0);
                        }
                        t.buffer[t.count] = i;
                        dic[root] = (t.buffer, t.count + 1);
                    }
                }
                return dic.Values.Select(t => t.buffer);
            }
        }

        class PriorityQueue<T>
        {
            public int Count => list.Count;

            private readonly List<T> list = new List<T>();

            public IComparer<T> Comparer { get; }

            public PriorityQueue(IComparer<T>? comparer = null)
            {
                Comparer = comparer ?? Comparer<T>.Default;
            }

            public void Clear()
            {
                list.Clear();
            }

            private static void Add(List<T> list, T item, IComparer<T> comparer)
            {
                var c = list.Count;
                list.Add(item);

                while (c != 0)
                {
                    var p = (c - 1) / 2;

                    if (comparer.Compare(list[c], list[p]) < 0)
                    {
                        var tmp = list[c];
                        list[c] = list[p];
                        list[p] = tmp;
                    }
                    c = p;
                }
            }

            private static T Take(List<T> list, IComparer<T> comparer)
            {
                var last = list.Count - 1;
                var top = list[0];
                list[0] = list[last];
                list.RemoveAt(last);

                for (int p = 0, l = 1; l < last; l = 2 * p + 1)
                {
                    var r = l + 1;
                    var c = l;

                    if (r < last && comparer.Compare(list[l], list[r]) > 0)
                    {
                        c = r;
                    }

                    if (comparer.Compare(list[p], list[c]) > 0)
                    {
                        var tmp = list[c];
                        list[c] = list[p];
                        list[p] = tmp;
                    }

                    p = c;
                }

                return top;
            }

            public void Add(T item)
            {
                Add(list, item, Comparer);
            }

            public T Take()
            {
                return Take(list, Comparer);
            }
        }

        class IndexSet : IReadOnlyCollection<int>
        {
            private const uint Zero = 0U;
            private const uint One = 1U;
            private const int Shift = 5;
            private const int Mask = (1 << Shift) - 1;

            private readonly List<uint> bits;
            public int Count { get; private set; }

            public IndexSet()
            {
                bits = new List<uint>(1) { Zero };
            }

            public IndexSet(int capacity)
            {
                bits = new List<uint>((capacity + Mask) >> Shift) { Zero };
            }

            public void Clear()
            {
                bits.Clear();
                bits.Add(Zero);
                Count = 0;
            }

            private static (int d, int r) DivRem(int index) => (index >> Shift, index & Mask);

            public bool Add(int index)
            {
                var (d, r) = DivRem(index);
                if (d < bits.Count)
                {
                    var old = (bits[d] >> r) & 1;
                    if (old == 0)
                    {
                        bits[d] ^= One << r;
                        Count++;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    do
                    {
                        bits.Add(Zero);
                    } while (d >= bits.Count);
                    bits[d] |= One << r;
                    Count++;
                }
                return true;
            }

            public bool Remove(int index)
            {
                var (d, r) = DivRem(index);
                if (d < bits.Count)
                {
                    var old = (bits[d] >> r) & 1;
                    if (old > 0)
                    {
                        bits[d] ^= One << r;
                        Count--;
                        return true;
                    }
                }
                return false;
            }

            public bool Contains(int index)
            {
                var (d, r) = DivRem(index);
                if (d < bits.Count)
                {
                    return ((bits[d] >> r) & 1) > 0;
                }
                return false;
            }

            public IEnumerator<int> GetEnumerator()
            {
                for (var d = 0; d < bits.Count; d++)
                {
                    var bit = bits[d];
                    var h = d << Shift;
                    for (var r = 0; bit > 0; r++, bit >>= 1)
                    {
                        if ((bit & 1) > 0)
                        {
                            yield return h + r;
                        }
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        static class CollectionsExtensions
        {
            internal static bool IntersectsWith<T>(this HashSet<T> lhs, HashSet<T> rhs)
            {
                var (smaller, larger) = lhs.Count > rhs.Count ? (rhs, lhs) : (lhs, rhs);
                return smaller.Any(larger.Contains);
            }

            internal static void RemoveWhereContainsUnstable<T>(this List<T> unorderedList, HashSet<T> targets)
            {
                for (var c = 0; c < unorderedList.Count;)
                {
                    if (targets.Contains(unorderedList[c]))
                    {
                        unorderedList[c] = unorderedList[unorderedList.Count - 1];
                        unorderedList.RemoveAt(unorderedList.Count - 1);
                    }
                    else
                    {
                        c++;
                    }
                }
            }

            internal static void RemoveAllUnstable<T>(this List<T> unorderedList, T target) where T : class
            {
                for (var c = 0; c < unorderedList.Count;)
                {
                    if (unorderedList[c] == target)
                    {
                        unorderedList[c] = unorderedList[unorderedList.Count - 1];
                        unorderedList.RemoveAt(unorderedList.Count - 1);
                    }
                    else
                    {
                        c++;
                    }
                }
            }

            internal static void RemoveWhereContainsIndex<T>(this List<T> list, IEnumerable<int> sortedTargetIndices)
            {
                using var it = sortedTargetIndices.GetEnumerator();

                if (it.MoveNext())
                {
                    var start = it.Current;
                    var dest = start;

                    while (it.MoveNext())
                    {
                        var end = it.Current;
                        for (var i = start + 1; i < end; i++)
                        {
                            list[dest++] = list[i];
                        }
                        start = end;
                    }
                    for (var i = start + 1; i < list.Count; i++)
                    {
                        list[dest++] = list[i];
                    }
                    list.RemoveRange(dest, list.Count - dest);
                }
            }
        }
    }    
}
