using Liquibook.NET.Book;

namespace Liquibook.NET.Events
{
    public class OnReplaceRejectEventArgs
    {
        public IOrder Order { get; }
        public string Reason { get; }

        public OnReplaceRejectEventArgs(IOrder order, string reason)
        {
            Order = order;
            Reason = reason;
        }
    }
}