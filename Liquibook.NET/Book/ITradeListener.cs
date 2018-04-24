using Liquibook.NET.Events;

namespace Liquibook.NET.Book
{
    public interface ITradeListener
    {
        void OnTrade(object sender, OnTradeEventArgs args);
    }
}