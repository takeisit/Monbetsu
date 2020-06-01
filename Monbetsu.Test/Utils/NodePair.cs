using System;
using System.Collections.Generic;

namespace Monbetsu.Test.Utils
{    
    public class NodePair<TNode, TVia> : IEquatable<NodePair<TNode, TVia>?>
    {
        public TNode StartNode { get; }
        public TVia Via { get; }
        public TNode EndNode { get; }

        public NodePair(TNode startNode, TVia via, TNode endNode)
        {
            StartNode = startNode;
            Via = via;
            EndNode = endNode;
        }

        public void Deconstruct(out TNode startNode, out TVia via, out TNode endNode)
        {
            startNode = StartNode;
            via = Via;
            endNode = EndNode;
        }

        public NodePair<TNode, TNewVia> Transform<TNewVia>(Func<TVia, TNewVia> func)
        {
            return new NodePair<TNode, TNewVia>(StartNode, func(Via), EndNode);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as NodePair<TNode, TVia>);
        }

        public bool Equals(NodePair<TNode, TVia>? other)
        {
            return other != null &&
                    EqualityComparer<TNode>.Default.Equals(StartNode, other.StartNode) &&
                    EqualityComparer<TVia>.Default.Equals(Via, other.Via) &&
                    EqualityComparer<TNode>.Default.Equals(EndNode, other.EndNode);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartNode, Via, EndNode);
        }

        public override string ToString() => $"{{{StartNode} -> {EndNode} via {Via}}}";
    }


}
