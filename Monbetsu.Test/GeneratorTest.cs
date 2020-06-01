using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Monbetsu.Test.Utils;
using NUnit.Framework;

namespace Monbetsu.Test
{
    
    public partial class GeneratorTest
    {
        [Test]
        public void Random001([Values(0, 1, 2, 3, 4, 5)]int seed)
        {
            var generator = new GraphGenerator
            {
                NumberOfEdgesPerNode = 1..6,
                NumberOfLayers = 2..7,
                NumberOfNodes = 3..30
            };

            var graph = generator.GenerateNestedly(new Random(seed));

            TestContext.WriteLine(graph.GetSummary());

            var label = 0;
            var edge = 0;
            var series = 0;
            var parallel = 0;
            var knot = 0;
            Monbetsu.MonbetsuClassifier.Classify(graph, graph.StartNodes,
                (g, n) => n.Outflows,
                (g, f, e, t) => label++,
                (g, f, t) => label++,
                (g, f, e, t, l) => edge++,
                (g, f, s, t, l) => series++,
                (g, f, s, t, l) => parallel++,
                (g, f, s, t, l) => knot++
                );

            TestContext.WriteLine($"label: {label}, edge: {edge}, series: {series}, parallel: {parallel}, knot: {knot}");
        }


    }
}
