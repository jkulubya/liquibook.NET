using System;
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
        private static Quantity qty2 { get; } = qty1 + qty1;
        private static Quantity qty3 { get; } = qty2 + qty1;
        private static Quantity qty4 { get; } = qty2 + qty2;
        private static Quantity qty6 { get; } = qty2 + qty4;
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

        [Fact]
        public void TestAonBidMatchReg()
        {
            var order_book = new SimpleOrderBook();
            var ask1 = new SimpleOrder(sellSide, prc2, qty1);
            var ask0 = new SimpleOrder(sellSide, prc1, qty4);
            var bid1 = new SimpleOrder(buySide, prc1, qty3); // AON
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

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
            Assert.True(dc.VerifyAsk(prc1, 1, qty4));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Match - complete
            {
                var fc1 = new FillChecker(bid1, qty3, prc1 * qty3);
                var fc2 = new FillChecker(ask0, qty3, prc1 * qty3);
                Assert.True(AddAndVerify(order_book, bid1, expectMatch, expectComplete, AON));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(2, order_book.Asks.Count());
        }

        [Fact]
        public void TestAonBidMatchMulti()
        {
            var order_book = new SimpleOrderBook();
            var ask3 = new SimpleOrder(sellSide, prc2, qty1);
            var ask2 = new SimpleOrder(sellSide, prc2, qty1);
            var ask1 = new SimpleOrder(sellSide, prc1, qty4); // AON no match
            var ask0 = new SimpleOrder(sellSide, prc1, qty4);
            var bid1 = new SimpleOrder(buySide, MARKET_ORDER_PRICE, qty6); // AON
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask2, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask3, expectNoMatch));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(4, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 2, qty4 + qty4));
            Assert.True(dc.VerifyAsk(prc2, 2, qty1 + qty1));

            // Match - complete
            {
                //ASSERT_NO_THROW(
                var fc1 = new FillChecker(bid1, qty6, prc1 * qty2 + prc1 * qty4);
                var fc2 = new FillChecker(ask0, qty2, prc1 * qty2);
                var fc3 = new FillChecker(ask1, qty4, prc1 * qty4);
                var fc4 = new FillChecker(ask2, 0, prc2 * (Quantity) 0);
                var fc5 = new FillChecker(ask3, 0, prc2 * (Quantity) 0);
                Assert.True(AddAndVerify(order_book, bid1, expectMatch, expectComplete, AON));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
                fc4.AssertFillSuccess();
                fc5.AssertFillSuccess();
                //); 
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty2));
            Assert.True(dc.VerifyAsk(prc2, 2, qty1 + qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(3, order_book.Asks.Count());
        }

        [Fact]
        public void TestAonBidNoMatchMulti()
        {
            var order_book = new SimpleOrderBook();
            var ask2 = new SimpleOrder(sellSide, prc2, qty4); // AON no match
            var ask1 = new SimpleOrder(sellSide, prc2, qty1);
            var ask0 = new SimpleOrder(sellSide, prc1, qty4);
            var bid1 = new SimpleOrder(buySide, MARKET_ORDER_PRICE, qty6); // AON
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask2, expectNoMatch, expectNoComplete, AON));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(3, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty4));
            Assert.True(dc.VerifyAsk(prc2, 2, qty4 + qty1));

            // Match - complete
            {
                //ASSERT_NO_THROW(
                var fc0 = new FillChecker(bid0, qtyNone, prcNone);
                var fc1 = new FillChecker(bid1, qty6, qty2 * prc1 + qty4 * prc2); // filled 600 @ 751000
                var fc2 = new FillChecker(ask0, qty2, qty2 * prc1); // filled 200 @ 250200
                var fc3 = new FillChecker(ask1, qtyNone, prcNone); // 0
                var fc4 = new FillChecker(ask2, qty4, qty4 * prc2); // filled 400 @ 500800
                Assert.True(AddAndVerify(order_book, bid1, expectMatch, expectComplete, AON));
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
                fc4.AssertFillSuccess();
                //); 
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty2));
        }

        [Fact]
        public void TestAonBidMatchAon()
        {
            var order_book = new SimpleOrderBook();
            var ask1 = new SimpleOrder(sellSide, prc2, qty1);
            var ask0 = new SimpleOrder(sellSide, prc1, qty3); // AON
            var bid1 = new SimpleOrder(buySide, prc1, qty3); // AON
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(2, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty3));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Match - complete
            {
                var fc1 = new FillChecker(bid1, qty3, prc1 * qty3);
                var fc2 = new FillChecker(ask0, qty3, prc1 * qty3);
                Assert.True(AddAndVerify(order_book, bid1, expectMatch, expectComplete, AON));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());
        }

        [Fact]
        public void TestRegAskMatchAon()
        {
            var order_book = new SimpleOrderBook();
            var ask0 = new SimpleOrder(sellSide, prc2, qty1);
            var ask1 = new SimpleOrder(sellSide, prc1, qty1);
            var bid1 = new SimpleOrder(buySide, prc1, qty2); // AON, but skipped
            var bid2 = new SimpleOrder(buySide, prc1, qty1); // AON
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, bid2, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));

            // Verify sizes
            Assert.Equal(3, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 2, qty3));
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Match - complete
            {
                var fc1 = new FillChecker(bid2, qty1, prc1 * qty1);
                var fc2 = new FillChecker(ask1, qty1, prc1 * qty1);
                Assert.True(AddAndVerify(order_book, ask1, expectMatch, expectComplete));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc1, 1, qty2));
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Verify sizes
            Assert.Equal(2, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());
        }

        [Fact]
        public void TestRegAskMatchMulti()
        {
            var order_book = new SimpleOrderBook();
            var ask0 = new SimpleOrder(sellSide, prc2, qty1);
            var ask1 = new SimpleOrder(sellSide, prc0, qty4);
            var bid1 = new SimpleOrder(buySide, prc1, qty1); // AON
            var bid2 = new SimpleOrder(buySide, prc1, qty1); // AON
            var bid0 = new SimpleOrder(buySide, prc0, qty7);

            // Calculate some expected results
            // ask1(400) matches bid 1(100), bid2(100), and part(200) of bid0 
            // leaving 500 shares of bid 0)
            Quantity bid0FillQty = qty4 - qty1 - qty1;
            Quantity bid0RemainQty = qty7 - bid0FillQty;
            int bid0FillAmount = bid0FillQty * prc0;
            int bid1FillAmount = prc1 * qty1;
            int bid2FillAmount = prc1 * qty1;
            int ask1FillAmount = bid1FillAmount + bid2FillAmount + bid0FillAmount;

            // No match
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, bid2, expectNoMatch, expectNoComplete, AON));

            // Verify sizes
            Assert.Equal(3, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 2, qty2));
            Assert.True(dc.VerifyBid(prc0, 1, qty7));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Match - complete
            {
                var fc0 = new FillChecker(bid1, qty1, prc1 * qty1);
                var fc1 = new FillChecker(bid2, qty1, prc1 * qty1);
                var fc2 = new FillChecker(bid0, qty2, prc0 * qty2);
                var fc3 = new FillChecker(ask1, qty4, ask1FillAmount);
                Assert.True(AddAndVerify(order_book, ask1, expectMatch, expectComplete));
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, bid0RemainQty));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());
        }

        [Fact]
        public void TestAonAskNoMatch()
        {
            var order_book = new SimpleOrderBook();
            var ask0 = new SimpleOrder(sellSide, prc2, qty1);
            var ask1 = new SimpleOrder(sellSide, prc1, qty4); // AON
            var bid1 = new SimpleOrder(buySide, prc1, qty1);
            var bid2 = new SimpleOrder(buySide, prc1, qty1);
            var bid0 = new SimpleOrder(buySide, prc0, qty7);

            // No match
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid2, expectNoMatch));

            // Verify sizes
            Assert.Equal(3, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 2, qty2));
            Assert.True(dc.VerifyBid(prc0, 1, qty7));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Match - complete
            {
                var fc0 = new FillChecker(bid1, qtyNone, prcNone);
                var fc1 = new FillChecker(bid2, qtyNone, prcNone);
                var fc2 = new FillChecker(bid0, qtyNone, prcNone);
                var fc3 = new FillChecker(ask1, qtyNone, prcNone);
                Assert.True(AddAndVerify(order_book, ask1, expectNoMatch, expectNoComplete, AON));
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc1, 2, qty2));
            Assert.True(dc.VerifyBid(prc0, 1, qty7));
            Assert.True(dc.VerifyAsk(prc1, 1, qty4));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Verify sizes
            Assert.Equal(3, order_book.Bids.Count());
            Assert.Equal(2, order_book.Asks.Count());
        }

        [Fact]
        public void TestAonAskMatchReg()
        {
            var order_book = new SimpleOrderBook();
            var ask0 = new SimpleOrder(sellSide, prc2, qty1);
            var ask1 = new SimpleOrder(sellSide, prc1, qty1); // AON
            var bid1 = new SimpleOrder(buySide, prc1, qty1);
            var bid0 = new SimpleOrder(buySide, prc0, qty7);

            // No match
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch));

            // Verify sizes
            Assert.Equal(2, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 1, qty1));
            Assert.True(dc.VerifyBid(prc0, 1, qty7));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Match - complete
            {
                var fc0 = new FillChecker(bid1, qty1, prc1 * qty1);
                var fc3 = new FillChecker(ask1, qty1, prc1 * qty1);
                Assert.True(AddAndVerify(order_book, ask1, expectMatch, expectComplete, AON));
                fc0.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty7));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());
        }

        [Fact]
        public void TestAonAskMatchMulti()
        {
            var order_book = new SimpleOrderBook();
            var ask0 = new SimpleOrder(sellSide, prc2, qty1); // no match due to price
            var ask1 = new SimpleOrder(sellSide, prc0, qty6); // AON
            var bid1 = new SimpleOrder(buySide, prc1, qty1); // AON
            var bid2 = new SimpleOrder(buySide, prc1, qty1);
            var bid3 = new SimpleOrder(buySide, prc1, qty1);
            var bid0 = new SimpleOrder(buySide, prc0, qty7);

            // No match
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, bid2, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid3, expectNoMatch));

            // Verify sizes
            Assert.Equal(4, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 3, qty3));
            Assert.True(dc.VerifyBid(prc0, 1, qty7));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Match - complete
            {
                // ASSERT_NO_THROW(

                int b1Cost = prc1 * qty1;
                var fc0 = new FillChecker(bid1, qty1, b1Cost);
                int b2Cost = prc1 * qty1;
                var fc1 = new FillChecker(bid2, qty1, b2Cost);
                int b3Cost = prc1 * qty1;
                var fc2 = new FillChecker(bid3, qty1, b3Cost);
                Quantity b0Fill = qty6 - qty1 - qty1 - qty1;
                int b0Cost = b0Fill * prc0;
                var fc3 = new FillChecker(bid0, b0Fill, b0Cost);
                int a1Cost = b0Cost + b1Cost + b2Cost + b3Cost;
                var fc4 = new FillChecker(ask1, qty6, a1Cost);
                Assert.True(AddAndVerify(order_book, ask1, expectMatch, expectComplete, AON));
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
                fc4.AssertFillSuccess();
                // ); 
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty4));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());
        }
///////////////////

        [Fact]
        public void TestOneAonBidOneAonAsk()
        {
            var order_book = new SimpleOrderBook();
            var bid1 = new SimpleOrder(buySide, prc1, qty1); // AON
            var ask1 = new SimpleOrder(sellSide, prc1, qty1); // AON

            // Prime the order book: No Matches
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(0, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 1, qty1));

            // Add matching order
            {
                var fc1 = new FillChecker(bid1, qty1, qty1 * prc1);
                var fc3 = new FillChecker(ask1, qty1, qty1 * prc1);
                Assert.True(AddAndVerify(order_book, ask1, expectMatch, expectComplete, AON));
                fc1.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }

            // Verify sizes
            Assert.Equal(0, order_book.Bids.Count());
            Assert.Equal(0, order_book.Asks.Count());
        }

        [Fact]
        public void TestTwoAonBidOneAonAsk()
        {
            var order_book = new SimpleOrderBook();
            var bid1 = new SimpleOrder(buySide, prc1, qty1); // AON
            var bid2 = new SimpleOrder(buySide, prc1, qty2); // AON
            var ask1 = new SimpleOrder(sellSide, prc1, qty3); // AON

            // Prime the order book: No Matches
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete,
                AON)); //AON)); //noConditions
            Assert.True(AddAndVerify(order_book, bid2, expectNoMatch, expectNoComplete, AON));

            // Verify sizes
            Assert.Equal(2, order_book.Bids.Count());
            Assert.Equal(0, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 2, qty1 + qty2));

            // Add matching order
            {
                var fc1 = new FillChecker(bid1, qty1, qty1 * prc1);
                var fc2 = new FillChecker(bid2, qty2, qty2 * prc1);
                var fc3 = new FillChecker(ask1, qty3, qty3 * prc1);
                Assert.True(AddAndVerify(order_book, ask1, expectMatch, expectComplete, AON));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }

            // Verify sizes
            Assert.Equal(0, order_book.Bids.Count());
            Assert.Equal(0, order_book.Asks.Count());

        }

        [Fact]
        public void TestOneAonBidTwoAsk()
        {
            var order_book = new SimpleOrderBook();

            var bid1 = new SimpleOrder(buySide, prc1, qty3); // AON
            var ask1 = new SimpleOrder(sellSide, prc1, qty1); // No Conditions
            var ask2 = new SimpleOrder(sellSide, prc1, qty2); // No Conditions

            // Prime the order book: No Matches
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete,
                AON)); //AON)); //noConditions

            // Add an order that does NOT meet the AON condition
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch, expectNoComplete, NoConditions));
            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 1, qty3));
            Assert.True(dc.VerifyAsk(prc1, 1, qty1));

            // Add matching order
            {
                var fc1 = new FillChecker(bid1, qty3, qty3 * prc1);
                var fc2 = new FillChecker(ask1, qty1, qty1 * prc1);
                var fc3 = new FillChecker(ask2, qty2, qty2 * prc1);
                Assert.True(AddAndVerify(order_book, ask2, expectMatch, expectComplete, NoConditions));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }

            // Verify sizes
            Assert.Equal(0, order_book.Bids.Count());
            Assert.Equal(0, order_book.Asks.Count());
        }

        [Fact]
        public void TestOneBidTwoAonAsk()
        {
            var order_book = new SimpleOrderBook();

            var bid1 = new SimpleOrder(buySide, prc1, qty3); // noConditions
            var ask1 = new SimpleOrder(sellSide, prc1, qty1); // AON 
            var ask2 = new SimpleOrder(sellSide, prc1, qty2); // AON

            // Prime the order book: No Matches
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(0, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 1, qty3));

            // Add matching order
            {
                var fc1 = new FillChecker(bid1, qty3, qty3 * prc1);
                var fc2 = new FillChecker(ask1, qty1, qty1 * prc1);
                var fc3 = new FillChecker(ask2, qty2, qty2 * prc1);
                Assert.True(AddAndVerify(order_book, ask1, expectNoMatch, expectNoComplete, NoConditions));
                Assert.True(AddAndVerify(order_book, ask2, expectMatch, expectComplete, NoConditions));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }

            // Verify sizes
            Assert.Equal(0, order_book.Bids.Count());
            Assert.Equal(0, order_book.Asks.Count());
        }

        [Fact]
        public void TestTwoAonBidTwoAonAsk()
        {
#if true
            //int todo_FixTestAonsTwoBidTwoAsk;
            //std::cout << "***** WARNING TEST " << "TestAonsTwoBidTwoAsk" << " is disabled" << std::endl;
            Console.WriteLine("***** WARNING TEST TestAonsTwoBidTwoAsk is disabled");
#else
// The current match algorithm tries to match one order from one side of the market to "N" orders
// from the other side.   This test won't pass because it requires two orders from each side.
// I'm leaving the test here as a challenge to future developers who want to improve the matching
// algorithm (good luck)

  var order_book = new SimpleOrderBook();

  SimpleOrder ask1(sellSide,prc1,qty3); // AON
  SimpleOrder ask2(sellSide,prc1,qty2); // AON

  SimpleOrder bid1(buySide,prc1,qty1); // AON
  SimpleOrder bid2(buySide,prc1,qty4); // AON

                                        // Prime the order book: No Matches
  Assert.True(AddAndVerify(order_book,bid1,expectNoMatch,expectNoComplete,AON));
  Assert.True(AddAndVerify(order_book,bid2,expectNoMatch,expectNoComplete,AON));
  Assert.True(AddAndVerify(order_book,ask1,expectNoMatch,expectNoComplete,AON));

  // Verify sizes
  Assert.Equal(2u,order_book.Bids.Count());
  Assert.Equal(1u,order_book.Asks.Count());

  // Verify depth
  var dc = new DepthCheck(order_book.Depth);
  Assert.True(dc.VerifyBid(prc1,2,qty1 + qty4));
  Assert.True(dc.VerifyAsk(prc1,1,qty3));

  // Add matching order
  {
      SimpleFillCheck fc1(bid1,qty1,qty3 * prc1);
  SimpleFillCheck fc2(bid2,qty4,qty3 * prc1);
  SimpleFillCheck fc3(ask1,qty3,qty1 * prc1);
  SimpleFillCheck fc4(ask2,qty2,qty2 * prc1);
  Assert.True(AddAndVerify(order_book,ask2,expectMatch,expectComplete,AON));
  }

  // Verify sizes
  Assert.Equal(0,order_book.Bids.Count());
  Assert.Equal(0,order_book.Asks.Count());
#endif
        }

        [Fact]
        public void TestAonAskNoMatchMulti()
        {
            var order_book = new SimpleOrderBook();
            var ask0 = new SimpleOrder(sellSide, prc2, qty1); // no match (price)
            var ask1 = new SimpleOrder(sellSide, prc0, qty6); // AON

            var bid0 = new SimpleOrder(buySide, prc0, qty4); // AON no match
            var bid1 = new SimpleOrder(buySide, prc1, qty1); // AON
            var bid2 = new SimpleOrder(buySide, prc1, qty4);

            // No match
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON)); //AON)); //noConditions
            Assert.True(AddAndVerify(order_book, bid2, expectNoMatch));

            // Verify sizes
            Assert.Equal(3, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 2, qty1 + qty4));
            Assert.True(dc.VerifyBid(prc0, 1, qty4));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));

            // This test was bogus -- testing a bug in the matching algorithm
            // I fixed the bug and the test started to fail.
            // So fixed the test to expect:
            // Ask1 (600 AON) should match bid0 (400 AON) + bid1(100) + bid 2(100 of 400)
            //
            // Now we need a new test of an AON that should NOT match!

            // No match
            {
//  ASSERT_NO_THROW(
                var fc0 = new FillChecker(bid0, qty4, prc0 * qty4);
                var fc1 = new FillChecker(bid1, qty1, qty1 * prc1);
                var fc2 = new FillChecker(bid2, qty1, prc1 * qty1);
                var fc3 = new FillChecker(ask1, qty6, prc0 * qty4 + qty1 * prc1 + prc1 * qty1);
                Assert.True(AddAndVerify(order_book, ask1, expectMatch, expectComplete, AON));
                fc0.AssertFillSuccess();
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
                fc3.AssertFillSuccess();
                //); 
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc1, 1, qty4 - qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());
        }

        [Fact]
        public void TestAonAskMatchAon()
        {
            var order_book = new SimpleOrderBook();
            var ask0 = new SimpleOrder(sellSide, prc2, qty1);
            var ask1 = new SimpleOrder(sellSide, prc1, qty2); // AON
            var bid1 = new SimpleOrder(buySide, prc1, qty2); // AON
            var bid0 = new SimpleOrder(buySide, prc0, qty4);

            // No match
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON));

            // Verify sizes
            Assert.Equal(2, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));
            Assert.True(dc.VerifyBid(prc1, 1, qty2));
            Assert.True(dc.VerifyBid(prc0, 1, qty4));

            // Match complete
            {
                var fc1 = new FillChecker(bid1, qty2, prc1 * qty2);
                var fc3 = new FillChecker(ask1, qty2, prc1 * qty2);
                Assert.True(AddAndVerify(order_book, ask1, expectMatch, expectComplete, AON));
                fc1.AssertFillSuccess();
                fc3.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));
            Assert.True(dc.VerifyBid(prc0, 1, qty4));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());
        }

        [Fact]
        public void TestReplaceAonBidSmallerMatch()
        {
            var order_book = new SimpleOrderBook();
            var ask2 = new SimpleOrder(sellSide, prc3, qty1);
            var ask1 = new SimpleOrder(sellSide, prc2, qty1);
            var ask0 = new SimpleOrder(sellSide, prc1, qty1);
            var bid1 = new SimpleOrder(buySide, prc1, qty2); // AON
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask2, expectNoMatch));

            // Verify sizes
            Assert.Equal(2, order_book.Bids.Count());
            Assert.Equal(3, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 1, qty2));
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));
            Assert.True(dc.VerifyAsk(prc3, 1, qty1));

            // Match - complete
            {
                var fc2 = new FillChecker(ask0, qty1, prc1 * qty1);
                Assert.True(ReplaceAndVerify(
                    order_book, bid1, -qty1, Constants.PriceUnchanged, OrderState.Complete, qty1));
                fc2.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));
            Assert.True(dc.VerifyAsk(prc3, 1, qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(2, order_book.Asks.Count());
        }

        [Fact]
        public void TestReplaceAonBidPriceMatch()
        {
            var order_book = new SimpleOrderBook();
            var ask2 = new SimpleOrder(sellSide, prc3, qty1);
            var ask1 = new SimpleOrder(sellSide, prc2, qty1);
            var ask0 = new SimpleOrder(sellSide, prc1, qty1);
            var bid1 = new SimpleOrder(buySide, prc1, qty2); // AON
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask2, expectNoMatch));

            // Verify sizes
            Assert.Equal(2, order_book.Bids.Count());
            Assert.Equal(3, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 1, qty2));
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));
            Assert.True(dc.VerifyAsk(prc3, 1, qty1));

            // Match - complete
            {
                var fc1 = new FillChecker(ask0, qty1, prc1 * qty1);
                var fc2 = new FillChecker(ask1, qty1, prc2 * qty1);
                Assert.True(ReplaceAndVerify(
                    order_book, bid1, qtyNone, prc2, OrderState.Complete, qty2));
                fc1.AssertFillSuccess();
                fc2.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc3, 1, qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(1, order_book.Asks.Count());
        }

        [Fact]
        public void TestReplaceBidLargerMatchAon()
        {
            var order_book = new SimpleOrderBook();
            var ask2 = new SimpleOrder(sellSide, prc3, qty1);
            var ask1 = new SimpleOrder(sellSide, prc2, qty1);
            var ask0 = new SimpleOrder(sellSide, prc1, qty2); // AON
            var bid1 = new SimpleOrder(buySide, prc1, qty1);
            var bid0 = new SimpleOrder(buySide, prc0, qty1);

            // No match
            Assert.True(AddAndVerify(order_book, bid0, expectNoMatch));
            Assert.True(AddAndVerify(order_book, bid1, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask0, expectNoMatch, expectNoComplete, AON));
            Assert.True(AddAndVerify(order_book, ask1, expectNoMatch));
            Assert.True(AddAndVerify(order_book, ask2, expectNoMatch));

            // Verify sizes
            Assert.Equal(2, order_book.Bids.Count());
            Assert.Equal(3, order_book.Asks.Count());

            // Verify depth
            var dc = new DepthCheck(order_book.Depth);
            Assert.True(dc.VerifyBid(prc1, 1, qty1));
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc1, 1, qty2));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));
            Assert.True(dc.VerifyAsk(prc3, 1, qty1));

            // Match - complete
            {
                var fc2 = new FillChecker(ask0, qty2, qty2 * prc1);
                Assert.True(ReplaceAndVerify(
                    order_book, bid1, qty1, Constants.PriceUnchanged, OrderState.Complete, qty2));
                fc2.AssertFillSuccess();
            }

            // Verify depth
            dc.Reset();
            Assert.True(dc.VerifyBid(prc0, 1, qty1));
            Assert.True(dc.VerifyAsk(prc2, 1, qty1));
            Assert.True(dc.VerifyAsk(prc3, 1, qty1));

            // Verify sizes
            Assert.Equal(1, order_book.Bids.Count());
            Assert.Equal(2, order_book.Asks.Count());
        }
    }
}