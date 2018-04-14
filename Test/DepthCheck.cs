using System;
using Liquibook.NET.Book;
using Liquibook.NET.Types;

namespace Test
{
    public class DepthCheck
    {
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
            throw new NotImplementedException();
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

        private void Reset()
        {
            
        }
    }
}