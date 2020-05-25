using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Monbetsu
{
    public abstract partial class MonbetsuClassifier<TGraph, TNode, TEdge, TLabel>
    {
        public static void PipeTraverseTopologically(
                IEnumerable<TNode> startNodes,
                Action<TNode, int, int, bool> nodeVisitor,
                Action<TNode, TEdge, TNode, bool> edgeVisitor,
                Action<TNode, int, int, int> postNodeVisitor,
                Action<TNode, int> sweepedParentNodeVisitor,
                Action<TNode, int> sweepedGrandparentNodeVisitor,
                FlowEnumerator<TNode, int> inflows,
                FlowEnumerator<TNode, int> outflows,
                in TGraph graph,
                bool strictlySweepAll
                )
            //=> TraverseTopologically(
            //    startNodes,
            //    nodeVisitor,
            //    edgeVisitor,
            //    postNodeVisitor,
            //    sweepedParentNodeVisitor,
            //    sweepedGrandparentNodeVisitor,
            //    inflows,
            //    outflows,
            //    in graph,
            //    strictlySweepAll);
            => throw new NotImplementedException();

    }
}

