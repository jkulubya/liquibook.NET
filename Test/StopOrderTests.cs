using Liquibook.NET.Book;
using Liquibook.NET.Simple;
using Liquibook.NET.Types;
using Xunit;

namespace Test
{
    public class StopOrderTests
    {
        private const bool sideBuy = true;
        private const bool sideSell = false;
        private readonly Price prcMkt = 0;
        private readonly Price prc53 = 53;
        private readonly Price prc54 = 54;
        private readonly Price prc55 = 55;
        private readonly Price prc56 = 56;
        private readonly Price prc57 = 57;
        private readonly Quantity q100 = 100;
        private readonly Quantity q1000 = 1000;
        private readonly bool expectMatch = true;
        private readonly bool expectNoMatch = false;
        private readonly bool expectComplete = true;
        private readonly bool expectNoComplete = false;

        [Fact]
        public void TestStopOrdersOffMarketNoTrade()
        {
            var book = new SimpleOrderBook();
            var order0 = new SimpleOrder(sideBuy, prc55, q100);
            var order1 = new SimpleOrder(sideSell, prcMkt, q100);

            // Enter order to generate a trade establishing market price
            Assert.True(Utils.AddAndVerify(book, order0, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order1, expectMatch, expectComplete));

            Assert.Equal(prc55, book.MarketPrice);

            var order2 = new SimpleOrder(sideBuy, prcMkt, q100, prc56);
            var order3 = new SimpleOrder(sideSell, prcMkt, q100, prc54);
            Assert.True(Utils.AddAndVerify(book, order2, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order3, expectNoMatch));
  
            // Orders were accepted, but not traded
            
            Assert.Equal(OrderState.Accepted, order2.State);
            Assert.Equal(OrderState.Accepted, order3.State);
        }

        [Fact]
        public void TestStopMarketOrdersOnMarketTradeImmediately()
        {
            var book = new SimpleOrderBook();
            var order0 = new SimpleOrder(sideBuy, prc55, q100);
            var order1 = new SimpleOrder(sideSell, prcMkt, q100);
            
            Assert.True(Utils.AddAndVerify(book, order0, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order1, expectMatch, expectComplete));
            
            Assert.Equal(prc55, book.MarketPrice);
            
            var order2 = new SimpleOrder(sideBuy, prcMkt, q100, prc55);
            var order3 = new SimpleOrder(sideSell, prcMkt, q100, prc55);
            Assert.True(Utils.AddAndVerify(book, order2, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order3, expectMatch, expectComplete));
        }

        [Fact]
        public void TestStopMarketOrdersTradeWhenStopPriceReached()
        {
            var book = new SimpleOrderBook();
            var order0 = new SimpleOrder(sideBuy, prc53, q100);
            var order1 = new SimpleOrder(sideSell, prc57, q100);
            book.MarketPrice = prc55;
            
            // Enter seed orders and be sure they don't trade with each other.
            Assert.True(Utils.AddAndVerify(book, order0, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order1, expectNoMatch));
            
            // enter stop orders.  Be sure they don't trade yet
            var order2 = new SimpleOrder(sideBuy, prcMkt, q100, prc56);
            var order3 = new SimpleOrder(sideSell, prcMkt, q100, prc54);
            
            Assert.True(Utils.AddAndVerify(book, order2, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order3, expectNoMatch));
            
            var order4 = new SimpleOrder(sideBuy, prc56, q1000, 0, OrderConditions.AllOrNone);
            var order5 = new SimpleOrder(sideSell, prc56, q1000, 0, OrderConditions.AllOrNone);

            {
                var fc0 = new FillChecker(order0, 0, 0);
                var fc1 = new FillChecker(order1, q100, q100 * prc57);
                var fc2 = new FillChecker(order2, q100, q100 * prc57);
                Assert.True(Utils.AddAndVerify(book, order4, expectNoMatch, expectNoComplete, OrderConditions.AllOrNone));
                Assert.True(Utils.AddAndVerify(book, order5, expectMatch, expectComplete, OrderConditions.AllOrNone));
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
            }
            
            Assert.Equal(prc57, book.MarketPrice);
            
            var order6 = new SimpleOrder(sideBuy, prc54, q1000, 0, OrderConditions.AllOrNone);
            var order7 = new SimpleOrder(sideSell, prc54, q1000, 0, OrderConditions.AllOrNone);

            {
                var fc0 = new FillChecker(order0, q100, q100 * prc53);
                var fc3 = new FillChecker(order3, q100, q100 * prc53);
                // Trade at 54 which should trigger order3 which should trade with order 0 at order 0's price
                Assert.True(Utils.AddAndVerify(book, order6, expectNoMatch, expectNoComplete, OrderConditions.AllOrNone));
                Assert.True(Utils.AddAndVerify(book, order7, expectMatch, expectComplete, OrderConditions.AllOrNone));
            }
            Assert.Equal(prc53, book.MarketPrice);
        }
        
    }
}