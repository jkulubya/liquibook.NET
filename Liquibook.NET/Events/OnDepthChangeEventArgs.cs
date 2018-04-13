using Liquibook.NET.Book;

namespace Liquibook.NET.Events
{
    public class OnDepthChangeEventArgs
    {
        public OrderBook Book { get; }
        public Depth Depth { get; }

        public OnDepthChangeEventArgs(OrderBook book, Depth depth)
        {
            Book = book;
            Depth = depth;
        }
    }
}