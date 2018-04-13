using Liquibook.NET.Book;

namespace Liquibook.NET.Events
{
    public class OnRejectEventArgs
    {
        public IOrder Order { get; }
        public string Reason { get; }

        public OnRejectEventArgs(IOrder order, string reason)
        {
            Order = order;
            Reason = reason;
        }
    }
}