namespace Liquibook.NET.Book
{
    public interface IDepthListener
    {
        void OnDepthChange(OrderBook orderBook, Depth depth);
    }
}