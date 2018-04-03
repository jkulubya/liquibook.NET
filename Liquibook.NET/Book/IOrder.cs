namespace Liquibook.NET.Book
{
    public interface IOrder
    {
        bool IsLimit { get; set; }
        bool IsBuy { get; set; }
        int Price { get; set; }
        int StopPrice { get; set; }
        int OrderQty { get; set; }
        bool AllOrNone { get; set; }
        bool ImmediateOrCancel { get; set; }
        
        // TODO: investigate methods at bottom of related cpp class
    }
}