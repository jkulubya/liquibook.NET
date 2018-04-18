using System.Linq;
using Liquibook.NET.Book;
using Liquibook.NET.Simple;
using Liquibook.NET.Types;
using Xunit;
using static Test.Utils;

namespace Test
{
    public class AllOrNoneTests
    {
        public OrderConditions AON = OrderConditions.AllOrNone;
        public OrderConditions NoConditions = OrderConditions.NoConditions;
        private static readonly Quantity qty1 = 100;
        private static Quantity qty2 {get; set;} = qty1 + qty1;
        private static Quantity qty3 {get; set;} = qty2 + qty1;
        private static Quantity qty4 {get; set;} = qty2 + qty2;
        private static Quantity qty6 {get; set;} = qty2 + qty4;
        private readonly Quantity qty7 = 700; // derive this?
        private readonly Quantity qtyNone = 0;

        private readonly Price prc0 = 1250;
        private readonly Price prc1 = 1251;
        private readonly Price prc2 = 1252;
        private readonly Price prc3 = 1253;
        private readonly Price prcNone = 0;
        private readonly Price MARKET_ORDER_PRICE = Constants.MarketOrderPrice;

        const bool buySide = true;
        const bool sellSide = false;
        const bool expectMatch = true;
        const bool expectNoMatch = false;
        const bool expectComplete = true;
        const bool expectNoComplete = false;
        
        [Fact]
        public void TestRegBidMatchAon()
        {
            var order_book = new SimpleOrderBook();
            var ask2 = new SimpleOrder(sellSide, prc2, qty1);
            var ask1 = new SimpleOrder(sellSide, prc1, qty1); // AON
            var ask0 = new SimpleOrder(sellSide, prc1, qty2); // AON, but skipped
            var bid1 = new SimpleOrder(buySide, prc1, qty1);
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask2, expectNoMatch));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(3, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 2, qty1 + qty2));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Match - complete
            {
                var fc1 = new FillChecker(bid1, qty1, prc1 * qty1);
                var fc2 = new FillChecker(ask1, qty1, prc1 * qty1);
                Assert.True(AddAndVerify(order_book, bid1, expectMatch, expectComplete));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty2));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(2, order_book.Asks.Count());
        }

        [Fact]
        public void TestRegBidMatchMulti()
        {
            var order_book = new SimpleOrderBook();
            var ask2 = new SimpleOrder(sellSide, prc1, qty7);
            var ask1 = new SimpleOrder(sellSide, prc1, qty1); // AON
            var ask0 = new SimpleOrder(sellSide, prc1, qty1); // AON
            var bid1 = new SimpleOrder(buySide, prc1, qty4);
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask2, expectNoMatch, expectNoComplete));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(3, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 3, qty7 + qty1 + qty1));

            // Match - complete
            {
                var fc0 = new FillChecker(bid1, qty4, prc1 * qty4);
                var fc1 = new FillChecker(ask0, qty1, prc1 * qty1);
                var fc2 = new FillChecker(ask1, qty1, prc1 * qty1);
                var fc3 = new FillChecker(ask2, qty2, prc1 * qty2);
                Assert.True(AddAndVerify(order_book, bid1, expectMatch, expectComplete));
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty4 + qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());
        }

        [Fact]
        public void TestAonBidNoMatch()
        {
            var order_book = new SimpleOrderBook();
            var ask1 = new SimpleOrder(sellSide, prc2, qty1); // no match, price
            var ask0 = new SimpleOrder(sellSide, prc1, qty1); 
            var bid1 = new SimpleOrder(buySide, prc1, qty3); // no match, AON
            var bid0 = new SimpleOrder(buySide, prc0, qty1); // no match, price

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(2, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Match - complete
            {
                var fc1 = new FillChecker(bid1, qtyNone, prcNone);
                var fc2 = new FillChecker(ask0, qtyNone, prcNone);
                Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc1, 1, qty3));
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Verify sizes
            Assert.Equal(2, order_book.Bids.Count());
            Assert.Equal(2, order_book.Asks.Count());
        }
    }
}