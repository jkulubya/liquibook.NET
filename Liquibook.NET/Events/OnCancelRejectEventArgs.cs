using Liquibook.NET.Book;

namespace Liquibook.NET.Events
{
    public class OnCancelRejectEventArgs
    {
        public IOrder Order { get; }
        public string Reason { get; }

        public OnCancelRejectEventArgs(IOrder order, string reason)
        {
            Order = order;
            Reason = reason;
        }
    }
}