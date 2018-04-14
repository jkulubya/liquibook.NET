using Liquibook.NET.Book;
using Liquibook.NET.Simple;
using Liquibook.NET.Types;

namespace Test
{
    internal class Utils
    {
        internal static bool AddAndVerify(OrderBook orderBook, IOrder order, bool matchExpected, bool completeExpected = false,
            OrderConditions conditions = 0)
        {
            var matched = orderBook.Add(order, conditions);
            if (matched == matchExpected)
            {
                if (completeExpected)
                {
                    return OrderState.Complete == (order as SimpleOrder)?.State;
                }else if ((conditions & OrderConditions.ImmediateOrCancel) != 0)
                {
                    return OrderState.Cancelled == (order as SimpleOrder)?.State;
                }
                else
                {
                    return OrderState.Accepted == (order as SimpleOrder)?.State;
                }
            }
            else
            {
                return false;
            }
        }

        internal static bool CancelAndVerify(OrderBook orderBook, IOrder order, OrderState expectedState)
        {
            orderBook.Cancel(order);
            return (order as SimpleOrder)?.State == expectedState;
        }

        internal static bool ReplaceAndVerify(OrderBook orderBook, IOrder order, int sizechange,
            Price newPrice, OrderState expectedState = OrderState.Accepted,
            Quantity matchQuantity = new Quantity())
        {
            var expectedOrderQuantity = order.OrderQty + sizechange;
            var expectedOpenQuantity = (order as SimpleOrder)?.OpenQuantity + sizechange - matchQuantity;
            var expectedPrice = (newPrice == Constants.PriceUnchanged) ? order.Price : newPrice;

            orderBook.Replace(order, sizechange, newPrice);

            var correct = true;
            if (expectedState != (order as SimpleOrder)?.State)
            {
                correct = false;
            }

            if (expectedOrderQuantity != order.OrderQty)
            {
                correct = false;
            }

            if (expectedOpenQuantity != (order as SimpleOrder)?.OpenQuantity)
            {
                correct = false;
            }

            if (expectedPrice != order.Price)
            {
                correct = false;
            }

            return correct;
        }

        internal static void VerifyFilled(IOrder order, Quantity filledQuantity, int filledCost,
            OrderConditions conditions = 0)
        {
            var expectedFilledQuantity = (order as SimpleOrder)?.FilledQuantity + filledQuantity;
            var expectedOpenQuantity = (order as SimpleOrder)?.OpenQuantity - expectedFilledQuantity;
            var expectedFilledCost = (order as SimpleOrder)?.FilledCost + filledCost;
        }
    }
}