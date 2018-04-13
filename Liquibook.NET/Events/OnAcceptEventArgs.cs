using Liquibook.NET.Book;
using Liquibook.NET.Types;

namespace Liquibook.NET.Events
{
    public class OnAcceptEventArgs
    {
        public IOrder Order { get; }
        public Quantity Quantity { get; }

        public OnAcceptEventArgs(IOrder order, Quantity quantity)
        {
            Order = order;
            Quantity = quantity;
        }
    }
}