using System;
using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public class DepthLevel
    {
        public Price Price { get; private set; }
        public int OrderCount { get; private set; }
        public Quantity AggregateQty { get; private set; }
        public bool IsExcess { get; set; }
        public int LastChange { get; set; }

        public DepthLevel(Price price, bool isExcess)
        {
            Price = price;
            OrderCount = 0;
            AggregateQty = 0;
            IsExcess = isExcess;
        }
        
        //TODO: Assignment Operator override c++, needed? no?

        public bool ChangedSince(int lastPublishedChange)
        {
            return LastChange > lastPublishedChange;
        }

        public void AddOrder(Quantity qty)
        {
            ++OrderCount;
            AggregateQty += qty;
        }

        public bool CloseOrder(Quantity qty)
        {
            var empty = false;

            if (OrderCount == 0)
            {
                throw new Exception("Order count too low");//TODO Custom exceptions?
            }
            
            // If this is the last order, reset the level
            else if (OrderCount == 1)
            {
                OrderCount = 0;
                AggregateQty = 0;
                empty = true;
            }
            // Else decrease
            else
            {
                --OrderCount;
                if (AggregateQty >= qty)
                {
                    AggregateQty -= qty;
                }
                else
                {
                    throw new Exception("Level quantity too low"); //TODO Custom exceptions?
                }
            }

            return empty;
        }

        public void IncreaseQty(Quantity qty)
        {
            AggregateQty += qty;
        }

        public void DecreaseQty(Quantity qty)
        {
            AggregateQty -= qty;
        }

        public void Set(Price price, Quantity quantity, int orderCount, int lastChange = 0)
        {
            Price = price;
            AggregateQty = quantity;
            OrderCount = orderCount;
            LastChange = lastChange;
        }
    }
}