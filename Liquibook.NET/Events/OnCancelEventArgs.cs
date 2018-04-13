using Liquibook.NET.Book;
using Liquibook.NET.Types;

namespace Liquibook.NET.Events
{
    public class OnCancelEventArgs
    {
        public IOrder Order { get; }
        public Quantity Quantity { get; }

        public OnCancelEventArgs(IOrder order, Quantity quantity)
        {
            Order = order;
            Quantity = quantity;
        }
    }
}