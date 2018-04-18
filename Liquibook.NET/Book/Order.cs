using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public class Order : IOrder
    {
        public virtual bool IsLimit => Price > 0;
        public bool IsBuy { get; set; }
        public Price Price { get; set; }
        public Price StopPrice { get; set; }
        public Quantity OrderQty { get; set; }
        public virtual bool AllOrNone => false;
        public virtual bool ImmediateOrCancel  => false;
    }
}