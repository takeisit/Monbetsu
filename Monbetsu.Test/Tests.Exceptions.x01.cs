using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Monbetsu.Test.x01
{
    public partial class Tests
    {


        [Test]
        public void TestCyclic000()
        {
            Assert.Catch<v01.TestGraphEnvironment.CyclicException>(() =>
            {
                var env = new v01.TestGraphEnvironment();

                var ns = new[] { new Node(0) };
                var es = new[] { new Edge(ns[0], ns[0], 1) };
                var graph = new Graph(ns, es);

                env.Classify(graph, ns, 
                    (g, startNode, edge, endNode) => { },
                    (g, startNode, sublabel, endNode) => { },
                    (g, startNode, sublabels, endNode) => { },
                    (g, startNode, sublabels, endNode) => { }
                    );
            });
        }

        [Test]
        public void TestCyclic001()
        {
            Assert.Catch<v01.TestGraphEnvironment.CyclicException>(() =>
            {
                var env = new v01.TestGraphEnvironment();

                var ns = new[] { new Node(0), new Node(1) };
                var es = new[] { new Edge(ns[0], ns[1], 1), new Edge(ns[1], ns[0], 2) };
                var graph = new Graph(ns, es);

                env.Classify(graph, new[] { ns[0] },
                    (g, startNode, edge, endNode) => { },
                    (g, startNode, sublabel, endNode) => { },
                    (g, startNode, sublabels, endNode) => { },
                    (g, startNode, sublabels, endNode) => { }
                    );
            });
        }

        [Test]
        public void TestCyclic002()
        {
            Assert.Catch<v01.TestGraphEnvironment.CyclicException>(() =>
            {
                var env = new v01.TestGraphEnvironment();

                var ns = new[] { new Node(0), new Node(1) };
                var es = new[] { new Edge(ns[0], ns[1], 1), new Edge(ns[1], ns[1], 2) };
                var graph = new Graph(ns, es);

                env.Classify(graph, new[] { ns[0] },
                    (g, startNode, edge, endNode) => { },
                    (g, startNode, sublabel, endNode) => { },
                    (g, startNode, sublabels, endNode) => { },
                    (g, startNode, sublabels, endNode) => { }
                    );
            });
        }

        [Test]
        public void TestCyclic003()
        {
            Assert.Catch<v01.TestGraphEnvironment.CyclicException>(() =>
            {
                var env = new v01.TestGraphEnvironment();

                var ns = new[] { new Node(0), new Node(1) };
                var es = new[] { new Edge(ns[0], ns[1], 1) };
                var graph = new Graph(ns, es);

                env.Classify(graph, new[] { ns[1] },
                    (g, startNode, edge, endNode) => { },
                    (g, startNode, sublabel, endNode) => { },
                    (g, startNode, sublabels, endNode) => { },
                    (g, startNode, sublabels, endNode) => { }
                    );
            });
        }

        [Test]
        public void TestCyclic004()
        {
            Assert.Catch<v01.TestGraphEnvironment.CyclicException>(() =>
            {
                var env = new v01.TestGraphEnvironment();

                var ns = new[] { new Node(0), new Node(1), new Node(2) };
                var es = new[] { new Edge(ns[0], ns[1], 1), new Edge(ns[2], ns[1], 2) };
                var graph = new Graph(ns, es);

                env.Classify(graph, new[] { ns[0] },
                    (g, startNode, edge, endNode) => { },
                    (g, startNode, sublabel, endNode) => { },
                    (g, startNode, sublabels, endNode) => { },
                    (g, startNode, sublabels, endNode) => { }
                    );
            });
        }
    }
}
