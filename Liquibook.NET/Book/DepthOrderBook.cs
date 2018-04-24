using System;
using System.Collections.Generic;
using System.Linq;
using Liquibook.NET.Events;
using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public class DepthOrderBook : OrderBook
    {
        public Depth Depth { get; }
        
        public DepthOrderBook(string symbol = "UNKNOWN", int depthSize = 5) : base(symbol)
        {
            Depth = new Depth(depthSize);
        }

        protected override void OnAccept(IOrder order, Quantity quantity)
        {
            if (order.IsLimit)
            {
                if (quantity == order.OrderQty)
                {
                    Depth.IgnoreFillQuantity(quantity, order.IsBuy);
                }
                else
                {
                    Depth.AddOrder(order.Price, order.OrderQty, order.IsBuy);
                }
            }
        }

        protected override void OnFill(IOrder order, IOrder matchedOrder, Quantity fillQuantity, int fillCost, bool inboundOrderFilled,
            bool matchedOrderFilled)
        {
            if (matchedOrder.IsLimit)
            {
                Depth.FillOrder(matchedOrder.Price, fillQuantity, matchedOrderFilled, matchedOrder.IsBuy);
            }

            if (order.IsLimit)
            {
                Depth.FillOrder(order.Price, fillQuantity, inboundOrderFilled, order.IsBuy);
            }
        }

        protected override void OnCancel(IOrder order, Quantity quantity)
        {
            if (order.IsLimit)
            {
                Depth.CloseOrder(order.Price, quantity, order.IsBuy);
            }
        }

        protected override void OnReplace(IOrder order, Quantity currentQuantity, Quantity newQuantity, Price newPrice)
        {
            Depth.ReplaceOrder(order.Price, newPrice, currentQuantity, newQuantity, order.IsBuy);
        }

        protected override void OnOrderBookChange()
        {
            if (Depth.Changed)
            {
                OnDepthChangeEvent?.Invoke(this, new OnDepthChangeEventArgs(this, Depth));
                var lastChange = Depth.LastPublishedChange;

                if ((!Depth.Bids.FirstOrDefault().Equals(default(KeyValuePair<Price, DepthLevel>)) &&
                     Depth.Bids.First().Value.ChangedSince(lastChange)) ||
                    (!Depth.Asks.FirstOrDefault().Equals(default(KeyValuePair<Price, DepthLevel>)) &&
                     Depth.Asks.First().Value.ChangedSince(lastChange)))
                {
                    OnBboChangeEvent?.Invoke(this, new OnBboChangeEventArgs(this, Depth));
                }
            }
            Depth.Published();
        }

        protected event EventHandler<OnBboChangeEventArgs> OnBboChangeEvent;
        protected event EventHandler<OnDepthChangeEventArgs> OnDepthChangeEvent;

        public void SetBboListener(IBboListener bboListener)
        {
            if(bboListener == null) return;
            OnBboChangeEvent = null;
            OnBboChangeEvent += bboListener.OnBboChange;
        }

        public void SetDepthListener(IDepthListener depthListener)
        {
            if (depthListener == null) return;
            OnDepthChangeEvent = null;
            OnDepthChangeEvent += depthListener.OnDepthChange;
        }
    }
}