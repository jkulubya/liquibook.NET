using Liquibook.NET.Book;
using Liquibook.NET.Types;

namespace Liquibook.NET.Events
{
    public class OnTradeEventArgs
    {
        public OrderBook Book { get; }
        public Quantity Quantity { get; }
        public int Cost { get; }

        public OnTradeEventArgs(OrderBook book, Quantity quantity, int cost)
        {
            Book = book;
            Quantity = quantity;
            Cost = cost;
        }
    }
}