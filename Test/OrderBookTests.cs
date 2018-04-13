using System;
using System.Collections.Generic;
using Liquibook.NET.Book;
using Liquibook.NET.Simple;
using Liquibook.NET.Types;
using Xunit;
using SimpleTracker = Liquibook.NET.Book.OrderTracker;
using TrackerMap = System.Collections.Generic.MultiMap<Liquibook.NET.Book.ComparablePrice, Liquibook.NET.Book.OrderTracker>;

namespace Test
{
    public class OrderBookTests
    {
        [Fact]
        public void TestBidsMultimapSortCorrect()
        {
            var bids = new TrackerMap();
            var order0 = new SimpleOrder(true, 1250, 100);
            var order1 = new SimpleOrder(true, 1255, 100);
            var order2 = new SimpleOrder(true, 1240, 100);
            var order3 = new SimpleOrder(true, Constants.MarketOrderPrice, 100);
            var order4 = new SimpleOrder(true, 1245, 100);
            
            bids.Add(new ComparablePrice(true, order0.Price), new SimpleTracker(order0, OrderConditions.NoConditions));
            bids.Add(new ComparablePrice(true, order1.Price), new SimpleTracker(order1, OrderConditions.NoConditions));
            bids.Add(new ComparablePrice(true, order2.Price), new SimpleTracker(order2, OrderConditions.NoConditions));
            bids.Add(new ComparablePrice(true, order3.Price), new SimpleTracker(order3, OrderConditions.NoConditions));
            bids.Add(new ComparablePrice(true, order4.Price), new SimpleTracker(order4, OrderConditions.NoConditions));

            var expectedOrder = new[] {order3, order1, order0, order4, order2}; 
            var index = 0;

            foreach (var pair in bids)
            {
                Assert.Equal(expectedOrder[index].Price, pair.Value.Order.Price);
                Assert.Same(expectedOrder[index], pair.Value.Order);
                ++index;
            }
            
            //TODO missing assertions here
        }

        [Fact]
        public void TestAsksMultimapSortCorrect()
        {
            var asks = new TrackerMap();
            var order0 = new SimpleOrder(false, 3250, 100);
            var order1 = new SimpleOrder(false, 3235, 800);
            var order2 = new SimpleOrder(false, 3230, 200);
            var order3 = new SimpleOrder(false, 0, 200);
            var order4 = new SimpleOrder(false, 3245, 100);
            var order5 = new SimpleOrder(false, 3265, 200);
            
            asks.Add(new ComparablePrice(false, order0.Price), new SimpleTracker(order0, OrderConditions.NoConditions));
            asks.Add(new ComparablePrice(false, order1.Price), new SimpleTracker(order1, OrderConditions.NoConditions));
            asks.Add(new ComparablePrice(false, order2.Price), new SimpleTracker(order2, OrderConditions.NoConditions));
            asks.Add(new ComparablePrice(false, order3.Price), new SimpleTracker(order3, OrderConditions.NoConditions));
            asks.Add(new ComparablePrice(false, order4.Price), new SimpleTracker(order4, OrderConditions.NoConditions));
            asks.Add(new ComparablePrice(false, order5.Price), new SimpleTracker(order5, OrderConditions.NoConditions));
            
            var expectedOrder = new[] {order3, order2, order1, order4, order0, order5};
            var index = 0;
            
            foreach (var pair in asks)
            {
                Assert.Equal(expectedOrder[index].Price, pair.Value.Order.Price);
                Assert.Same(expectedOrder[index], pair.Value.Order);
                ++index;
            }
            
            //TODO missing assertions here
        }
    }
}