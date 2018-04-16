using System;
using Liquibook.NET.Book;
using Liquibook.NET.Simple;
using Liquibook.NET.Types;

namespace Test
{
    public class FillChecker
    {
        private IOrder Order { get; }
        private Quantity ExpectedFilledQuantity { get; }
        private Quantity ExpectedOpenQuantity { get; }
        private int ExpectedFilledCost { get; }
        private OrderConditions Conditions { get; }
        
        public FillChecker(IOrder order, Quantity filledQuantity, int filledCost, OrderConditions conditions = 0)
        {
            Order = order;
            ExpectedFilledQuantity = (order as SimpleOrder).FilledQuantity + filledQuantity;
            ExpectedOpenQuantity = (order as SimpleOrder).OrderQty - ExpectedFilledQuantity;
            ExpectedFilledCost = (order as SimpleOrder).FilledCost + filledCost;
            Conditions = conditions;
        }
        
        public void AssertFillSuccess()
        {
            var simpleOrder = Order as SimpleOrder;

            if (ExpectedFilledQuantity != simpleOrder?.FilledQuantity)
            {
                throw new Exception("Unexpected fill quantity");
            }

            if (ExpectedOpenQuantity != simpleOrder?.OpenQuantity)
            {
                throw new Exception("Unexpected open quantity");
            }

            if (ExpectedFilledCost != simpleOrder?.FilledCost)
            {
                throw new Exception("Unexpected filled cost");
            }

            if (simpleOrder.State != OrderState.Complete && ExpectedOpenQuantity == 0)
            {
                throw new Exception("Unexpected state with no open quantity");
            } else if (ExpectedOpenQuantity > 0)
            {
                var ioc = (Conditions & OrderConditions.ImmediateOrCancel) != 0;
                if (simpleOrder.State != OrderState.Accepted && !ioc)
                {
                    throw new Exception("Unexpected state with open quantity");
                }

                if (simpleOrder.State != OrderState.Cancelled && ioc)
                {
                    throw new Exception("Unexpected state for IOC with open quantity");
                }
            }
        }
    }
}