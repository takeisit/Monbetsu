using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monbetsu.x02
{
    public abstract partial class GraphEnvironment<TGraph, TNode, TEdge, TLabel>
    {
        public static void PipeTraverseTopologically(
                IEnumerable<TNode> startNodes,
                Action<TNode, int, int, bool> nodeVisitor,
                Action<TNode, TEdge, TNode, bool> edgeVisitor,
                Action<TNode, int, int, int> postNodeVisitor,
                Action<TNode, int> sweepedParentNodeVisitor,
                Action<TNode, int> sweepedGrandparentNodeVisitor,
                Func<TGraph, TNode, IEnumerable<(TEdge edge, TNode fromNode)>> inflows,
                Func<TGraph, TNode, IEnumerable<(TEdge edge, TNode toNode)>> outflows,
                in TGraph graph,
                bool strictlySweepAll
                )
            => TraverseTopologically(
                startNodes,
                nodeVisitor,
                edgeVisitor,
                postNodeVisitor,
                sweepedParentNodeVisitor,
                sweepedGrandparentNodeVisitor,
                inflows,
                outflows,
                in graph,
                strictlySweepAll);
    }
}

