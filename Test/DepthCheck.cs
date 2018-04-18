using System;
using System.Linq;
using Liquibook.NET.Book;
using Liquibook.NET.Types;

namespace Test
{
    public class DepthCheck
    {
        private int AskIndex { get; set; } = 0;
        private int BidIndex { get; set; } = 0;
        private Depth Depth { get; set; }

        public DepthCheck(Depth depth)
        {
            Depth = depth;
            Reset();
        }

        public static bool VerifyDepth(DepthLevel level, Price price, int count, Quantity quantity)
        {
            var matched = true;
            if (level.Price != price) matched = false;
            if (level.OrderCount != count) matched = false;
            if (level.AggregateQty != quantity) matched = false;
            if (level.IsExcess) matched = false;

            return matched;
        }

        public bool VerifyBid(Price price, int count, Quantity quantity)
        {
            var result = false;
            if (Depth.Bids.Count == 0)
            {
                if (!(price != 0 || count != 0 || quantity != 0))
                {
                    result = true;
                }
            }
            else
            {
                result = VerifyDepth(Depth.Bids.ElementAt(BidIndex).Value, price, count, quantity);
            }
            ++BidIndex;
            return result;
        }
        
        public bool VerifyAsk(Price price, int count, Quantity quantity)
        {
            var result = false;
            if (Depth.Asks.Count == 0)
            {
                if (!(price != 0 || count != 0 || quantity != 0))
                {
                    result = true;
                }
            }
            else
            {
                result =  VerifyDepth(Depth.Asks.ElementAt(AskIndex).Value, price, count, quantity);

            }
            ++AskIndex;
            return result;
        }
        
        public bool VerifyBidsDone()
        {
            foreach (var kvp in Depth.Bids)
            {
                if (kvp.Value.OrderCount != 0) return false;
            }

            return true;
        }
        
        public bool VerifyAsksDone()
        {
            foreach (var kvp in Depth.Asks)
            {
                if (kvp.Value.OrderCount != 0) return false;
            }

            return true;
        }

        public void Reset()
        {
            AskIndex = 0;
            BidIndex = 0;
        }
    }
}