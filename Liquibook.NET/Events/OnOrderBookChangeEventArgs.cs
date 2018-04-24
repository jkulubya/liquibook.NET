using Liquibook.NET.Book;

namespace Liquibook.NET.Events
{
    public class OnOrderBookChangeEventArgs
    {
        public OrderBook Book { get; }

        public OnOrderBookChangeEventArgs(OrderBook book)
        {
            Book = book;
        }
    }
}