using Liquibook.NET.Book;
using Liquibook.NET.Simple;

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
    }
}