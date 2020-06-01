// Copyright (c) takeisit. All Rights Reserved.
// https://github.com/takeisit/Monbetsu/
// SPDX-License-Identifier: MIT

// ver 0.5.1

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Monbetsu
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

        public static void Classify<TGraph, TNode, TEdge, TLabel>(TGraph graph, IEnumerable<TNode> startNodes, FlowEnumerator<TGraph, TNode, TEdge> getOutflows, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
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

        public static void Classify<TNode, TEdge, TLabel>(IEnumerable<TNode> startNodes, FlowEnumerator<TNode, TEdge> getOutflows, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
            where TNode : notnull
            where TEdge : notnull
            where TLabel : notnull
        {
            MonbetsuClassifier<TNode, TEdge, TLabel>.Classify(startNodes, getOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }

        public static partial class Integratedly
        {
            public static void Classify<TGraph, TNode, TEdge, TLabel>(TGraph graph, IEnumerable<TNode> startNodes, IGraphStructure<TGraph, TNode, TEdge> graphStructure, ILabelFactory<TGraph, TNode, TEdge, TLabel> labelFactoy, ILabeler<TGraph, TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
                where TGraph : notnull
                where TNode : notnull
                where TEdge : notnull
                where TLabel : notnull
            {
                MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.ClassifyIntegratedly(graph, startNodes, graphStructure, labelFactoy, labeler, cancellationToken);
            }

            public static void Classify<TGraph, TNode, TEdge, TLabel>(TGraph graph, IEnumerable<TNode> startNodes, FlowEnumerator<TGraph, TNode, TEdge> getOutflows, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
                where TGraph : notnull
                where TNode : notnull
                where TEdge : notnull
                where TLabel : notnull
            {
                MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.ClassifyIntegratedly(graph, startNodes, getOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
            }

            public static void Classify<TNode, TEdge, TLabel>(IEnumerable<TNode> startNodes, IGraphStructure<TNode, TEdge> graphStructure, ILabelFactory<TNode, TEdge, TLabel> labelFactoy, ILabeler<TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
                where TNode : notnull
                where TEdge : notnull
                where TLabel : notnull
            {
                MonbetsuClassifier<TNode, TEdge, TLabel>.ClassifyIntegratedly(startNodes, graphStructure, labelFactoy, labeler, cancellationToken);
            }

            public static void Classify<TNode, TEdge, TLabel>(IEnumerable<TNode> startNodes, FlowEnumerator<TNode, TEdge> getOutflows, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
                where TNode : notnull
                where TEdge : notnull
                where TLabel : notnull
            {
                MonbetsuClassifier<TNode, TEdge, TLabel>.ClassifyIntegratedly(startNodes, getOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
            }
        }

        public static partial class Uniquely
        {
            public static void Classify<TGraph, TNode, TEdge, TLabel>(TGraph graph, IEnumerable<TNode> startNodes, IGraphStructure<TGraph, TNode, TEdge> graphStructure, ILabelFactory<TGraph, TNode, TEdge, TLabel> labelFactoy, IUniquelyLabeler<TGraph, TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
                where TGraph : notnull
                where TNode : notnull
                where TEdge : notnull
                where TLabel : notnull
            {
                MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.ClassifyUniquely(graph, startNodes, graphStructure, labelFactoy, labeler, cancellationToken);
            }

            public static void Classify<TGraph, TNode, TEdge, TLabel>(TGraph graph, IEnumerable<TNode> startNodes, FlowEnumerator<TGraph, TNode, TEdge> getOutflows, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, UniquelySeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
                where TGraph : notnull
                where TNode : notnull
                where TEdge : notnull
                where TLabel : notnull
            {
                MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.ClassifyUniquely(graph, startNodes, getOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
            }

            public static void Classify<TNode, TEdge, TLabel>(IEnumerable<TNode> startNodes, IGraphStructure<TNode, TEdge> graphStructure, ILabelFactory<TNode, TEdge, TLabel> labelFactoy, IUniquelyLabeler<TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
                where TNode : notnull
                where TEdge : notnull
                where TLabel : notnull
            {
                MonbetsuClassifier<TNode, TEdge, TLabel>.ClassifyUniquely(startNodes, graphStructure, labelFactoy, labeler, cancellationToken);
            }

            public static void Classify<TNode, TEdge, TLabel>(IEnumerable<TNode> startNodes, FlowEnumerator<TNode, TEdge> getOutflows, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, UniquelySeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
                where TNode : notnull
                where TEdge : notnull
                where TLabel : notnull
            {
                MonbetsuClassifier<TNode, TEdge, TLabel>.ClassifyUniquely(startNodes, getOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
            }
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

    public interface IUniquelyLabeler<TGraph, TNode, TEdge, TLabel>
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull
    {
        void LabelEdge(TGraph graph, TNode fromNode, TEdge edge, TNode toNode, TLabel label);
        void LabelSeriesSubgraph(TGraph graph, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);
        void LabelParallelSubgraph(TGraph graph, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);
        void LabelKnotSubgraph(TGraph graph, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);
    }

    public interface IUniquelyLabeler<TNode, TEdge, TLabel>
        where TNode : notnull
        where TEdge : notnull
        where TLabel : notnull
    {
        void LabelEdge(TNode fromNode, TEdge edge, TNode toNode, TLabel label);
        void LabelSeriesSubgraph(TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label);
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

    public delegate void UniquelySeriesSubgraphLabeler<TGraph, TNode, TLabel>(TGraph graph, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label)
        where TGraph : notnull
        where TNode : notnull
        where TLabel : notnull;

    public delegate void UniquelySeriesSubgraphLabeler<TNode, TLabel>(TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label)
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

    //public delegate void SubgraphLabeler<TGraph, TNode, TLabel>(TGraph graph, SubgraphKind subgraphKind, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label)
    //    where TGraph : notnull
    //    where TNode : notnull
    //    where TLabel : notnull;

    //public delegate void SubgraphLabeler<TNode, TLabel>(SubgraphKind subgraphKind, TNode startNode, IEnumerable<TLabel> sublebels, TNode endNode, TLabel label)
    //    where TNode : notnull
    //    where TLabel : notnull;


    public delegate IEnumerable<(TEdge, TNode)> FlowEnumerator<TGraph, TNode, TEdge>(TGraph graph, TNode node)
        where TGraph : notnull
        where TNode : notnull
        where TEdge : notnull;

    public delegate IEnumerable<(TEdge, TNode)> FlowEnumerator<TNode, TEdge>(TNode node)
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

    //public enum SubgraphKind
    //{
    //    Edge,
    //    Series,
    //    Parallel,
    //    Knot
    //}

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

        public static void Classify(TGraph graph, IEnumerable<TNode> startNodes, FlowEnumerator<TGraph, TNode, TEdge> getOutflows, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            var labeler = new DefaultLabeler(graph, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot);

            Classify(graph, startNodes, getOutflows, labeler, cancellationToken);
        }

        public void ClassifyIntegratedly(TGraph graph, IEnumerable<TNode> startNodes, ILabelFactory<TGraph, TNode, TEdge, TLabel> labelFactory, ILabeler<TGraph, TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            ClassifyIntegratedly(graph, startNodes, GetOutflows, labelFactory.CreateFromEdge, labelFactory.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public void ClassifyIntegratedly(TGraph graph, IEnumerable<TNode> startNodes, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            ClassifyIntegratedly(graph, startNodes, GetOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }

        public static void ClassifyIntegratedly(TGraph graph, IEnumerable<TNode> startNodes, IGraphStructure<TGraph, TNode, TEdge> graphStructure, ILabelFactory<TGraph, TNode, TEdge, TLabel> labelFactoy, ILabeler<TGraph, TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            ClassifyIntegratedly(graph, startNodes, graphStructure.GetOutFlows, labelFactoy.CreateFromEdge, labelFactoy.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public static void ClassifyIntegratedly(TGraph graph, IEnumerable<TNode> startNodes, FlowEnumerator<TGraph, TNode, TEdge> getOutflows, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            var labeler = new IntegratedlyLabeler(graph, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot);

            Classify(graph, startNodes, getOutflows, labeler, cancellationToken);
        }

        public void ClassifyUniquely(TGraph graph, IEnumerable<TNode> startNodes, ILabelFactory<TGraph, TNode, TEdge, TLabel> labelFactory, IUniquelyLabeler<TGraph, TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            ClassifyUniquely(graph, startNodes, GetOutflows, labelFactory.CreateFromEdge, labelFactory.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public void ClassifyUniquely(TGraph graph, IEnumerable<TNode> startNodes, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, UniquelySeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            ClassifyUniquely(graph, startNodes, GetOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }

        public static void ClassifyUniquely(TGraph graph, IEnumerable<TNode> startNodes, IGraphStructure<TGraph, TNode, TEdge> graphStructure, ILabelFactory<TGraph, TNode, TEdge, TLabel> labelFactoy, IUniquelyLabeler<TGraph, TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            ClassifyUniquely(graph, startNodes, graphStructure.GetOutFlows, labelFactoy.CreateFromEdge, labelFactoy.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public static void ClassifyUniquely(TGraph graph, IEnumerable<TNode> startNodes, FlowEnumerator<TGraph, TNode, TEdge> getOutflows, EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge, UniquelySeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            var labeler = new UniquelyLabeler(graph, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot);

            Classify(graph, startNodes, getOutflows, labeler, cancellationToken);
        }

        private static void Classify(TGraph graph, IEnumerable<TNode> startNodes, FlowEnumerator<TGraph, TNode, TEdge> getOutflows, ILabeler labeler, CancellationToken cancellationToken = default)
        {
            try
            {
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

#if MONBETSU_DEBUG_DUMP
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var item = list[i];

                    item.TwistAncestors();
                }

                for (var i = list.Count - 1; i >= 0; i--)
                {
                    Debug.WriteLine(list[i].Dump());
                }
#endif
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var item = list[i];
#if !MONBETSU_DEBUG_DUMP
                    item.TwistAncestors();
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
            catch(MonbetsuException)
            {
                throw;
            }
            catch(OperationCanceledException)
            {
                throw;
            }
            catch(Exception exception)
            {
                throw new MonbetsuException("classification failed", exception);
            }
        }

        private static (IReadOnlyList<Item> list, int maxParentCount) BuildTopology(in TGraph graph, FlowEnumerator<TGraph, TNode, TEdge> getOutflows, IEnumerable<TNode> startNodes, CancellationToken cancellationToken = default)
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
                            throw new CyclicException<TNode, TEdge>(parent.Node, edge, toNode);
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

        interface ILabeler
        {
            IItemWithLabel LabelEdge(IItemWithLabel parent, Item child);

            (Item newParent, Item childOfNewParent, TLabel label) LabelSeries(IItemWithLabel itemWithLabel, Item end);

            (Item newParent, TLabel label, bool isLabelInherited) LabelParallel(Item parent, IEnumerable<TLabel> sublabels, Item child, bool isLabelInheritable);

            (Item newParent, TLabel label, bool isLabelInherited) LabelKnot(Item parent, IEnumerable<TLabel> sublabels, Item child, bool isLabelInheritable);
        }

        abstract class LabelerBase : ILabeler
        {
            protected TGraph Graph { get; }
            protected EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> CreateLabelFromEdge { get; }
            protected SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> CreateLabelFromSubgraph { get; }
            protected EdgeLabeler<TGraph, TNode, TEdge, TLabel> LabelEdgeCallback { get; }
            protected ParallelSubgraphLabeler<TGraph, TNode, TLabel> LabelParallelCallback { get; }
            protected KnotSubgraphLabeler<TGraph, TNode, TLabel> LabelKnotCallback { get; }

            protected LabelerBase(
                TGraph graph,
                EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge,
                SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph,
                EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge,
                ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel,
                KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot)
            {
                Graph = graph;
                CreateLabelFromEdge = createLabelFromEdge ?? throw new ArgumentNullException(nameof(createLabelFromEdge));
                CreateLabelFromSubgraph = createLabelFromSubgraph ?? throw new ArgumentNullException(nameof(createLabelFromSubgraph));
                LabelEdgeCallback = labelEdge ?? ((_0, _1, _2, _3, _4) => { });
                LabelParallelCallback = labelParallel ?? ((_0, _1, _2, _3, _4) => { });
                LabelKnotCallback = labelKnot ?? ((_0, _1, _2, _3, _4) => { });
            }

            public IItemWithLabel LabelEdge(IItemWithLabel parent, Item child)
            {
                if (parent is ItemWithEdge rawEdgeContainer)
                {
                    var edge = rawEdgeContainer.Edge;

                    var (label, _) = CreateEdgeLabelCore(parent.Item, edge, child, parent.Item.Children.Count == 1);
                    var newParent = new ItemWithLabel(parent.Item, label);
                    LabelEdgeCallback(Graph, parent.Item.Node, edge, child.Node, label);
                    return newParent;
                }
                else
                {
                    return parent;
                }
            }

            protected virtual (TLabel label, bool isLabelInherited) CreateEdgeLabelCore(Item parent, TEdge edge, Item child, bool isLabelInheritable)
            {                
                if (isLabelInheritable && parent.Parents.Count == 1)
                {
                    return (parent.Parents[0].Label, true);
                }
                else
                {
                    return (CreateLabelFromEdge(Graph, parent.Node, edge, child.Node), false);
                }
            }


            protected virtual (TLabel label, bool isLabelInherited) CreateSubgraphLabelCore(Item parent, Item child, bool isLabelInheritable)
            {
                if (isLabelInheritable && parent.Parents.Count == 1)
                {
                    return (parent.Parents[0].Label, true);
                }
                else
                {
                    return (CreateLabelFromSubgraph(Graph, parent.Node, child.Node), false);
                }
            }

            public abstract (Item newParent, Item childOfNewParent, TLabel label) LabelSeries(IItemWithLabel itemWithLabel, Item end);

            
            public (Item newParent, TLabel label, bool isLabelInherited) LabelParallel(Item parent, IEnumerable<TLabel> sublabels, Item child, bool isLabelInheritable)
            {
                var (label, isLabelInherited) = CreateSubgraphLabelCore(parent, child, isLabelInheritable);
                LabelParallelCallback(Graph, parent.Node, sublabels, child.Node, label);
                return (parent, label, isLabelInherited);
            }

            public (Item newParent, TLabel label, bool isLabelInherited) LabelKnot(Item parent, IEnumerable<TLabel> sublabels, Item child, bool isLabelInheritable)
            {
                var (label, isLabelInherited) = CreateSubgraphLabelCore(parent, child, isLabelInheritable);
                LabelKnotCallback(Graph, parent.Node, sublabels, child.Node, label);
                return (parent, label, isLabelInherited);
            }
        }

        class DefaultLabeler : LabelerBase
        {
            private readonly SeriesSubgraphLabeler<TGraph, TNode, TLabel> labelSeries;
            
            public DefaultLabeler(
                TGraph graph,
                EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge,
                SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph,
                EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge,
                SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries, 
                ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel, 
                KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot)
                : base(graph, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelParallel, labelKnot)
            {
                this.labelSeries = labelSeries ?? ((_0, _1, _2, _3, _4) => { });
            }

            public override (Item newParent, Item childOfNewParent, TLabel label) LabelSeries(IItemWithLabel itemWithLabel, Item end)
            {
                var start = itemWithLabel.Item;
                Item pp;
                do
                {
                    pp = start;
                    start = start.Parents[0].Item;
                } while (start.IsSerial);

                var label = CreateLabelFromSubgraph(Graph, start.Node, end.Node);
                labelSeries(Graph, start.Node, itemWithLabel.Label, end.Node, label);

                return (start, pp, label);
            }
        }

        class IntegratedlyLabeler : LabelerBase
        {
            private readonly SeriesSubgraphLabeler<TGraph, TNode, TLabel> labelSeries;

            public IntegratedlyLabeler(
                TGraph graph,
                EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge,
                SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph,
                EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge,
                SeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries,
                ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel,
                KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot)
                : base(graph, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelParallel, labelKnot)
            {
                this.labelSeries = labelSeries ?? ((_0, _1, _2, _3, _4) => { });
            }

            public override (Item newParent, Item childOfNewParent, TLabel label) LabelSeries(IItemWithLabel itemWithLabel, Item end)
            {
                var start = itemWithLabel.Item;
                Item pp;
                do
                {
                    pp = start;
                    start = start.Parents[0].Item;
                } while (start.IsSerial);

                labelSeries(Graph, start.Node, itemWithLabel.Label, end.Node, itemWithLabel.Label);
                return (start, pp, itemWithLabel.Label);
            }
        }

        class UniquelyLabeler : LabelerBase
        {
            private readonly UniquelySeriesSubgraphLabeler<TGraph, TNode, TLabel> labelSeries;

            public UniquelyLabeler(
                TGraph graph,
                EdgeLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromEdge,
                SubgraphLabelCreator<TGraph, TNode, TEdge, TLabel> createLabelFromSubgraph,
                EdgeLabeler<TGraph, TNode, TEdge, TLabel>? labelEdge,
                UniquelySeriesSubgraphLabeler<TGraph, TNode, TLabel>? labelSeries,
                ParallelSubgraphLabeler<TGraph, TNode, TLabel>? labelParallel,
                KnotSubgraphLabeler<TGraph, TNode, TLabel>? labelKnot)
                : base(graph, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelParallel, labelKnot)
            {
                this.labelSeries = labelSeries ?? ((_0, _1, _2, _3, _4) => { });
            }

            protected override (TLabel label, bool isLabelInherited) CreateEdgeLabelCore(Item parent, TEdge edge, Item child, bool isLabelInheritable)
            {
                return (CreateLabelFromEdge(Graph, parent.Node, edge, child.Node), false);
            }

            protected override (TLabel label, bool isLabelInherited) CreateSubgraphLabelCore(Item parent, Item child, bool isLabelInheritable)
            {
                return (CreateLabelFromSubgraph(Graph, parent.Node, child.Node), false);
            }

            public override (Item newParent, Item childOfNewParent, TLabel label) LabelSeries(IItemWithLabel itemWithLabel, Item end)
            {
                var start = itemWithLabel;
                var sublabels = new List<TLabel>();
                Item pp;
                do
                {
                    sublabels.Add(start.Label);
                    pp = start.Item;
                    start = start.Item.Parents[0];
                } while (start.Item.IsSerial);

                sublabels.Add(start.Label);

                var label = CreateLabelFromSubgraph(Graph, start.Item.Node, end.Node);
                labelSeries(Graph, start.Item.Node, sublabels, end.Node, label);

                return (start.Item, pp, label);
            }
        }

        interface IItemWithLabel
        {
            Item Item { get; }
            TLabel Label { get; }

            IItemWithLabel Update(Item item, TLabel label);
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

            public IItemWithLabel Update(Item item, TLabel label) => throw new InvalidOperationException();
        }

        [DebuggerDisplay("Subgraph To {Item.Node}: {Label}")]
        class ItemWithLabel : IItemWithLabel
        {
            public Item Item { get; private set; }
            public TLabel Label { get; private set; }

            public ItemWithLabel(Item item, TLabel label)
            {
                Item = item;
                Label = label;
            }

            public IItemWithLabel Update(Item item, TLabel label)
            {
                Item = item;
                Label = label;
                return this;
            }
        }

        enum CheckpointLevel
        {
            None,
            Subcommon,
            Common
        }

        [DebuggerDisplay("{Node} parent#={Parents.Count} ancestor#={CommonAncestors.Count}+{SubcommonAncestors.Count} child#={Children.Count} descendant#={CommonDescendants.Count}+{SubcommonDescendants.Count}, Order={Order}")]
        class Item
        {
            internal static readonly HashSet<Item> EmptyItemSet = new HashSet<Item>();
            private static readonly HashSet<Item> Previsited = new HashSet<Item>();
            private static readonly HashSet<Item> Visited = new HashSet<Item>();

            public TNode Node { get; }

            public HashSet<Item> CommonDescendants { get; private set; } = EmptyItemSet;
            public HashSet<Item> SubcommonDescendants { get; private set; } = EmptyItemSet;
            
            public List<IItemWithLabel> Parents { get; } = new List<IItemWithLabel>();
            public List<Item> Children { get; } = new List<Item>();

            public HashSet<Item> CommonAncestors { get; private set; } = EmptyItemSet;
            public HashSet<Item> SubcommonAncestors { get; private set; } = EmptyItemSet;

            public int Order { get; set; }

            public bool IsSerial => Parents.Count == 1 && Children.Count == 1;
            
            public bool IsStatusVisited => SubcommonAncestors == Visited;
            
            public bool IsStatusPrevisited => SubcommonAncestors == Previsited;


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

#if MONBETSU_DEBUG_DUMP
            internal string Dump()
            {
                static string J(IEnumerable<MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>.Item> source)
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
#endif

            internal void TwistAncestors()
            {
                using var it = Parents.Select(c => c.Item).GetEnumerator();
                if (it.MoveNext())
                {
                    var first = it.Current;
                    if (it.MoveNext())
                    {
                        var counter = first.CommonAncestors.ToDictionary(item => item, _ => 1);
                        if (!counter.ContainsKey(first))
                        {
                            counter[first] = 1;
                        }

                        do
                        {
                            foreach (var item in it.Current.CommonAncestors.Append(it.Current))
                            {
                                counter.TryGetValue(item, out var existing);
                                counter[item] = existing + 1;
                            }
                        }
                        while (it.MoveNext());

                        CommonAncestors = new HashSet<Item>();
                        SubcommonAncestors = new HashSet<Item>();
            
                        foreach (var kv in counter)
                        {
                            if (kv.Value == Parents.Count)
                            {
                                CommonAncestors.Add(kv.Key);
                            }
                            else
                            {
                                if (kv.Value > 1)
                                {
                                    SubcommonAncestors.Add(kv.Key);
                                }
                            }
                        }

                        if (CommonAncestors.Count == 0)
                        {
                            CommonAncestors = EmptyItemSet;
                        }
                        if (SubcommonAncestors.Count == 0)
                        {
                            SubcommonAncestors = EmptyItemSet;
                        }
                    }
                    else if (first.Children.Count > 1)
                    {
                        CommonAncestors = new HashSet<Item>(first.CommonAncestors) { first };
                    }
                    else
                    {
                        CommonAncestors = first.CommonAncestors;
                    }
                }
            }

            internal void TwistDescendants()
            {
                using var it = Children.GetEnumerator();
                if (it.MoveNext())
                {
                    var first = it.Current;
                    if (it.MoveNext())
                    {
                        var counter = first.CommonDescendants.ToDictionary(item => item, _ => 1);
                        if (!counter.ContainsKey(first))
                        {
                            counter[first] = 1;
                        }

                        do
                        {
                            foreach (var item in it.Current.CommonDescendants.Append(it.Current))
                            {
                                counter.TryGetValue(item, out var existing);
                                counter[item] = existing + 1;
                            }
                        }
                        while (it.MoveNext());

                        CommonDescendants = new HashSet<Item>();
                        SubcommonDescendants = new HashSet<Item>();
                        
                        foreach (var kv in counter)
                        {
                            if (kv.Value == Children.Count)
                            {
                                CommonDescendants.Add(kv.Key);
                            }
                            else
                            {
                                if (kv.Value > 1)
                                {
                                    SubcommonDescendants.Add(kv.Key);
                                }
                            }
                        }

                        if (CommonDescendants.Count == 0)
                        {
                            CommonDescendants = EmptyItemSet;
                        }
                        if (SubcommonDescendants.Count == 0)
                        {
                            SubcommonDescendants = EmptyItemSet;
                        }
                    }
                    else if (first.Parents.Count > 1)
                    {
                        CommonDescendants = new HashSet<Item>(first.CommonDescendants) { first };
                    }
                    else
                    {
                        CommonDescendants = first.CommonDescendants;
                    }
                }
            }

            private CheckpointLevel GetAncestorCheckpointLevelFor(Item other)
            {
                if (CommonAncestors.Contains(other))
                {
                    return CheckpointLevel.Common;
                }
                else if (SubcommonAncestors.Contains(other))
                {
                    return CheckpointLevel.Subcommon;
                }
                return CheckpointLevel.None;
            }

            internal void LabelEdges(ILabeler labeler)
            {
                for (var p = 0; p < Parents.Count; p++)
                {
                    Parents[p] = labeler.LabelEdge(Parents[p], this);
                }
            }

            internal bool SettleSeries(ILabeler labeler)
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

            private IItemWithLabel SettleSeries(ILabeler labeler, int parentIndex)
            {
                //var p = Parents[parentIndex].Item;
                //Item pp;
                //do
                //{
                //    pp = p;
                //    p = p.Parents[0].Item;
                //} while (p.IsSerial);

                //var (newParent, label) = labeler.LabelSeries(p, Parents[parentIndex].Label, this);
                //Parents[parentIndex].Update(newParent, label);
                //p.Children[p.Children.IndexOf(pp)] = this;

                var (newParent, childOfNewParent, label) = labeler.LabelSeries(Parents[parentIndex], this);
                Parents[parentIndex].Update(newParent, label);
                newParent.Children[newParent.Children.IndexOf(childOfNewParent)] = this;

                return Parents[parentIndex];
            }

            private (bool onPath, CheckpointLevel checkpointLevel) IsOnMergablePath(Item item, int sourceIndex, Pool pool)
            {
                if (pool.Drops[sourceIndex])
                {
                    return (false, CheckpointLevel.None);
                }

                var checkpointLevel = CheckpointLevel.None;
                if (!item.CommonDescendants.Contains(this))
                {
                    if (item.SubcommonDescendants.Contains(this))
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

                if (checkpointLevel == CheckpointLevel.None && !CommonAncestors.HasIntarsection(item.CommonAncestors) && !SubcommonAncestors.HasIntarsection(item.CommonAncestors))
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
                        pool.Queue.Add((parent.Item, sourceIndex, checkpointLevel));
                    }
                    sources.Add(sourceIndex);
                }
            }

            internal bool TryMerge(ILabeler labeler, Pool pool)
            {
                var isModified = false;
                
                pool.Reset(Parents.Count);

                bool TieKnots(Item start, int sourceIndex, IndexSet sources)
                {
                    var isModifiedLocal = false;
                    var baseParentCount = Parents.Count - pool.MergedSourceIndices.Count;

                    foreach (var ss in pool.DisjointSet.Groups(2))
                    {
                        if (ss.All(s => !pool.Drops[s] && pool.LatestChildren[s]!.CommonAncestors.Contains(start)))
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

                            var (parent, label, isLabelInherited) = labeler.LabelKnot(start, ss.SelectMany(s => pool.Sublabels[s]).Select(l => l.Label), this, start.Children.Count == children.Count);
                            var knot = Parents[surrogateIndex].Update(parent, label);
                            var isSeriesSettlable = isLabelInherited && (ssCount < baseParentCount || Children.Count == 0);
                            
                            var sublabel = pool.Sublabels[surrogateIndex]!;

                            start.Children.RemoveWhereContainsUnorderedly(children);
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
                        var (newParent, label, isLabelInherited) = labeler.LabelParallel(start, parallelCandidates.Select(s => Parents[s].Label), this, start.Children.Count == parallelCandidates.Count);
                        var par = Parents[surrogateIndex].Update(newParent, label);
                        var isSeriesSettlable = isLabelInherited && (parallelCandidates.Count < baseParentCount || Children.Count == 0);

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
                        start.Children.RemoveAllUnorderedly(this);
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
                    var sources = pool.Slot[parent];

                    if (checkpointLevel > CheckpointLevel.None)
                    {
                        TieKnots(parent, sourceIndex, sources);
                        BundleParallel(parent, sourceIndex, sources);
                        if (checkpointLevel == CheckpointLevel.Common)
                        {
                            break;
                        }
                        pool.ChildrenSlot.Remove(parent);
                    }

                    foreach (var s in sources)
                    {
                        pool.DisjointSet.Unite(sourceIndex, s);
                    }

                    pool.Slot.Remove(parent);

                    for (var i = 0; i < parent.Parents.Count; i++)
                    {
                        EnqueueIfOnPath(parent, parent.Parents[i], sourceIndex, pool);
                    }
                }

                Parents.RemoveWhereContainsIndexUnorderedly(pool.MergedSourceIndices);

                return isModified;
            }
        }

        class Pool
        {
            internal PriorityQueue<(Item parent, int sourceIndex, CheckpointLevel checkpointLevel)> Queue { get; } = new PriorityQueue<(Item parent, int sourceIndex, CheckpointLevel checkpointLevel)>(Comparer<(Item parent, int sourceIndex, CheckpointLevel checkpointLevel)>.Create((x, y) => y.parent.Order.CompareTo(x.parent.Order)));
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

        public static void Classify(IEnumerable<TNode> startNodes, FlowEnumerator<TNode, TEdge> getOutflows, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
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

        public void ClassifyIntegratedly(IEnumerable<TNode> startNodes, ILabelFactory<TNode, TEdge, TLabel> labelFactory, ILabeler<TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            ClassifyIntegratedly(startNodes, GetOutflows, labelFactory.CreateFromEdge, labelFactory.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public void ClassifyIntegratedly(IEnumerable<TNode> startNodes, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            ClassifyIntegratedly(startNodes, GetOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }

        public static void ClassifyIntegratedly(IEnumerable<TNode> startNodes, IGraphStructure<TNode, TEdge> graphStructure, ILabelFactory<TNode, TEdge, TLabel> labelFactoy, ILabeler<TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            ClassifyIntegratedly(startNodes, graphStructure.GetOutflows, labelFactoy.CreateFromEdge, labelFactoy.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public static void ClassifyIntegratedly(IEnumerable<TNode> startNodes, FlowEnumerator<TNode, TEdge> getOutflows, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, SeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
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

            MonbetsuClassifier<Unit, TNode, TEdge, TLabel>.ClassifyIntegratedly(default, startNodes,
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

        public void ClassifyUniquely(IEnumerable<TNode> startNodes, ILabelFactory<TNode, TEdge, TLabel> labelFactory, IUniquelyLabeler<TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            ClassifyUniquely(startNodes, GetOutflows, labelFactory.CreateFromEdge, labelFactory.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public void ClassifyUniquely(IEnumerable<TNode> startNodes, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, UniquelySeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            ClassifyUniquely(startNodes, GetOutflows, createLabelFromEdge, createLabelFromSubgraph, labelEdge, labelSeries, labelParallel, labelKnot, cancellationToken);
        }

        public static void ClassifyUniquely(IEnumerable<TNode> startNodes, IGraphStructure<TNode, TEdge> graphStructure, ILabelFactory<TNode, TEdge, TLabel> labelFactoy, IUniquelyLabeler<TNode, TEdge, TLabel> labeler, CancellationToken cancellationToken = default)
        {
            ClassifyUniquely(startNodes, graphStructure.GetOutflows, labelFactoy.CreateFromEdge, labelFactoy.CreateFromSubgraph, labeler.LabelEdge, labeler.LabelSeriesSubgraph, labeler.LabelParallelSubgraph, labeler.LabelKnotSubgraph, cancellationToken);
        }

        public static void ClassifyUniquely(IEnumerable<TNode> startNodes, FlowEnumerator<TNode, TEdge> getOutflows, EdgeLabelCreator<TNode, TEdge, TLabel> createLabelFromEdge, SubgraphLabelCreator<TNode, TEdge, TLabel> createLabelFromSubgraph, EdgeLabeler<TNode, TEdge, TLabel>? labelEdge, UniquelySeriesSubgraphLabeler<TNode, TLabel>? labelSeries, ParallelSubgraphLabeler<TNode, TLabel>? labelParallel, KnotSubgraphLabeler<TNode, TLabel>? labelKnot, CancellationToken cancellationToken = default)
        {
            EdgeLabeler<Unit, TNode, TEdge, TLabel> edge = labelEdge switch
            {
                null => (_, f, s, t, l) => { },
                _ => (_, f, s, t, l) => labelEdge(f, s, t, l)
            };

            UniquelySeriesSubgraphLabeler<Unit, TNode, TLabel> series = labelSeries switch
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

            MonbetsuClassifier<Unit, TNode, TEdge, TLabel>.ClassifyUniquely(default, startNodes,
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

    

    public class MonbetsuException : Exception
    {
        protected internal MonbetsuException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
    }

    public class CyclicException : MonbetsuException
    {
        protected internal CyclicException(string? message = null, Exception? innerException = null) : base(message, innerException) { }
    }

    public class CyclicException<TNode, TEdge> : CyclicException
        where TNode : notnull
        where TEdge : notnull
    {
        public TNode FromNode { get; }
        public TEdge Edge { get; }
        public TNode ToNode { get; }


        internal CyclicException(TNode fromNode, TEdge edge, TNode toNode)
            : base($"A cycle is detected in the graph. One of nodes on the cyclic path is {fromNode} -> {toNode} ({edge}).")
        {
            FromNode = fromNode;
            Edge = edge;
            ToNode = toNode;
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
            internal static bool HasIntarsection<T>(this HashSet<T> lhs, HashSet<T> rhs)
            {
                var (smaller, larger) = lhs.Count > rhs.Count ? (rhs, lhs) : (lhs, rhs);
                return smaller.Any(larger.Contains);
            }

            internal static void RemoveWhereContainsUnorderedly<T>(this List<T> unorderedList, HashSet<T> targets)
            {
                var last = unorderedList.Count - 1;
                for (var c = 0; c <= last;)
                {
                    if (targets.Contains(unorderedList[c]))
                    {
                        unorderedList[c] = unorderedList[last--];
                    }
                    else
                    {
                        c++;
                    }
                }
                unorderedList.RemoveRange(last + 1, unorderedList.Count - last - 1);
            }

            internal static void RemoveAllUnorderedly<T>(this List<T> unorderedList, T target) where T : class
            {
                var last = unorderedList.Count - 1;
                for (var c = 0; c <= last;)
                {
                    if (unorderedList[c] == target)
                    {
                        unorderedList[c] = unorderedList[last--];
                    }
                    else
                    {
                        c++;
                    }
                }
                unorderedList.RemoveRange(last + 1, unorderedList.Count - last - 1);
            }

            internal static void RemoveWhereContainsIndexUnorderedly<T>(this List<T> unorderedList, IndexSet targetIndices)
            {
                var last = unorderedList.Count - 1;
                var targetCount = targetIndices.Count;

                if (targetCount == 0)
                {
                    return;
                }

                while(last >= 0 && targetCount > 0 && targetIndices.Contains(last))
                {
                    unorderedList.RemoveAt(last--);
                    targetCount--;
                }

                for (var c = 0; c <= last && targetCount > 0; c++)
                {
                    if (targetIndices.Contains(c))
                    {
                        unorderedList[c] = unorderedList[last--];
                        targetCount--;

                        while (last > c && targetIndices.Contains(last))
                        {
                            last--;
                            if (--targetCount == 0)
                            {
                                break;
                            }
                        }
                    }
                }
                unorderedList.RemoveRange(last + 1, unorderedList.Count - last - 1);
            }
        }
    }    
}
