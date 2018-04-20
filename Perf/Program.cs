using System;
using BenchmarkDotNet.Running;

namespace Perf
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<OrderBookBenchmark>();
        }
    }
}