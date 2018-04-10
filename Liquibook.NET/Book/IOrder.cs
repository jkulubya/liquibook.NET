using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public interface IOrder
    {
        bool IsLimit { get; set; }
        bool IsBuy { get; set; }
        Price Price { get; set; }
        Price StopPrice { get; set; }
        Quantity OrderQty { get; set; }
        bool AllOrNone { get; set; }
        bool ImmediateOrCancel { get; set; }
        
        // TODO: investigate methods at bottom of related cpp class
    }
}