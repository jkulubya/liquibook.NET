using System;
using System.Collections.Generic;
using System.Linq;
using Liquibook.NET.Events;
using Liquibook.NET.Types;
using DeferredMatches = System.Collections.Generic.List<(Liquibook.NET.Book.ComparablePrice Price, Liquibook.NET.Book.OrderTracker Tracker)>;
using TrackerMap = System.Collections.Generic.MultiMap<Liquibook.NET.Book.ComparablePrice, Liquibook.NET.Book.OrderTracker>;
using TrackerVec = System.Collections.Generic.List<Liquibook.NET.Book.OrderTracker>;
using TypedCallback = Liquibook.NET.Book.Callback;
using Callbacks = System.Collections.Generic.List<Liquibook.NET.Book.Callback>;

namespace Liquibook.NET.Book
{
    public class OrderBook
    {
        public TrackerMap Bids { get; private set; } = new TrackerMap();
        public TrackerMap Asks { get; private set; } = new TrackerMap();
        public TrackerMap StopBids { get; private set; } = new TrackerMap();
        public TrackerMap StopAsks { get; private set; } = new TrackerMap();
        private TrackerVec PendingOrders { get; set; } = new TrackerVec();
        private Callbacks Callbacks { get; set; } = new Callbacks();
        private bool HandlingCallbacks { get; set; } = false;
        public string Symbol { get; }
        private Price _marketPrice;

        public Price MarketPrice
        {
            get => _marketPrice;
            set
            {
                var oldMarketPrice = MarketPrice;
                _marketPrice = value;

                if (value > oldMarketPrice || oldMarketPrice == Constants.MarketOrderPrice)
                {
                    var buySide = true;
                    CheckStopOrders(buySide, value, StopBids);
                }
                else if (value < oldMarketPrice || oldMarketPrice == Constants.MarketOrderPrice)
                {
                    var buySide = false;
                    CheckStopOrders(buySide, value, StopAsks);
                }
            }
        }

        public OrderBook(string symbol = "UNKNOWN")
        {
            Symbol = symbol;
            MarketPrice = Constants.MarketOrderPrice;
        }

        public bool Add(IOrder order, OrderConditions orderConditions = OrderConditions.NoConditions)
        {
            var matched = false;

            if (order.OrderQty == 0)
            {
                Callbacks.Add(Callback.Reject(order, "size must be positive"));
            }
            else
            {
                var acceptCbIndex = Callbacks.Count;
                Callbacks.Add(Callback.Accept(order));
                var inbound = new OrderTracker(order, orderConditions);
                if (inbound.Order.StopPrice != 0 && AddStopOrder(inbound))
                {
                    // The order has been added to stops
                }
                else
                {
                    matched = SubmitOrder(inbound);
                    Callbacks[acceptCbIndex].Quantity = inbound.FilledQuantity;
                    if (inbound.ImmediateOrCancel && !inbound.Filled)
                    {
                        Callbacks.Add(Callback.Cancel(order, 0));
                    }
                }

                while (PendingOrders.Any())
                {
                    SubmitPendingOrders();
                }
                Callbacks.Add(Callback.BookUpdate(this));
            }
            CallbackNow();
            return matched;
        }

        public void Cancel(IOrder order)
        {
            var found = false;
            Quantity openQuantity = 0;

            if (order.IsBuy)
            {
                FindOnMarket(order, out var bid);
                if (bid != null)
                {
                    openQuantity = bid.OpenQuantity;
                    Bids.Erase(bid);
                    found = true;
                }
            }
            else
            {
                FindOnMarket(order, out var ask);
                if (ask != null)
                {
                    openQuantity = ask.OpenQuantity;
                    Asks.Erase(ask);
                    found = true;
                }
            }

            if (found)
            {
                Callbacks.Add(Callback.Cancel(order, openQuantity));
                Callbacks.Add(Callback.BookUpdate(this));
            }
            else
            {
                Callbacks.Add(Callback.CancelReject(order, "not found"));
            }
            CallbackNow();
        }

        public bool Replace(IOrder order, int sizeDelta, Price newPrice)
        {
            var matched = false;
            var priceChange = newPrice != order.Price;
            var price = newPrice == Constants.PriceUnchanged ? order.Price : newPrice;

            var market = order.IsBuy ? Bids : Asks;

            if (FindOnMarket(order, out var tracker))
            {
                if (sizeDelta < 0 && tracker.OpenQuantity < -sizeDelta)
                {
                    sizeDelta = -tracker.OpenQuantity;
                    if (sizeDelta == 0)
                    {
                        Callbacks.Add(Callback.ReplaceReject(tracker.Order, "order is already filled"));
                        return false;
                    }
                }
                
                Callbacks.Add(Callback.Replace(order, tracker.OpenQuantity, sizeDelta, price));
                var newOpenQuantity = tracker.OpenQuantity + sizeDelta;
                tracker.ChangeQuantity(sizeDelta);

                if (newOpenQuantity == 0)
                {
                    Callbacks.Add(Callback.Cancel(order, 0));
                    market.Erase(tracker);
                }
                else
                {
                    market.Erase(tracker);
                    matched = AddOrder(tracker, price);
                }

                while (PendingOrders.Any())
                {
                    SubmitPendingOrders();
                }
                Callbacks.Add(Callback.BookUpdate(this));
            }
            else
            {
                Callbacks.Add(Callback.ReplaceReject(order, "not found"));
            }
            CallbackNow();
            return matched;
        }

        protected bool AddStopOrder(OrderTracker tracker)
        {
            var isBuy = tracker.Order.IsBuy;
            var key = new ComparablePrice(isBuy, tracker.Order.StopPrice);
            var isStopped = key < _marketPrice;

            if (isStopped)
            {
                if(isBuy)
                {
                    StopBids.Add(key, tracker);
                }
                else
                {
                    StopAsks.Add(key, tracker);
                }
            }

            return isStopped;
        }

        protected void CheckStopOrders(bool side, Price price, MultiMap<ComparablePrice, OrderTracker> stops)
        {
            var until = new ComparablePrice(side, price);
            foreach (var stop in stops)
            {
                if (until < stop.Key) break;
                PendingOrders.Add(stop.Value);
                stops.Erase(stop.Value);
            }
        }

        protected void SubmitPendingOrders()
        {
            foreach (var pendingOrder in PendingOrders)
            {
                SubmitOrder(pendingOrder);
            }
            
            PendingOrders.Clear();
        }

        private bool SubmitOrder(OrderTracker order)
        {
            var orderPrice = order.Order.Price;
            return AddOrder(order, orderPrice);
        }
        
        protected bool FindOnMarket(IOrder order, out OrderTracker result)
        {
            var key = new ComparablePrice(order.IsBuy, order.Price);
            var sideMap = order.IsBuy ? Bids : Asks;

            foreach (var keyValuePair in sideMap)
            {
                if (keyValuePair.Value.Order == order)
                {
                    result = keyValuePair.Value;
                    return true;
                }
                else if (key < keyValuePair.Key)
                {
                    result = null;
                    return false;
                }
            }

            result = null;
            return false;
        }

        private bool AddOrder(OrderTracker inbound, Price orderPrice)
        {
            var matched = false;
            var order = inbound.Order;
            var deferredAons = new DeferredMatches();

            matched = MatchOrder(inbound, orderPrice, order.IsBuy ? Asks : Bids, deferredAons);

            if (inbound.OpenQuantity > 0 && !inbound.ImmediateOrCancel)
            {
                if (order.IsBuy)
                {
                    Bids.Add(new ComparablePrice(true, orderPrice), inbound);
                    if (CheckDeferredAons(deferredAons, Asks, Bids)) matched = true;
                }
                else
                {
                    Asks.Add(new ComparablePrice(false, orderPrice), inbound);
                    if (CheckDeferredAons(deferredAons, Bids, Asks)) matched = true;
                }
            }

            return matched;
        }

        protected bool CheckDeferredAons(DeferredMatches aons, TrackerMap deferredTrackers, TrackerMap marketTrackers)
        {
            var result = false;
            var ignoredAons = new DeferredMatches();

            foreach (var aon in aons)
            {
                var currentPrice = aon.Price;
                var tracker = aon.Tracker;
                var matched = MatchOrder(tracker, currentPrice.Price, marketTrackers, ignoredAons);
                result |= matched;
                if (tracker.Filled)
                {
                    deferredTrackers.Erase(aon.Tracker);
                }
            }

            return result;
        }

        protected bool MatchOrder(OrderTracker inbound, Price inboundPrice, TrackerMap currentOrders,
            DeferredMatches deferredAons)
        {
            if (inbound.AllOrNone)
            {
                return MatchAonOrder(inbound, inboundPrice, currentOrders, deferredAons);
            }

            return MatchRegularOrder(inbound, inboundPrice, currentOrders, deferredAons);
        }

        protected bool MatchRegularOrder(OrderTracker inbound, Price inboundPrice, TrackerMap currentOrders,
            DeferredMatches deferredAons)
        {
            var matched = false;
            var inboundQuantity = inbound.OpenQuantity;

            foreach (var currentOrder in currentOrders)
            {
                if (!inbound.Filled)
                {
                    var currentPrice = currentOrder.Key;
                    if (!currentPrice.Matches(inboundPrice)) break;
                    var currentOrderTracker = currentOrder.Value;
                    var currentQuantity = currentOrderTracker.OpenQuantity;

                    if (currentOrderTracker.AllOrNone)
                    {
                        if (currentQuantity <= inboundQuantity)
                        {
                            var traded = CreateTrade(inbound, currentOrderTracker);

                            if (traded > 0)
                            {
                                matched = true;
                                currentOrders.Erase(currentOrderTracker);
                                inboundQuantity -= traded;
                            }
                        }
                        else
                        {
                            deferredAons.Add((currentOrder.Key, currentOrderTracker));
                        }
                    }
                    else
                    {
                        var traded = CreateTrade(inbound, currentOrderTracker);
                        if (traded > 0)
                        {
                            matched = true;
                            if(currentOrderTracker.Filled) currentOrders.Erase(currentOrderTracker);
                            inboundQuantity -= traded;
                        }
                    }
                }
            }

            return matched;
        }

        protected bool MatchAonOrder(OrderTracker inbound, Price inboundPrice, TrackerMap currentOrders,
            DeferredMatches deferredAons)
        {
            var matched = false;
            var inboundQuantity = inbound.OpenQuantity;
            var deferredQuantity = 0;
            var deferredMatches = new DeferredMatches();

            foreach (var kvp in currentOrders)
            {
                var currentPrice = kvp.Key;
                if(!currentPrice.Matches(inboundPrice)) break;

                var currentOrder = kvp.Value;
                var currentQuantity = currentOrder.OpenQuantity;

                if (currentOrder.AllOrNone)
                {
                    if (currentQuantity <= inboundQuantity)
                    {
                        if (inboundQuantity <= (currentQuantity + deferredQuantity))
                        {
                            var maxQuantity = inboundQuantity - currentQuantity;
                            if (maxQuantity == TryCreateDeferredTrades(inbound, deferredMatches, maxQuantity,
                                    maxQuantity, currentOrders))
                            {
                                inboundQuantity -= maxQuantity;
                                var traded = CreateTrade(inbound, currentOrder);
                                if (traded > 0)
                                {
                                    inboundQuantity -= traded;
                                    matched = true;
                                    currentOrders.Erase(kvp.Value);
                                }
                            }
                        }
                        else
                        {
                            deferredQuantity += currentQuantity;
                            deferredMatches.Add((kvp.Key, kvp.Value));
                        }
                    }
                    else
                    {
                        deferredAons.Add((kvp.Key, kvp.Value));
                    }
                }
                else
                {
                    if (inboundQuantity <= (currentQuantity + deferredQuantity))
                    {
                        var traded = TryCreateDeferredTrades(inbound, deferredMatches, inboundQuantity,
                            (inboundQuantity > currentQuantity)
                                ? (inboundQuantity - currentQuantity)
                                : new Quantity(),
                            currentOrders);
                        if (inboundQuantity <= currentQuantity + traded)
                        {
                            traded += CreateTrade(inbound, currentOrder);
                            if (traded > 0)
                            {
                                inboundQuantity -= traded;
                                matched = true;
                            }

                            if (currentOrder.Filled)
                            {
                                currentOrders.Erase(kvp.Value);
                            }
                        }
                    }
                    else
                    {
                        deferredQuantity += currentQuantity;
                        deferredMatches.Add((kvp.Key, kvp.Value));
                    }
                }
            }
            return matched;
        }

        protected Quantity TryCreateDeferredTrades(OrderTracker inbound, DeferredMatches deferredMatches,
            Quantity maxQuantity, Quantity minQuantity, TrackerMap currentOrders)
        {
            Quantity traded = 0;
            var fills = new List<Quantity>(deferredMatches.Capacity);
            Quantity foundQuantity = 0;

            foreach (var deferredMatch in deferredMatches)
            {
                var tracker = deferredMatch.Tracker;
                var quantity = tracker.OpenQuantity;

                if (foundQuantity + quantity > maxQuantity)
                {
                    if (tracker.AllOrNone)
                    {
                        quantity = 0;
                    }
                    else
                    {
                        quantity = maxQuantity - foundQuantity;
                    }
                }

                foundQuantity += quantity;
                fills.Add(quantity);
            }

            if (minQuantity <= foundQuantity && foundQuantity <= maxQuantity)
            {
                var i = 0;
                foreach (var deferredMatch in deferredMatches)
                {
                    var tracker = deferredMatch.Tracker;
                    traded += CreateTrade(inbound, tracker, fills[i]);
                    if (tracker.Filled)
                    {
                        currentOrders.Erase(deferredMatch.Tracker);
                    }

                    ++i;
                }
            }

            return traded;
        }

        protected Quantity CreateTrade(OrderTracker inboundTracker, OrderTracker currentTracker, int maxQuantity = -1)
        {
            var crossPrice = currentTracker.Order.Price;

            if (Constants.MarketOrderPrice == crossPrice)
            {
                crossPrice = inboundTracker.Order.Price;
            }

            if (Constants.MarketOrderPrice == crossPrice)
            {
                crossPrice = MarketPrice;
            }

            if (Constants.MarketOrderPrice == crossPrice)
            {
                return 0;
            }

            Quantity fillQuantity = maxQuantity != -1
                ? Math.Min(maxQuantity,
                    Math.Min(inboundTracker.OpenQuantity, currentTracker.OpenQuantity))
                : Math.Min(inboundTracker.OpenQuantity, currentTracker.OpenQuantity);

            if (fillQuantity > 0)
            {
                inboundTracker.Fill(fillQuantity);
                currentTracker.Fill(fillQuantity);
                MarketPrice = crossPrice;
                var fillFlag = FillFlag.NeitherFilled;
                if (inboundTracker.OpenQuantity == 0)
                {
                    fillFlag = fillFlag | FillFlag.InboundFilled;
                }

                if (currentTracker.OpenQuantity == 0)
                {
                    fillFlag = fillFlag | FillFlag.MatchedFilled;
                }

                Callbacks.Add(Callback.Fill(inboundTracker.Order, currentTracker.Order, fillQuantity, crossPrice,
                    fillFlag));
            }

            return fillQuantity;
        }

        public void CallbackNow()
        {
            if (!HandlingCallbacks)
            {
                HandlingCallbacks = true;
                while (Callbacks.Any())
                {
                    var workingCallbacks = Callbacks.ToList();
                    Callbacks.Clear();
                    foreach (var callback in workingCallbacks)
                    {
                        PerformCallback(callback);
                    }
                }

                HandlingCallbacks = false;
            }
        }
        
        protected virtual void PerformCallback(TypedCallback callback)
        {
            switch (callback.Type)
            {
                    case CallbackType.OrderFill:
                        var fillCost = callback.Price * callback.Quantity;
                        var inboundFilled = (callback.Flag & (FillFlag.InboundFilled | FillFlag.BothFilled)) != 0;
                        var matchedFilled = (callback.Flag & (FillFlag.MatchedFilled | FillFlag.BothFilled)) != 0;
                        OnFill(callback.Order, callback.MatchedOrder, callback.Quantity, fillCost, inboundFilled,
                            matchedFilled);
                        OnFillEvent?.Invoke(this,
                            new OnFillEventArgs(callback.Order, callback.MatchedOrder, callback.Quantity, fillCost,
                                inboundFilled, matchedFilled));
                        OnTrade(this, callback.Quantity, fillCost);
                        OnTradeEvent?.Invoke(this, new OnTradeEventArgs(this, callback.Quantity, fillCost));
                        break;
                    case CallbackType.OrderAccept:
                        OnAccept(callback.Order, callback.Quantity);
                        OnAcceptEvent?.Invoke(this, new OnAcceptEventArgs(callback.Order, callback.Quantity));
                        break;
                    case CallbackType.OrderReject:
                        OnReject(callback.Order, callback.RejectReason);
                        OnRejectEvent?.Invoke(this, new OnRejectEventArgs(callback.Order, callback.RejectReason));
                        break;
                    case CallbackType.OrderCancel:
                        OnCancel(callback.Order, callback.Quantity);
                        OnCancelEvent?.Invoke(this, new OnCancelEventArgs(callback.Order, callback.Quantity));
                        break;
                    case CallbackType.OrderCancelReject:
                        OnCancelReject(callback.Order,callback.RejectReason);
                        OnCancelRejectEvent?.Invoke(this, new OnCancelRejectEventArgs(callback.Order, callback.RejectReason));
                        break;
                    case CallbackType.OrderReplace:
                        OnReplace(callback.Order, callback.Order.OrderQty, callback.Order.OrderQty + callback.Delta,
                            callback.Price);
                        OnReplaceEvent?.Invoke(this,
                            new OnReplaceEventArgs(callback.Order, callback.Order.OrderQty,
                                callback.Order.OrderQty + callback.Delta, callback.Price));
                        break;
                    case CallbackType.OrderReplaceReject:
                        OnReplaceReject(callback.Order, callback.RejectReason);
                        OnReplaceRejectEvent?.Invoke(this,
                            new OnReplaceRejectEventArgs(callback.Order, callback.RejectReason));
                        break;
                    case CallbackType.BookUpdate:
                        OnOrderBookChange();
                        OnOrderBookChangeEvent?.Invoke(this, new OnOrderBookChangeEventArgs(this));
                        break;
                    default:
                        throw new Exception($"Unexpected callback type {callback.Type}");
            }
        }
        
        protected virtual void OnAccept(IOrder order, Quantity quantity){}
        protected virtual void OnReject(IOrder order, string reason){}
        protected virtual void OnFill(IOrder order, IOrder matchedOrder, Quantity fillQuantity, int fillCost,
            bool inboundOrderFilled, bool matchedOrderFilled){}
        protected virtual void OnCancel(IOrder order, Quantity quantity){}
        protected virtual void OnCancelReject(IOrder order, string reason){}
        protected virtual void OnReplace(IOrder order, Quantity currentQuantity, Quantity newQuantity, Price newPrice){}
        protected virtual void OnReplaceReject(IOrder order, string reason){}
        protected virtual void OnTrade(OrderBook book, Quantity quantity, int cost){}
        protected virtual void OnOrderBookChange(){}

        protected event EventHandler<OnAcceptEventArgs> OnAcceptEvent;
        protected event EventHandler<OnRejectEventArgs> OnRejectEvent;
        protected event EventHandler<OnFillEventArgs> OnFillEvent;
        protected event EventHandler<OnCancelEventArgs> OnCancelEvent;
        protected event EventHandler<OnCancelRejectEventArgs> OnCancelRejectEvent;
        protected event EventHandler<OnReplaceEventArgs> OnReplaceEvent;
        protected event EventHandler<OnReplaceRejectEventArgs> OnReplaceRejectEvent;
        protected event EventHandler<OnTradeEventArgs> OnTradeEvent; 
        protected event EventHandler<OnOrderBookChangeEventArgs> OnOrderBookChangeEvent;
        
        public void SetOrderListener(IOrderListener orderListener)
        {
            if(orderListener == null) return;
            OnAcceptEvent = null;
            OnRejectEvent = null;
            OnFillEvent = null;
            OnCancelEvent = null;
            OnCancelRejectEvent = null;
            OnReplaceEvent = null;
            OnReplaceRejectEvent = null;
            OnAcceptEvent += orderListener.OnAccept;
            OnRejectEvent += orderListener.OnReject;
            OnFillEvent += orderListener.OnFill;
            OnCancelEvent += orderListener.OnCancel;
            OnCancelRejectEvent += orderListener.OnCancelReject;
            OnReplaceEvent += orderListener.OnReplace;
            OnReplaceRejectEvent += orderListener.OnReplaceReject;
        }

        public void SetTradeListener(ITradeListener tradeListener)
        {
            if(tradeListener == null) return;
            OnTradeEvent = null;
            OnTradeEvent += tradeListener.OnTrade;
        }

        public void SetOrderBookListener(IOrderBookListener orderBookListener)
        {
            if (orderBookListener == null) return;
            OnOrderBookChangeEvent = null;
            OnOrderBookChangeEvent += orderBookListener.OnOrderBookChange;
        }
    }
}