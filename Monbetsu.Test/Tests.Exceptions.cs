using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace Monbetsu.Test
{
    public partial class Tests
    {


        [Test]
        public void TestCyclic000()
        {
            var exception = Assert.Catch<CyclicException<Node, Edge>>(() =>
            {
                var env = new TestGraphEnvironment();

                var ns = new[] { new Node(0) };
                var es = new[] { new Edge(ns[0], ns[0], 1) };
                var graph = new Graph(ns, es);

                env.Classify(graph, ns,
                    (g, startNode, edge, endNode) => new Label(),
                    (g, startNode, endNode) => new Label(),
                    (g, startNode, edge, endNode, label) => { },
                    (g, startNode, sublabel, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { }
                    );
            });
        }

        [Test]
        public void TestCyclic001()
        {
            var env = new TestGraphEnvironment();

            var ns = new[] { new Node(0), new Node(1) };
            var es = new[] { new Edge(ns[0], ns[1], 1), new Edge(ns[1], ns[0], 2) };
            var graph = new Graph(ns, es);


            var exception = Assert.Catch<CyclicException<Node, Edge>>(() =>
            {
                
                env.Classify(graph, new[] { ns[0] },
                    (g, startNode, edge, endNode) => new Label(),
                    (g, startNode, endNode) => new Label(),
                    (g, startNode, edge, endNode, label) => { },
                    (g, startNode, sublabel, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { }
                    );
            });

            exception.FromNode.Should().Be(ns[1]);
            exception.ToNode.Should().Be(ns[0]);
            exception.Edge.Should().Be(es[1]);
        }

        [Test]
        public void TestCyclic002()
        {
            Assert.Catch<CyclicException>(() =>
            {
                var env = new TestGraphEnvironment();

                var ns = new[] { new Node(0), new Node(1) };
                var es = new[] { new Edge(ns[0], ns[1], 1), new Edge(ns[1], ns[1], 2) };
                var graph = new Graph(ns, es);

                env.Classify(graph, new[] { ns[0] },
                    (g, startNode, edge, endNode) => new Label(),
                    (g, startNode, endNode) => new Label(),
                    (g, startNode, edge, endNode, label) => { },
                    (g, startNode, sublabel, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { }
                    );
            });
        }

        [Test]
        public void TestCancellation()
        {
            var cts = new CancellationTokenSource();
            Assert.Catch<OperationCanceledException>(() =>
            {
                var env = new TestGraphEnvironment();

                var ns = new[] { new Node(0), new Node(1), new Node(2) };
                var es = new[] { new Edge(ns[0], ns[1], 1), new Edge(ns[1], ns[2], 2) };
                var graph = new Graph(ns, es);

                env.Classify(graph, new[] { ns[0] },
                    (g, startNode, edge, endNode) =>
                    {
                        cts.Cancel();
                        return new Label();
                    },
                    (g, startNode, endNode) => { return new Label(); },
                    (g, startNode, edge, endNode, label) => { },
                    (g, startNode, sublabel, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { },
                    cts.Token);
            });
        }

        [Test]
        public void TestOtherException()
        {
            var exception = Assert.Catch<Monbetsu.MonbetsuException>(() =>
            {
                var env = new TestGraphEnvironment();

                var ns = new[] { new Node(0), new Node(1), new Node(2) };
                var es = new[] { new Edge(ns[0], ns[1], 1), new Edge(ns[1], ns[2], 2) };
                var graph = new Graph(ns, es);

                env.Classify(graph, new[] { ns[0] },
                    (g, startNode, edge, endNode) =>
                    {
                        throw new InvalidDataException();
                    },
                    (g, startNode, endNode) => { return new Label(); },
                    (g, startNode, edge, endNode, label) => { },
                    (g, startNode, sublabel, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { },
                    (g, startNode, sublabels, endNode, label) => { }
                    );
            });

            exception.InnerException.Should().BeOfType<InvalidDataException>();
        }
    }
}
