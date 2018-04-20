using System;
using BenchmarkDotNet.Attributes;
using Liquibook.NET.Book;

namespace Perf
{
    [MemoryDiagnoser]
    public class OrderBookBenchmark
    {
        private OrderBook Book { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            Book = new OrderBook();
        }

        [Benchmark]
        public void EnterInBook()
        {
            var order = new Order()
            {
                IsBuy = true,
                OrderQty = 100,
                Price = 100000
            };
            Book.Add(order);
        }
        
        [Benchmark]
        public void EnterThenCancelInBook()
        {
            var order = new Order()
            {
                IsBuy = true,
                OrderQty = 100,
                Price = 100000
            };
            Book.Add(order);
            Book.Cancel(order);
        }
    }
}