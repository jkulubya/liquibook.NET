using System;
using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public class OrderTracker
    {
        private Quantity _openQuantity;
        public Quantity OpenQuantity => _openQuantity - Reserved;
        public bool AllOrNone => Convert.ToBoolean(Conditions & OrderConditions.AllOrNone);
        public bool ImmediateOrCancel => (Conditions & OrderConditions.ImmediateOrCancel) != 0;
        private OrderConditions Conditions { get; set; }
        private int Reserved { get; set; } = 0;
        public IOrder Order { get; set; }
        
        public OrderTracker(IOrder order, OrderConditions condition, bool liquibookOrderKnowsConditions = false)
        {
            Order = order;
            _openQuantity = order.OrderQty;
            Reserved = 0;
            Conditions = condition;

            if (liquibookOrderKnowsConditions)
            {
                if (order.AllOrNone)
                {
                    Conditions = condition | OrderConditions.AllOrNone;
                }

                if (order.ImmediateOrCancel)
                {
                    Conditions = condition | OrderConditions.ImmediateOrCancel;
                }
            }
        }

        public Quantity Reserve(int reserved)
        {
            Reserved += reserved;
            return _openQuantity - Reserved;
        }

        public void ChangeQuantity(int delta)
        {
            if ((delta < 0) && OpenQuantity < Math.Abs(delta))
            {
                throw new Exception("Replace size reduction larger than open quantity"); //TODO errors
            }

            _openQuantity += delta;
        }

        public void Fill(Quantity quantity)
        {
            if (quantity > _openQuantity)
            {
                throw new Exception("Fill size is larger than open quantity"); //TODO errors
            }

            _openQuantity -= quantity;
        }

        public bool Filled => _openQuantity == 0;

        public Quantity FilledQuantity => Order.OrderQty - OpenQuantity;
    }
}