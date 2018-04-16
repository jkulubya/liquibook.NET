using Liquibook.NET.Simple;
using Liquibook.NET.Types;
using Xunit;

namespace Test
{
    public class MarketPriceTests
    {
        private const bool sideBuy = true;
        private const bool sideSell = false;
        private readonly Price prcMkt = 0;
        private readonly Price prc0 = 9900;
        private readonly Quantity q100 = 100;
        private const bool ExpectMatch = true;
        private const bool ExpectNoMatch = false;
        private const bool ExpectComplete = true;
        private const bool ExpectNoComplete = false;

        [Fact]
        public void TestNoMktToMktWithoutPreviousTrade()
        {
            var book = new SimpleOrderBook();
            var order0 = new SimpleOrder(sideBuy, prcMkt, q100);
            var order1 = new SimpleOrder(sideSell, prcMkt, q100);
            
            Assert.True(Utils.AddAndVerify(book, order0, ExpectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order1, ExpectNoMatch));
            
            var fc0 = new FillChecker(order0, 0, 0);
            var fc1 = new FillChecker(order1, 0, 0);

            Assert.Equal((Price) 0, book.MarketPrice);
        }

        [Fact]
        public void TestTradeSetsMarketPrice()
        {
            var book = new SimpleOrderBook();
            var order0 = new SimpleOrder(sideBuy, prcMkt, q100);
            var order1 = new SimpleOrder(sideSell, prcMkt, q100);
            
            Assert.True(Utils.AddAndVerify(book, order0, ExpectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order1, ExpectNoMatch));
            
            Assert.Equal((Price) 0, book.MarketPrice);
            
            var order2 = new SimpleOrder(sideBuy, prc0, q100);
            {
                var fc0 = new FillChecker(order0, 0, 0);
                var fc1 = new FillChecker(order1, q100, q100 * prc0);
                var fc2 = new FillChecker(order2, q100, q100 * prc0);
                Assert.True(Utils.AddAndVerify(book, order2, ExpectMatch, ExpectComplete));
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
            }
            Assert.Equal(prc0, book.MarketPrice);
            
            var order3 = new SimpleOrder(sideSell, prcMkt, q100);
            {
                var fc0 = new FillChecker(order0, q100, q100 * prc0);
                var fc3 = new FillChecker(order3, q100, q100 * prc0);
                Assert.True(Utils.AddAndVerify(book, order3, ExpectMatch, ExpectComplete));
                fc0.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }
            Assert.Equal(prc0, book.MarketPrice);
        }

        [Fact]
        public void TestExplicitlySettingMarketPriceAllowsMarketToMarketTrades()
        {
            var book = new SimpleOrderBook();
            var order0 = new SimpleOrder(sideBuy, prcMkt, q100);
            var order1 = new SimpleOrder(sideSell, prcMkt, q100);
            
            Assert.True(Utils.AddAndVerify(book, order0, ExpectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order1, ExpectNoMatch));

            {
                var fc0 = new FillChecker(order0, 0, 0);
                var fc1 = new FillChecker(order1, 0, 0);
                book.MarketPrice = prc0;
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
            }
            
            var order2 = new SimpleOrder(sideBuy, prcMkt, q100);
            var order3 = new SimpleOrder(sideSell, prcMkt, q100);

            {
                var fc0 = new FillChecker(order0, q100, q100 * prc0);
                var fc1 = new FillChecker(order1, q100, q100 * prc0);
                var fc2 = new FillChecker(order2, q100, q100 * prc0);
                var fc3 = new FillChecker(order3, q100, q100 * prc0);
                Assert.True(Utils.AddAndVerify(book, order2, ExpectMatch, ExpectComplete));
                Assert.True(Utils.AddAndVerify(book, order3, ExpectMatch, ExpectComplete));
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }
        }
    }
}