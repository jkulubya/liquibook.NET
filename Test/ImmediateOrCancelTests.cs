using System.Linq;
using Liquibook.NET.Book;
using Liquibook.NET.Simple;
using Xunit;
using static Test.Utils;

namespace Test
{
  public class ImmediateOrCancelTests
  {
    private OrderConditions IOC { get; } = OrderConditions.ImmediateOrCancel;
    private OrderConditions FOK { get; } = OrderConditions.FillOrKill;

    [Fact]
    public void TestIocBidNoMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // No Match - will cancel order
      {
        var fc0 = new FillChecker(bid0, 0, 0, IOC);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        //var fc3 = new FillChecker(ask0, 0, 0);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, bid0, false, false, IOC));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestIocBidPartialMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 100);
      var bid0 = new SimpleOrder(true, 1250, 300);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Partial Match - will cancel order
      {
        var fc0 = new FillChecker(bid0, 100, 125000, IOC);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 100, 125000);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, bid0, true, false, IOC));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestIocBidFullMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 400);
      var bid0 = new SimpleOrder(true, 1250, 300);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1250, 1, 400));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Full Match - will complete order
      {
        var fc0 = new FillChecker(bid0, 300, 1250 * 300, IOC);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 300, 1250 * 300);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, bid0, true, true, IOC));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestIocBidMultiMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 400);
      var bid0 = new SimpleOrder(true, 1251, 500);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1250, 1, 400));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Full Match - will complete order
      {
        var fc0 = new FillChecker(bid0, 500, 625100, IOC);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 400, 1250 * 400);
        var fc4 = new FillChecker(ask1, 100, 1251 * 100);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, bid0, true, true, IOC));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestFokBidNoMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // No Match - will cancel order
      {
        var fc0 = new FillChecker(bid0, 0, 0, FOK);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        //var fc3 = new FillChecker(ask0, 0, 0);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, bid0, false, false, FOK));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestFokBidPartialMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 100);
      var bid0 = new SimpleOrder(true, 1250, 300);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Partial Match - will not fill and will cancel order
      {
        var fc0 = new FillChecker(bid0, 0, 0, FOK);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 0, 0);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, bid0, false, false, FOK));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestFokBidFullMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 400);
      var bid0 = new SimpleOrder(true, 1250, 300);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1250, 1, 400));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Full Match - will complete order
      {
        var fc0 = new FillChecker(bid0, 300, 1250 * 300, FOK);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 300, 1250 * 300);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, bid0, true, true, FOK));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestFokBidMultiMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 400);
      var bid0 = new SimpleOrder(true, 1251, 500);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1250, 1, 400));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Full Match - will complete order
      {
        var fc0 = new FillChecker(bid0, 500, 625100, FOK);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 400, 1250 * 400);
        var fc4 = new FillChecker(ask1, 100, 1251 * 100);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, bid0, true, true, FOK));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestIocAskNoMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // No Match - will cancel order
      {
        // var fc0 = new FillChecker(bid0, 0, 0);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 0, 0, IOC);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, ask0, false, false, IOC));
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestIocAskPartialMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 300);
      var bid0 = new SimpleOrder(true, 1250, 100);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Partial Match - will cancel order
      {
        var fc0 = new FillChecker(bid0, 100, 125000);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 100, 125000, IOC);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, ask0, true, false, IOC));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestIocAskFullMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 300);
      var bid0 = new SimpleOrder(true, 1250, 300);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 300));
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Full match
      {
        var fc0 = new FillChecker(bid0, 300, 1250 * 300);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 300, 1250 * 300, IOC);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, ask0, true, true, IOC));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestIocAskMultiMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1249, 400);
      var bid0 = new SimpleOrder(true, 1250, 300);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 300));
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Full match
      {
        var fc0 = new FillChecker(bid0, 300, 1250 * 300);
        var fc1 = new FillChecker(bid1, 100, 1249 * 100);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 400, 499900, IOC);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, ask0, true, true, IOC));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestFokAskNoMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // No Match - will cancel order
      {
        // var fc0 = new FillChecker(bid0, 0, 0);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 0, 0, FOK);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, ask0, false, false, FOK));
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestFokAskPartialMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 300);
      var bid0 = new SimpleOrder(true, 1250, 100);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Partial Match - will not fill and will cancel order
      {
        var fc0 = new FillChecker(bid0, 0, 0);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 0, 0, FOK);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, ask0, false, false, FOK));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestFokAskFullMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1250, 300);
      var bid0 = new SimpleOrder(true, 1250, 300);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 300));
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Full Match
      {
        var fc0 = new FillChecker(bid0, 300, 1250 * 300);
        var fc1 = new FillChecker(bid1, 0, 0);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 300, 1250 * 300, FOK);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, ask0, true, true, FOK));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestFokAskMultiMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 100);
      var ask0 = new SimpleOrder(false, 1249, 400);
      var bid0 = new SimpleOrder(true, 1250, 300);
      var bid1 = new SimpleOrder(true, 1249, 100);
      var bid2 = new SimpleOrder(true, 1248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 300));
      Assert.True(dc.VerifyBid(1249, 1, 100));
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Full match
      {
        var fc0 = new FillChecker(bid0, 300, 1250 * 300);
        var fc1 = new FillChecker(bid1, 100, 1249 * 100);
        var fc2 = new FillChecker(bid2, 0, 0);
        var fc3 = new FillChecker(ask0, 400, 499900, FOK);
        var fc4 = new FillChecker(ask1, 0, 0);
        var fc5 = new FillChecker(ask2, 0, 0);
        Assert.True(AddAndVerify(order_book, ask0, true, true, FOK));
        fc0.AssertFillSuccess();
        fc1.AssertFillSuccess();
        fc2.AssertFillSuccess();
        fc3.AssertFillSuccess();
        fc4.AssertFillSuccess();
        fc5.AssertFillSuccess();
      }

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1248, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }
  }
}