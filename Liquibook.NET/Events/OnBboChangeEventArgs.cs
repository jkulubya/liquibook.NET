using Liquibook.NET.Book;

namespace Liquibook.NET.Events
{
    public class OnBboChangeEventArgs
    {
        public OrderBook Book { get; }
        public Depth Depth { get; }

        public OnBboChangeEventArgs(OrderBook book, Depth depth)
        {
            Book = book;
            Depth = depth;
        }
    }
}