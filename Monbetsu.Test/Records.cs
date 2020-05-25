using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using FluentAssertions;
using Monbetsu.Test;
using NUnit.Framework;

namespace Monbetsu.Docs
{

    [Ignore("only for docs")]
    public class Records
    {
        class RecOptions : Recorder<Graph, Node, Edge, RecordLabel>.RecordOptions
        {
            public RecOptions()
            {
                EdgeDecorator = (dotEdge, nodes, label, flags, options) =>
                {
                    dotEdge.Label = label?.LabelText ?? "";
                };
                SeriesSubgraphDecorator = (dotEdge, nodes, label, flags, options) =>
                {
                    dotEdge.Label = $"{label.LabelText}\nSeries\n{{{string.Join(", ", nodes.Via) }}}";
                };
                ParallelSubgraphDecorator = (dotEdge, nodes, label, flags, options) =>
                {
                    dotEdge.Label = $"{label.LabelText}\nParallel\n{{{string.Join(", ", nodes.Via) }}}";
                };
                KnotSubgraphDecorator = (dotEdge, nodes, label, flags, options) =>
                {
                    dotEdge.Label = $"{label.LabelText}\nKnot\n{{{string.Join(", ", nodes.Via) }}}";
                };
            }
        }


        [Test]
        public void Example01()
        {
            var graphOption = new Action<Dot.RootGraph>(graph =>
            {
                graph.RankSep = 0.02;
                graph.NewRank = true;
                graph.Viewport = new Dot.Viewport
                {
                    Width = 500,
                    Height = 500,
                    Zoom = 0.7
                };

                graph.Ranks.Add(new Dot.Rank(Dot.RankType.source) { 0 });
                graph.Ranks.Add(new Dot.Rank(Dot.RankType.sink) { 6 });
                graph.Ranks.Add(new Dot.Rank(Dot.RankType.same) { 2, 5 });
            });

            var fileses = RecordGrouping(new[]
                {
                    new RecOptions
                    {
                        FitToLast = true,
                        OptionConfigurer = graphOption
                    },
                    new RecOptions
                    {
                        FitToLast = true,
                        HighlightLatests = true,
                        HighlightBelongingEdges = true,
                        NameFactory = (test, pattern, step) => $"highlighted-step-{step:D4}",
                        OptionConfigurer = graphOption,
                    },
                    new RecOptions
                    {
                        FitToLast = true,
                        HideLabels = true,
                        HideSubgraphEdges = true,
                        NameFactory = (test, pattern, step) => $"original-step-{step:D4}",
                        OptionConfigurer = graphOption
                    },
                },
                (from: 0, to: 1),
                (from: 1, to: 2),
                (from: 1, to: 3),
                (from: 2, to: 3),
                (from: 2, to: 4),
                (from: 3, to: 4),
                (from: 4, to: 6),
                (from: 0, to: 5),
                (from: 5, to: 6),
                (from: 0, to: 6)
                );

            File.Copy(fileses.Last().Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "example01-original-graph.png"), true);
            File.Copy(fileses.First().Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "example01-labeled-graph.png"), true);

            var gifPath = Path.Combine(PathUtil.SolutionDir, "docs", "images", "example01-steps.gif");
            Visualizer.GenerateAnimation(fileses.ElementAt(1), gifPath);
        }


        [Test]
        public void Series01()
        {
            var files = RecordGrouping(new RecOptions
            {
                    //HideLabels = true,
                    OptionConfigurer = graph =>
                    {
                        graph.RankDir = Dot.RankDir.LR;
                    },
                },
                (from: 0, to: 1),
                (from: 1, to: 2),
                (from: 2, to: 3)
                );

            File.Copy(files.Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "subgraph-series.png"), true);
        }

        [Test]
        public void Parallel01()
        {
            var files = RecordGrouping(new RecOptions
            {
                    //HideLabels = true,
                    OptionConfigurer = graph =>
                    {
                        graph.RankSep = 0.02;
                        graph.NewRank = true;
                    },
                },
                (from: 0, to: 1),
                (from: 0, to: 1),
                (from: 0, to: 1),
                (from: 0, to: 1)
                );

            File.Copy(files.Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "subgraph-parallel.png"), true);
        }

        [Test]
        public void Knot01()
        {
            var files = RecordGrouping(new RecOptions
                {
                    //HideLabels = true,
                    OptionConfigurer = graph =>
                    {
                        graph.RankSep = 0.02;
                        graph.NewRank = true;
                        //graph.Splines = Dot.Splines.polyline;
                        graph.Ranks.Add(new Dot.Rank(Dot.RankType.same) { 1, 2 });
                    },
                },
                (from: 0, to: 1),
                (from: 0, to: 2),
                (from: 1, to: 2),
                (from: 1, to: 3),
                (from: 2, to: 3)
                );

            File.Copy(files.Last(), Path.Combine(PathUtil.SolutionDir, "docs", "images", "subgraph-knot.png"), true);
        }

        private IEnumerable<string> RecordGrouping(params Input[] inputs)
            => RecordGrouping(new RecOptions(), inputs.AsEnumerable());


        private IEnumerable<string> RecordGrouping(IEnumerable<Input> inputs)
            => RecordGrouping(new RecOptions(), inputs.AsEnumerable());


        private IEnumerable<IEnumerable<string>> RecordGrouping(IEnumerable<RecOptions> options, params Input[] inputs)
            => options.Select(opt => RecordGrouping(opt, inputs)).ToList();
        
        private IEnumerable<string> RecordGrouping(RecOptions options, params Input[] inputs)
            => RecordGrouping(options, inputs.AsEnumerable());




        private IEnumerable<string> RecordGrouping(RecOptions options, IEnumerable<Input> inputs)
        {
            var outputPaths = new List<string>();

            foreach (var pattern in Pattarn.Generate(inputs, 0, options.Versions))
            {
                var env = pattern.Version switch
                {
                    ImplementationVersions.Latest => new RecordGraphEnvironment() as ITestGraphEnvironment,
                    ImplementationVersions.V03 => new Test.v03.RecordGraphEnvironment(),
                    _ => throw new Exception()
                };

                var graph = new Graph(pattern.Nodes, pattern.Edges);
                
                var outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "record-images", TestContext.CurrentContext.Test.Name);

                Directory.CreateDirectory(outputDir);

                var recorder = new Recorder<Graph, Node, Edge, RecordLabel>(node => node.Id.ToString(), options, outputDir);

                var locals = recorder.Record(pattern.Id, pattern.EdgeWithNodePair, (labelEdge, labelSeries, labelParallel, labelKnot) =>
                {
                    env.Classify(graph, pattern.StartNodes,
                        (g, t, e, f, l) => labelEdge(g, t, e, f, (RecordLabel)l),
                        (g, s, sl, e, l) => labelSeries(g, s, (RecordLabel)sl, e, (RecordLabel)l),
                        (g, s, sls, e, l) => labelParallel(g, s, sls.OfType<RecordLabel>(), e, (RecordLabel)l),
                        (g, s, sls, e, l) => labelKnot(g, s, sls.OfType<RecordLabel>(), e, (RecordLabel)l));
                });

                outputPaths.AddRange(locals);
            }
            return outputPaths;
        }


    }
}
