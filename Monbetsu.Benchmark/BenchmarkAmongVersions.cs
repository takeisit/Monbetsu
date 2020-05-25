using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Monbetsu.Benchmark
{
    [MemoryDiagnoser]
    [ReturnValueValidator]
    [ResultColumn]
    [SummaryColumn]
    public class BenchmarkAmongVersions
    {
        public struct Result
        {
            public int Label;
            public int Edge;
            public int Series;
            public int Parallel;
            public int Knot;
            public string GraphSummary;

            public override string ToString()
            {
                return $@"Label:{Label} Edge: {Edge}, Series: {Series}, Parallel: {Parallel}, Knot: {Knot}";
            }
        }

        private Test.GraphGenerator.Graph graph = default!;
        private string graphSummary = "";
        private Result lastResult;

        [Params(0, 1)]
        public int Seed;

        [GlobalSetup]
        public void Setup()
        {
            var generator = new Test.GraphGenerator
            {
                NumberOfEdgesPerNode = 1..6,
                NumberOfLayers = 2..7,
                NumberOfNodes = 3..30
            };

            graph = generator.GenerateNestedly(new Random(Seed));

            graphSummary = graph.GetSummary();
        }


        [GlobalCleanup]
        public void Cleanup()
        {
            SummaryColumnAttribute.Column.Output(graphSummary);
            ResultColumnAttribute.Column.Output(lastResult);
        }
        

        [Benchmark(Baseline = true)]
        public Result Latest()
        {
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

            lastResult = new Result { Label = label, Edge = edge, Series = series, Parallel = parallel, Knot = knot, GraphSummary = graphSummary };
            return lastResult;
        }


        [Benchmark]
        public Result Ver03()
        {
            var label = 0;
            var edge = 0;
            var series = 0;
            var parallel = 0;
            var knot = 0;
            Monbetsu.v03.MonbetsuClassifier.Classify(graph, graph.StartNodes,
                (g, n) => n.Outflows,
                (g, f, e, t) => label++,
                (g, f, t) => label++,
                (g, f, e, t, l) => edge++,
                (g, f, s, t, l) => series++,
                (g, f, s, t, l) => parallel++,
                (g, f, s, t, l) => knot++
                );

            lastResult = new Result { Label = label, Edge = edge, Series = series, Parallel = parallel, Knot = knot, GraphSummary = graphSummary };
            return lastResult;
        }

        internal class ExtraColumn : IColumn
        {
            internal const string ExtraPrefix = "//#EXTRA#";
            public string Id => $"Extra:{Key}";

            public string ColumnName => $"Result:{Key}";

            public bool AlwaysShow => true;

            public ColumnCategory Category => ColumnCategory.Custom;

            public int PriorityInCategory => 0;

            public bool IsNumeric => false;

            public UnitType UnitType => UnitType.Dimensionless;

            public string Legend => $"Extra:{Key}";

            public string Key { get; }

            private string Prefix => $"{ExtraPrefix}{Key}##";

            internal ExtraColumn(string key)
            {
                Key = key;
            }

            public void Output<T>(T extra)
            {
                Console.Write(Prefix);
                Console.WriteLine(extra);
            }

            public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
            {
                var prefix = Prefix;
                return summary[benchmarkCase].ExecuteResults
                    .SelectMany(_ => _.ExtraOutput)
                    .Where(_ => _.StartsWith(prefix)).Select(_ => _.Substring(prefix.Length))
                    .LastOrDefault() ?? "";
            }

            public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);

            public bool IsAvailable(Summary summary) => true;

            public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        internal class SummaryColumnAttribute : ColumnConfigBaseAttribute
        {
            internal static readonly ExtraColumn Column = new ExtraColumn("Summary");

            public SummaryColumnAttribute() : base(Column)
            {

            }
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        internal class ResultColumnAttribute : ColumnConfigBaseAttribute
        {
            internal static readonly ExtraColumn Column = new ExtraColumn("Result");

            public ResultColumnAttribute() : base(Column)
            {
                
            }
        }
    }

    
}
