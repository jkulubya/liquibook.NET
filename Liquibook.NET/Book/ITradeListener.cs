using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public interface ITradeListener
    {
        void OnTrade(OrderBook book, Quantity quantity, int cost);
    }
}