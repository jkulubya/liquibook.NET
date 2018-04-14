using System.Linq;
using Liquibook.NET.Book;
using Liquibook.NET.Types;
using Xunit;

namespace Test
{
    public class DepthTests
    {
        private static bool VerifyLevel(DepthLevel level, Price price, int orderCount, Quantity aggregateQuantity)
        {
            var matched = true;
            if (level.Price != price) matched = false;
            if (level.OrderCount != orderCount) matched = false;
            if (level.AggregateQty != aggregateQuantity) matched = false;
            return matched;
        }
        
        [Fact]
        public void TestAddAsk()
        {
            var depth = new Depth();
            var cc = new ChangedChecker(depth);
            depth.AddOrder(1234, 100, false);
            var firstAsk = depth.Asks.First().Value;
            Assert.True(VerifyLevel(firstAsk, 1234, 1, 100));
            Assert.True(cc.VerifyAskChanged(true, false, false, false, false));
        }

        [Fact]
        public void TestAddAsks()
        {
            var depth = new Depth();
            var cc = new ChangedChecker(depth);
            depth.AddOrder(1234, 100, false);
            Assert.True(cc.VerifyAskChanged(true, false, false, false, false));
            cc.Reset();
            
            depth.AddOrder(1234, 200, false);
            Assert.True(cc.VerifyAskChanged(true, false, false, false, false));
            cc.Reset();
            
            depth.AddOrder(1234, 300, false);
            Assert.True(cc.VerifyAskChanged(true, false, false, false, false));

            var firstAsk = depth.Asks.First().Value;
            Assert.True(VerifyLevel(firstAsk, 1234, 3, 600));
        }

        [Fact]
        public void TestAppendAskLevels()
        {
            var depth = new Depth();
            var cc = new ChangedChecker(depth);
            depth.AddOrder(1236, 300, false);
            Assert.True(cc.VerifyAskChanged(true, false, false, false, false));
            cc.Reset();
            
            depth.AddOrder(1235, 200, false);
            Assert.True(cc.VerifyAskChanged(true, true, false, false, false));
            cc.Reset();
            
            depth.AddOrder(1232, 100, false);
            Assert.True(cc.VerifyAskChanged(true, true, true, false, false));
            cc.Reset();
            
            depth.AddOrder(1235, 400, false);
            Assert.True(cc.VerifyAskChanged(false, true, false, false, false));
            cc.Reset();

            var ask1 = depth.Asks.First().Value;
            var ask2 = depth.Asks.ElementAt(1).Value;
            var ask3 = depth.Asks.ElementAt(2).Value;
            
            Assert.True(VerifyLevel(ask1, 1232, 1, 100));
            Assert.True(VerifyLevel(ask2, 1235, 2, 600));
            Assert.True(VerifyLevel(ask3, 1236, 1, 300));
        }
    }
}