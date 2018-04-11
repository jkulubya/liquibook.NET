using System;
using System.Collections.Generic;
using System.Linq;
using Liquibook.NET.Types;
using DeferredMatches = System.Collections.Generic.List<(Liquibook.NET.Book.ComparablePrice Price, Liquibook.NET.Book.OrderTracker Tracker)>;
using TrackerMap = System.Collections.Generic.MultiMap<Liquibook.NET.Book.ComparablePrice, Liquibook.NET.Book.OrderTracker>;
using TrackerVec = System.Collections.Generic.List<Liquibook.NET.Book.OrderTracker>;

namespace Liquibook.NET.Book
{
    public class OrderBook
    {
        public TrackerMap Bids { get; private set; } = new TrackerMap();
        public TrackerMap Asks { get; private set; } = new TrackerMap();
        public TrackerMap StopBids { get; private set; } = new TrackerMap();
        public TrackerMap StopAsks { get; private set; } = new TrackerMap();
        private TrackerVec PendingOrders { get; set; } = new TrackerVec();
        public string Symbol { get; }
        private Price _marketPrice;

        public Price MarketPrice
        {
            get => _marketPrice;
            private set
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
                //TODO rejected order callback
            }
            else
            {
                //TODO accept order callback
                var inbound = new OrderTracker(order, orderConditions);
                if (inbound.Order.StopPrice != 0 && AddStopOrder(inbound))
                {
                    
                }
                else
                {
                    matched = SubmitOrder(inbound);
                    //todo accept order callback with filled quantity
                    if (inbound.ImmediateOrCancel && !inbound.Filled)
                    {
                        //todo cancel ioc that wasn't immediately filled
                    }
                }

                while (PendingOrders.Any())
                {
                    SubmitPendingOrders();
                }
                
                //todo book update callback
            }
            //todo callback_now();
            return matched;
        }

        public void Cancel(IOrder order)
        {
            var found = false;
            Quantity openQuantity;

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
                // cancel callback
                // book update callback
            }
            else
            {
                //cancel reject callback
            }
            //callback now
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
                        //TODO replace reject callback, order already filled
                        return false;
                    }
                }
                
                //TODO accept replace callback
                var newOpenQuantity = tracker.OpenQuantity + sizeDelta;
                tracker.ChangeQuantity(sizeDelta);

                if (newOpenQuantity == 0)
                {
                    //TODO cancel callback
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
                //TODO book update callback
            }
            else
            {
                // TODO replace reject callback
            }
            //TODO callback now
            return matched;
        }

        public bool AddStopOrder(OrderTracker tracker)
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

        public void CheckStopOrders(bool side, Price price, MultiMap<ComparablePrice, OrderTracker> stops)
        {
            var until = new ComparablePrice(side, price);
            foreach (var stop in stops)
            {
                if (until < stop.Key) break;
                PendingOrders.Add(stop.Value);
                stops.Remove(stop.Key);
            }
        }

        public void SubmitPendingOrders()
        {
            foreach (var pendingOrder in PendingOrders)
            {
                SubmitOrder(pendingOrder);
            }
            
            PendingOrders.Clear();
        }

        public bool SubmitOrder(OrderTracker order)
        {
            var orderPrice = order.Order.Price;
            return AddOrder(order, orderPrice);
        }
        
        public bool FindOnMarket(IOrder order, out OrderTracker result)
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

        public bool AddOrder(OrderTracker inbound, Price orderPrice)
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

        public bool CheckDeferredAons(DeferredMatches aons, TrackerMap deferredTrackers, TrackerMap marketTrackers)
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
                    deferredTrackers.Remove(aon.Price);
                }
            }

            return result;
        }

        public bool MatchOrder(OrderTracker inbound, Price inboundPrice, TrackerMap currentOrders,
            DeferredMatches deferredAons)
        {
            if (inbound.AllOrNone)
            {
                return MatchAonOrder(inbound, inboundPrice, currentOrders, deferredAons);
            }

            return MatchRegularOrder(inbound, inboundPrice, currentOrders, deferredAons);
        }

        public bool MatchRegularOrder(OrderTracker inbound, Price inboundPrice, TrackerMap currentOrders,
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
                                currentOrders.Remove(currentPrice);
                                inboundQuantity -= traded;
                            }
                        }
                        else
                        {
                            deferredAons.Add((currentOrder.Key, currentOrder.Value));
                        }
                    }
                    else
                    {
                        var traded = CreateTrade(inbound, currentOrderTracker);
                        if (traded > 0)
                        {
                            matched = true;
                            if(currentOrderTracker.Filled) currentOrders.Remove(currentOrder.Key);
                            inboundQuantity -= traded;
                        }
                    }
                }
            }

            return matched;
        }

        public bool MatchAonOrder(OrderTracker inbound, Price inboundPrice, TrackerMap currentOrders,
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
                        if (inboundQuantity <= currentQuantity + deferredQuantity)
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
                                    currentOrders.Remove(kvp.Key);
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
                    if (inboundQuantity <= currentQuantity + deferredQuantity)
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
                                currentOrders.Remove(kvp.Key);
                            }
                        }
                    }
                    else
                    {
                        deferredQuantity += currentQuantity;
                        deferredMatches.Remove((kvp.Key, kvp.Value));
                    }
                }
            }
            return matched;
        }

        public Quantity TryCreateDeferredTrades(OrderTracker inbound, DeferredMatches deferredMatches,
            Quantity maxQuantity, Quantity minQuantity, TrackerMap currentOrders)
        {
            Quantity traded = 0;
            var fills = new List<Quantity>(deferredMatches.Capacity);
            Quantity foundQuantity = 0;

            var index = 0;
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
                fills[index] = quantity;
                ++index;
            }

            if (minQuantity <= foundQuantity && foundQuantity <= maxQuantity)
            {
                var i = 0;
                foreach (var deferredMatch in deferredMatches)
                {
                    var tracker = deferredMatch.Tracker;
                    traded = CreateTrade(inbound, tracker, fills[index]);
                    if (tracker.Filled)
                    {
                        currentOrders.Remove(deferredMatch.Price);
                    }

                    ++i;
                }
            }

            return traded;
        }

        public Quantity CreateTrade(OrderTracker inboundTracker, OrderTracker currentTracker, int maxQuantity = 0)
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

            Quantity fillQuantity = Math.Min(maxQuantity,
                Math.Min(inboundTracker.OpenQuantity, currentTracker.OpenQuantity));

            if (fillQuantity > 0)
            {
                inboundTracker.Fill(fillQuantity);
                currentTracker.Fill(fillQuantity);
                MarketPrice = crossPrice;
                
                //TODO Callbacks
            }

            return fillQuantity;
        }
            
    }
}