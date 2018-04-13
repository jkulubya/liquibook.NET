using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public interface IOrderListener
    {
        void OnAccept(IOrder order);
        void OnReject(IOrder order, string reason);
        void OnFill(IOrder order, IOrder matchedOrder, Quantity fillQuantity, int fillCost);
        void OnCancel(IOrder order);
        void OnCancelReject(IOrder order, string reason);
        void OnReplace(IOrder order, Quantity sizeDelta, Price newPrice);
        void OnReplaceReject(IOrder order, string reason);
    }
}