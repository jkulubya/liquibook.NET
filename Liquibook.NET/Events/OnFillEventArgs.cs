using Liquibook.NET.Book;
using Liquibook.NET.Types;

namespace Liquibook.NET.Events
{
    public class OnFillEventArgs
    {
        public IOrder Order { get; }
        public IOrder MatchedOrder { get; }
        public Quantity FillQuantity { get; }
        public int FillCost { get; }
        public bool InboundOrderFilled { get; }
        public bool MatchedOrderFilled { get; }

        public OnFillEventArgs(IOrder order, IOrder matchedOrder, Quantity fillQuantity, int fillCost,
            bool inboundOrderFilled, bool matchedOrderFilled)
        {
            Order = order;
            MatchedOrder = matchedOrder;
            FillQuantity = fillQuantity;
            FillCost = fillCost;
            InboundOrderFilled = inboundOrderFilled;
            MatchedOrderFilled = matchedOrderFilled;
        }
    }
}