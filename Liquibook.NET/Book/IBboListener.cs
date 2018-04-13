namespace Liquibook.NET.Book
{
    public interface IBboListener
    {
        void OnBboChange(OrderBook orderBook, Depth depth);
    }
}