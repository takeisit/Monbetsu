using System;
using BenchmarkDotNet.Running;

namespace Monbetsu.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchmarkAmongVersions>();
        }
    }
}
