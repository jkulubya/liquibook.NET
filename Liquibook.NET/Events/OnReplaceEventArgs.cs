using Liquibook.NET.Book;
using Liquibook.NET.Types;

namespace Liquibook.NET.Events
{
    public class OnReplaceEventArgs
    {
        public IOrder Order { get; }
        public Quantity CurrentQuantity { get; }
        public Quantity NewQuantity { get; }
        public Price NewPrice { get; }

        public OnReplaceEventArgs(IOrder order, Quantity currentQuantity, Quantity newQuantity, Price newPrice)
        {
            Order = order;
            CurrentQuantity = currentQuantity;
            NewQuantity = newQuantity;
            NewPrice = newPrice;
        }
    }
}