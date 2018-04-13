using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public class Callback
    {
        public CallbackType Type { get; set; }
        public IOrder Order { get; set; }
        public IOrder MatchedOrder { get; set; }
        public Quantity Quantity { get; set; }
        public Price Price { get; set; }
        public FillFlag Flag { get; set; }
        public int Delta { get; set; }
        public string RejectReason { get; set; }

        public static Callback Accept(IOrder order)
        {
            var result = new Callback
            {
                Type = CallbackType.OrderAccept,
                Order = order
            };
            return result;
        }

        public static Callback Reject(IOrder order, string reason)
        {
            var result = new Callback
            {
                Type = CallbackType.OrderReject,
                Order = order,
                RejectReason = reason
            };
            return result;
        }

        public static Callback Fill(IOrder inboundOrder, IOrder matchedOrder, Quantity fillQuantity, Price fillPrice,
            FillFlag fillFlag)
        {
            var result = new Callback
            {
                Type = CallbackType.OrderFill,
                Order = inboundOrder,
                MatchedOrder = matchedOrder,
                Quantity = fillQuantity,
                Price = fillPrice,
                Flag = fillFlag
            };
            return result;
        }

        public static Callback Cancel(IOrder order, Quantity openQuantity)
        {
            var result = new Callback
            {
                Type = CallbackType.OrderCancel,
                Order = order,
                Quantity = openQuantity
            };
            return result;
        }

        public static Callback CancelReject(IOrder order, string reason)
        {
            var result = new Callback
            {
                Type = CallbackType.OrderCancelReject,
                Order = order,
                RejectReason = reason
            };
            return result;
        }

        public static Callback Replace(IOrder order, Quantity currentOpenQuantity, int sizeDelta, Price newPrice)
        {
            var result = new Callback
            {
                Type = CallbackType.OrderReplace,
                Order = order,
                Quantity = currentOpenQuantity,
                Delta = sizeDelta,
                Price = newPrice
            };
            return result;
        }

        public static Callback ReplaceReject(IOrder order, string reason)
        {
            var result = new Callback
            {
                Type = CallbackType.OrderReplaceReject,
                Order = order,
                RejectReason = reason
            };
            return result;
        }

        public static Callback BookUpdate(OrderBook book)
        {
            return new Callback
            {
                Type = CallbackType.BookUpdate
            };
        }
    }

    public enum CallbackType
    {
        Unknown,
        OrderAccept,
        OrderReject,
        OrderFill,
        OrderCancel,
        OrderCancelReject,
        OrderReplace,
        OrderReplaceReject,
        BookUpdate
    }

    public enum FillFlag
    {
        NeitherFilled = 0,
        InboundFilled = 1,
        MatchedFilled = 2,
        BothFilled = 4
    }
}