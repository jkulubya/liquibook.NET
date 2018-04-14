using System.Collections.Generic;
using System.Linq;
using Liquibook.NET.Book;
using Liquibook.NET.Types;

namespace Test
{
    public class ChangedChecker
    {
        private Depth Depth { get; set; }
        private int LastChange { get; set; }
        public ChangedChecker(Depth depth)
        {
            Depth = depth;
            Reset();
        }

        public void Reset()
        {
            LastChange = Depth.LastChange;
        }

        public bool VerifyBidChanged(bool l0, bool l1, bool l2, bool l3, bool l4)
        {
            return VerifySideChanged(Depth.Bids, l0, l1, l2, l3, l4);
        }
        
        public bool VerifyAskChanged(bool l0, bool l1, bool l2, bool l3, bool l4)
        {
            return VerifySideChanged(Depth.Asks, l0, l1, l2, l3, l4);
        }

        public bool VerifyBidStamps(int l0, int l1, int l2, int l3, int l4)
        {
            return VerifySideStamps(Depth.Bids, l0, l1, l2, l3, l4);
        }
        
        public bool VerifyAskStamps(int l0, int l1, int l2, int l3, int l4)
        {
            return VerifySideStamps(Depth.Asks, l0, l1, l2, l3, l4);
        }

        public bool VerifyBboChanged(bool bidChanged, bool askChanged)
        {
            var matched = true;
            if (Depth.Bids.First().Value.ChangedSince(LastChange))
            {
                if (!bidChanged) matched = false;
            }
            else if (bidChanged) matched = false;

            if (Depth.Asks.First().Value.ChangedSince(LastChange))
            {
                if (!askChanged) matched = false;
            }
            else if (askChanged) matched = false;

            return matched;
        }

        public bool VerifyBboStamps(int bidStamp, int askStamp)
        {
            var matched = true;
            if (Depth.Bids.First().Value.LastChange != bidStamp) matched = false;
            if (Depth.Asks.First().Value.LastChange != askStamp) matched = false;
            return matched;
        }

        private bool VerifySideStamps(SortedDictionary<Price, DepthLevel> depthLevels, int l0, int l1, int l2, int l3,
            int l4)
        {
            var matched = true;
            if (depthLevels.ElementAt(0).Value.LastChange != l0)
            {
                matched = false;
            }
            if (depthLevels.ElementAt(1).Value.LastChange != l1)
            {
                matched = false;
            }
            if (depthLevels.ElementAt(2).Value.LastChange != l2)
            {
                matched = false;
            }
            if (depthLevels.ElementAt(3).Value.LastChange != l3)
            {
                matched = false;
            }
            if (depthLevels.ElementAt(4).Value.LastChange != l4)
            {
                matched = false;
            }

            return matched;
        }

        private bool VerifyLevel(SortedDictionary<Price, DepthLevel> depthLevels, int index, bool changeExpected)
        {
            var matched = depthLevels.ElementAt(index).Value.ChangedSince(LastChange) == changeExpected;
            return matched;
        }

        private bool VerifySideChanged(SortedDictionary<Price, DepthLevel> depthLevels, bool l0, bool l1, bool l2,
            bool l3, bool l4)
        {
            var matched = true;
            var maxIndex = depthLevels.Count - 1;
            for (var i = 0; i <= maxIndex; i++)
            {
                switch (i)
                {
                    case 0:
                        matched = VerifyLevel(depthLevels, i, l0);
                        break;
                    case 1:
                        matched = VerifyLevel(depthLevels, i, l1);
                        break;
                    case 2:
                        matched = VerifyLevel(depthLevels, i, l2);
                        break;
                    case 3:
                        matched = VerifyLevel(depthLevels, i, l3);
                        break;
                    case 4:
                        matched = VerifyLevel(depthLevels, i, l4);
                        break;

                }
            }
            return matched;
        }
        
    }
}