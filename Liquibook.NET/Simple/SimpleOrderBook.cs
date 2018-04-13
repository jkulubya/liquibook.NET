using Liquibook.NET.Book;

namespace Liquibook.NET.Simple
{
    public class SimpleOrderBook : DepthOrderBook
    {
        private int FillId { get; set; } = 0;

        protected override void PerformCallback(Callback callback)
        {
            base.PerformCallback(callback);
            switch (callback.Type)
            {
                case CallbackType.OrderAccept:
                    (callback.Order as SimpleOrder)?.Accept();
                    break;
                case CallbackType.OrderFill:
                    ++FillId;
                    var fillCost = callback.Quantity * callback.Price;
                    (callback.MatchedOrder as SimpleOrder)?.Fill(callback.Quantity, fillCost, FillId);
                    (callback.Order as SimpleOrder)?.Fill(callback.Quantity, fillCost, FillId);
                    break;
                case CallbackType.OrderCancel:
                    (callback.Order as SimpleOrder)?.Cancel();
                    break;
                case CallbackType.OrderReplace:
                    (callback.Order as SimpleOrder)?.Replace(callback.Delta, callback.Price);
                    break;
            }
        }
    }
}