using System;
using System.Collections.Generic;
using Liquibook.NET.Book;
using Liquibook.NET.Events;
using Liquibook.NET.Simple;
using Liquibook.NET.Types;
using Xunit;

namespace Test
{
    public class ListenersTests
    {
        [Fact]
        public void TestOrderCallbacks()
        {
            var order0 = new SimpleOrder(false, 3250, 100);
            var order1 = new SimpleOrder(true, 3250, 800);
            var order2 = new SimpleOrder(false, 3230, 0);
            var order3 = new SimpleOrder(false, 3240, 200);
            var order4 = new SimpleOrder(true, 3250, 600);

            var listener = new OrderCbListener();
            var order_book = new OrderBook();
            order_book.SetOrderListener(listener);
            // Add order, should be accepted
            order_book.Add(order0);
            Assert.Equal(1, listener.accepts_.Count);
            listener.Reset();
            // Add matching order, should be accepted, followed by a fill
            order_book.Add(order1);
            Assert.Equal(1, listener.accepts_.Count);
            Assert.Equal(1, listener.fills_.Count);
            listener.Reset();
            // Add invalid order, should be rejected
            order_book.Add(order2);
            Assert.Equal(1, listener.rejects_.Count);
            listener.Reset();
            // Cancel only valid order, should be cancelled
            order_book.Cancel(order1);
            Assert.Equal(1, listener.cancels_.Count);
            listener.Reset();
            // Cancel filled order, should be rejected
            order_book.Cancel(order0);
            Assert.Equal(1, listener.cancel_rejects_.Count);
            listener.Reset();
            // Add a new order and replace it, should be replaced
            order_book.Add(order3);
            order_book.Replace(order3, 0, 3250);
            Assert.Equal(1, listener.accepts_.Count);
            Assert.Equal(1, listener.replaces_.Count);
            listener.Reset();
            // Add matching order, should be accepted, followed by a fill
            order_book.Add(order4);
            Assert.Equal(1, listener.accepts_.Count);
            Assert.Equal(1, listener.fills_.Count);
            listener.Reset();
            // Replace matched order, with too large of a size decrease, replace
            // should be rejected
            order_book.Replace(order3, -500, 0);
            Assert.Equal(0, listener.replaces_.Count);
            Assert.Equal(1, listener.replace_rejects_.Count);
        }

        [Fact]
        public void TestOrderBookCallbacks()
        {
            var order0 = new SimpleOrder(false, 3250, 100);
            var order1 = new SimpleOrder(true, 3250, 800);
            var order2 = new SimpleOrder(false, 3230, 0);
            var order3 = new SimpleOrder(false, 3240, 200);
            var order4 = new SimpleOrder(true, 3250, 600);

            var listener = new OrderBookCbListener();
            var order_book = new OrderBook();
            order_book.SetOrderBookListener(listener);
            // Add order, should be accepted
            order_book.Add(order0);
            Assert.Equal(1, listener.changes_.Count);
            listener.Reset();
            // Add matching order, should be accepted, followed by a fill
            order_book.Add(order1);
            Assert.Equal(1, listener.changes_.Count);
            listener.Reset();
            // Add invalid order, should be rejected
            order_book.Add(order2);
            Assert.Equal(0, listener.changes_.Count); // NO CHANGE
            listener.Reset();
            // Cancel only valid order, should be cancelled
            order_book.Cancel(order1);
            Assert.Equal(1, listener.changes_.Count);
            listener.Reset();
            // Cancel filled order, should be rejected
            order_book.Cancel(order0);
            Assert.Equal(0, listener.changes_.Count); // NO CHANGE
            listener.Reset();
            // Add a new order and replace it, should be replaced
            order_book.Add(order3);
            order_book.Replace(order3, 0, 3250);
            Assert.Equal(2, listener.changes_.Count);
            listener.Reset();
            // Add matching order, should be accepted, followed by a fill
            order_book.Add(order4);
            Assert.Equal(1, listener.changes_.Count);
            listener.Reset();
            // Replace matched order, with too large of a size decrease, replace
            // should be rejected
            order_book.Replace(order3, -500, 0);
            Assert.Equal(0, listener.changes_.Count); // NO CHANGE
        }

        [Fact]
        public void TestDepthCallbacks()
        {
            var buy0 = new SimpleOrder(true, 3250, 100);
            var buy1 = new SimpleOrder(true, 3249, 800);
            var buy2 = new SimpleOrder(true, 3248, 300);
            var buy3 = new SimpleOrder(true, 3247, 200);
            var buy4 = new SimpleOrder(true, 3246, 600);
            var buy5 = new SimpleOrder(true, 3245, 300);
            var buy6 = new SimpleOrder(true, 3244, 100);
            var sell0 = new SimpleOrder(false, 3250, 300);
            var sell1 = new SimpleOrder(false, 3251, 200);
            var sell2 = new SimpleOrder(false, 3252, 200);
            var sell3 = new SimpleOrder(false, 3253, 400);
            var sell4 = new SimpleOrder(false, 3254, 300);
            var sell5 = new SimpleOrder(false, 3255, 100);
            var sell6 = new SimpleOrder(false, 3255, 100);

            var listener = new DepthCbListener();
            var order_book = new DepthOrderBook();
            order_book.SetDepthListener(listener);
            // Add buy orders, should be accepted
            order_book.Add(buy0);
            order_book.Add(buy1);
            order_book.Add(buy2);
            order_book.Add(buy3);
            order_book.Add(buy4);
            Assert.Equal(5, listener.changes_.Count);
            listener.Reset();

            // Add buy orders past end, should be accepted, but not affect depth
            order_book.Add(buy5);
            order_book.Add(buy6);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();

            // Add sell orders, should be accepted and affect depth
            order_book.Add(sell5);
            order_book.Add(sell4);
            order_book.Add(sell3);
            order_book.Add(sell2);
            order_book.Add(sell1);
            order_book.Add(sell0);
            Assert.Equal(6, listener.changes_.Count);
            listener.Reset();

            // Add sell order past end, should be accepted, but not affect depth
            order_book.Add(sell6);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();
        }

        [Fact]
        public void TestBboCallbacks()
        {
            var buy0 = new SimpleOrder(true, 3250, 100);
            var buy1 = new SimpleOrder(true, 3249, 800);
            var buy2 = new SimpleOrder(true, 3248, 300);
            var buy3 = new SimpleOrder(true, 3247, 200);
            var buy4 = new SimpleOrder(true, 3246, 600);
            var buy5 = new SimpleOrder(true, 3245, 300);
            var buy6 = new SimpleOrder(true, 3244, 100);
            var sell0 = new SimpleOrder(false, 3250, 300);
            var sell1 = new SimpleOrder(false, 3251, 200);
            var sell2 = new SimpleOrder(false, 3252, 200);
            var sell3 = new SimpleOrder(false, 3253, 400);
            var sell4 = new SimpleOrder(false, 3254, 300);
            var sell5 = new SimpleOrder(false, 3255, 100);
            var sell6 = new SimpleOrder(false, 3255, 100);

            var listener = new BboCbListener();
            var order_book = new DepthOrderBook();
            order_book.SetBboListener(listener);
            // Add buy orders, should be accepted
            order_book.Add(buy0);
            Assert.Equal(1, listener.changes_.Count);
            listener.Reset();
            order_book.Add(buy1);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();
            order_book.Add(buy2);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();
            order_book.Add(buy3);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();
            order_book.Add(buy4);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();

            // Add buy orders past end, should be accepted, but not affect depth
            order_book.Add(buy5);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();
            order_book.Add(buy6);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();

            // Add sell orders, should be accepted and affect bbo
            order_book.Add(sell2);
            Assert.Equal(1, listener.changes_.Count);
            listener.Reset();
            order_book.Add(sell1);
            Assert.Equal(1, listener.changes_.Count);
            listener.Reset();
            order_book.Add(sell0);
            Assert.Equal(1, listener.changes_.Count);
            listener.Reset();
            // Add sell orders worse than best bid, should not effect bbo
            order_book.Add(sell5);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();
            order_book.Add(sell4);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();
            order_book.Add(sell3);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();

            // Add sell order past end, should be accepted, but not affect depth
            order_book.Add(sell6);
            Assert.Equal(0, listener.changes_.Count);
            listener.Reset();
        }

        [Fact]
        public void TestTradeCallbacks()
        {
            var order0 = new SimpleOrder(false, 3250, 100);
            var order1 = new SimpleOrder(true, 3250, 800);
            var order2 = new SimpleOrder(false, 3230, 0);
            var order3 = new SimpleOrder(false, 3240, 200);
            var order4 = new SimpleOrder(true, 3250, 600);

            var listener = new TradeCbListener();
            var order_book = new OrderBook();
            order_book.SetTradeListener(listener);
            // Add order, should be accepted
            order_book.Add(order0);
            Assert.Equal(0, listener.quantities_.Count);
            listener.Reset();
            // Add matching order, should result in a trade
            order_book.Add(order1);
            Assert.Equal(1, listener.quantities_.Count);
            Assert.Equal(1, listener.costs_.Count);
            Assert.Equal((Quantity)100, listener.quantities_[0]);
            Assert.Equal(100 * 3250, listener.costs_[0]);
            listener.Reset();
            // Add invalid order, should be rejected
            order_book.Add(order2);
            Assert.Equal(0, listener.quantities_.Count);
            listener.Reset();
            // Cancel only valid order, should be cancelled
            order_book.Cancel(order1);
            Assert.Equal(0, listener.quantities_.Count);
            listener.Reset();
            // Cancel filled order, should be rejected
            order_book.Cancel(order0);
            Assert.Equal(0, listener.quantities_.Count);
            listener.Reset();
            // Add a new order and replace it, should be replaced
            order_book.Add(order3);
            order_book.Replace(order3, 0, 3250);
            Assert.Equal(0, listener.quantities_.Count);
            listener.Reset();
            // Add matching order, should be accepted, followed by a fill
            order_book.Add(order4);
            Assert.Equal(1, listener.quantities_.Count);
            Assert.Equal(1, listener.costs_.Count);
            listener.Reset();
            // Replace matched order, with too large of a size decrease, replace
            // should be rejected
            order_book.Replace(order3, -500, 0);
            Assert.Equal(0, listener.quantities_.Count);
        }

    }

    public class OrderCbListener : IOrderListener
    {
        public void OnAccept(object sender, OnAcceptEventArgs args)
        {
            accepts_.Add(args.Order as SimpleOrder);
        }

        public void OnReject(object sender, OnRejectEventArgs args)
        {
            rejects_.Add(args.Order as SimpleOrder);
        }

        public void OnFill(object sender, OnFillEventArgs args)
        {
            fills_.Add(args.Order as SimpleOrder);
        }

        public void OnCancel(object sender, OnCancelEventArgs args)
        {
            cancels_.Add(args.Order as SimpleOrder);
        }

        public void OnCancelReject(object sender, OnCancelRejectEventArgs args)
        {
            cancel_rejects_.Add(args.Order as SimpleOrder);
        }

        public void OnReplace(object sender, OnReplaceEventArgs args)
        {
            replaces_.Add(args.Order as SimpleOrder);
        }

        public void OnReplaceReject(object sender, OnReplaceRejectEventArgs args)
        {
            replace_rejects_.Add(args.Order as SimpleOrder);
        }

        public void Reset()
        {
            accepts_.Clear();
            rejects_.Clear();
            fills_.Clear();
            cancels_.Clear();
            cancel_rejects_.Clear();
            replaces_.Clear();
            replace_rejects_.Clear();
        }
        
        public List<SimpleOrder> accepts_ { get; } = new List<SimpleOrder>();
        public List<SimpleOrder> rejects_ { get; } = new List<SimpleOrder>();
        public List<SimpleOrder> fills_ { get; } = new List<SimpleOrder>();
        public List<SimpleOrder> cancels_ { get; } = new List<SimpleOrder>();
        public List<SimpleOrder> cancel_rejects_ { get; } = new List<SimpleOrder>();
        public List<SimpleOrder> replaces_ { get; } = new List<SimpleOrder>();
        public List<SimpleOrder> replace_rejects_ { get; } = new List<SimpleOrder>();
    }

    public class TradeCbListener : ITradeListener
    {
        public void OnTrade(object sender, OnTradeEventArgs args)
        {
            quantities_.Add(args.Quantity);
            costs_.Add(args.Cost);
        }

        public void Reset()
        {
            quantities_.Clear();
            costs_.Clear();
        }
        public List<Quantity> quantities_ { get; } = new List<Quantity>();
        public List<int> costs_ { get; } = new List<int>();
    }
    
    public class OrderBookCbListener : IOrderBookListener
    {
        public void OnOrderBookChange(object sender, OnOrderBookChangeEventArgs args)
        {
            changes_.Add(args.Book);
        }

        public void Reset()
        {
            changes_.Clear();
        }
        
        public List<OrderBook> changes_ { get; } = new List<OrderBook>();
    }
    
    public class DepthCbListener : IDepthListener
    {
        public void OnDepthChange(object sender, OnDepthChangeEventArgs args)
        {
            changes_.Add(args.Book as DepthOrderBook);
        }

        public void Reset()
        {
            changes_.Clear();
        }
        
        public List<DepthOrderBook> changes_ { get; } = new List<DepthOrderBook>();
        
    }
    
    public class BboCbListener : IBboListener
    {
        public void OnBboChange(object sender, OnBboChangeEventArgs args)
        {
            changes_.Add(args.Book as DepthOrderBook);
        }

        public void Reset()
        {
            changes_.Clear();
        }
        
        public List<DepthOrderBook> changes_ { get; } = new List<DepthOrderBook>();
        
    }
}