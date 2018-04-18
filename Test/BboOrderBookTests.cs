using System.Linq;
using Liquibook.NET.Book;
using Liquibook.NET.Simple;
using Liquibook.NET.Types;
using Xunit;
using static Test.Utils;
using TrackerMap = System.Collections.Generic.MultiMap<Liquibook.NET.Book.ComparablePrice, Liquibook.NET.Book.OrderTracker>;
using SimpleTracker = Liquibook.NET.Book.OrderTracker;

namespace Test
{
  public class BboOrderBookTests
  {
    [Fact]
    public void TestBboBidsMultimapSortCorrect()
    {
      var bids = new TrackerMap();
      var order0 = new SimpleOrder(true, 1250, 100);
      var order1 = new SimpleOrder(true, 1255, 100);
      var order2 = new SimpleOrder(true, 1240, 100);
      var order3 = new SimpleOrder(true, 0, 100);
      var order4 = new SimpleOrder(true, 1245, 100);

      // Insert out of price order
      bids.Add(new ComparablePrice(true, order0.Price), new SimpleTracker(order0, OrderConditions.NoConditions));
      bids.Add(new ComparablePrice(true, order1.Price), new SimpleTracker(order1, OrderConditions.NoConditions));
      bids.Add(new ComparablePrice(true, order2.Price), new SimpleTracker(order2, OrderConditions.NoConditions));
      bids.Add(new ComparablePrice(true, 0), new SimpleTracker(order3, OrderConditions.NoConditions));
      bids.Add(new ComparablePrice(true, order4.Price), new SimpleTracker(order4, OrderConditions.NoConditions));

      // Should access in price order
      var expected_order = new[] {order3, order1, order0, order4, order2};
      var index = 0;

      foreach (var pair in bids)
      {
        if (expected_order[index].Price == Constants.MarketOrderPrice)
        {
          Assert.Equal(Constants.MarketOrderPrice, pair.Key.Price);
        }
        else
        {
          Assert.Equal(expected_order[index].Price, pair.Key.Price);
        }
        Assert.Equal(expected_order[index], pair.Value.Order);
        ++index;
      }
      
      //TODO fix stuff below
      // Should be able to search and find
      //Assert.True((bids.upper_bound(book::ComparablePrice(true, 1245)))->second.ptr()->price() == 1240);
      //Assert.True((bids.lower_bound(book::ComparablePrice(true, 1245)))->second.ptr()->price() == 1245);
    }

    [Fact]
    public void TestBboAsksMultimapSortCorrect()
    {
      var asks = new TrackerMap();
      var order0 = new SimpleOrder(false, 3250, 100);
      var order1 = new SimpleOrder(false, 3235, 800);
      var order2 = new SimpleOrder(false, 3230, 200);
      var order3 = new SimpleOrder(false, 0, 200);
      var order4 = new SimpleOrder(false, 3245, 100);
      var order5 = new SimpleOrder(false, 3265, 200);

      // Insert out of price order
      asks.Add(new ComparablePrice(false, order0.Price), new SimpleTracker(order0, OrderConditions.NoConditions));
      asks.Add(new ComparablePrice(false, order1.Price), new SimpleTracker(order1, OrderConditions.NoConditions));
      asks.Add(new ComparablePrice(false, order2.Price), new SimpleTracker(order2, OrderConditions.NoConditions));
      asks.Add(new ComparablePrice(false, Constants.MarketOrderPrice), new SimpleTracker(order3, OrderConditions.NoConditions));
      asks.Add(new ComparablePrice(false, order4.Price), new SimpleTracker(order4, OrderConditions.NoConditions));
      asks.Add(new ComparablePrice(false, order5.Price), new SimpleTracker(order5, OrderConditions.NoConditions));

      // Should access in price order
      var expected_order = new[] {order3, order2, order1, order4, order0, order5};
      var index = 0;

      foreach (var pair in asks)
      {
        if (expected_order[index].Price == Constants.MarketOrderPrice)
        {
          Assert.Equal(Constants.MarketOrderPrice, pair.Key.Price);
        }
        else
        {
          Assert.Equal(expected_order[index].Price, pair.Key.Price);
        }
        Assert.Equal(expected_order[index], pair.Value.Order);
        ++index;
      }
      
      //TODO see below
      //Assert.True((asks.upper_bound(book::ComparablePrice(false, 3235)))->second.ptr()->price() == 3245);
      //Assert.True((asks.lower_bound(book::ComparablePrice(false, 3235)))->second.ptr()->price() == 3235);
    }

    [Fact]
    public void TestBboAddCompleteBid()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 1252, 100);
      var ask0 = new SimpleOrder(false, 1251, 100);
      var bid1 = new SimpleOrder(true, 1251, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));

      // Match - complete
      {
        var fc1 = new FillChecker(bid1, 100, 125100);
        var fc2 = new FillChecker(ask0, 100, 125100);
        Assert.True(AddAndVerify(order_book, bid1, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());
    }

    [Fact]
    public void TestBboAddCompleteAsk()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1251, 100);
      var ask1 = new SimpleOrder(false, 1250, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));

      // Match - complete
      {
        var fc1 = new FillChecker(ask1, 100, 125000);
        var fc2 = new FillChecker(bid0, 100, 125000);
        Assert.True(AddAndVerify(order_book, ask1, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(0, 0, 0));
      Assert.True(dc.VerifyAsk(1251, 1, 100));

      // Verify sizes
      Assert.Equal(0, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());
    }

    [Fact]
    public void TestBboAddMultiMatchBid()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 1252, 100);
      var ask0 = new SimpleOrder(false, 1251, 300);
      var ask2 = new SimpleOrder(false, 1251, 200);
      var bid1 = new SimpleOrder(true, 1251, 500);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 2, 500));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Match - complete
      {
        var fc1 = new FillChecker(bid1, 500, 1251 * 500);
        var fc2 = new FillChecker(ask2, 200, 1251 * 200);
        var fc3 = new FillChecker(ask0, 300, 1251 * 300);
        Assert.True(AddAndVerify(order_book, bid1, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify remaining
      Assert.Equal(ask1, order_book.Asks.First().Value.Order);
    }

    [Fact]
    public void TestBboAddMultiMatchAsk()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 9252, 100);
      var ask0 = new SimpleOrder(false, 9251, 300);
      var ask2 = new SimpleOrder(false, 9251, 200);
      var ask3 = new SimpleOrder(false, 9250, 600);
      var bid0 = new SimpleOrder(true, 9250, 100);
      var bid1 = new SimpleOrder(true, 9250, 500);
      var bid2 = new SimpleOrder(true, 9248, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(9250, 2, 600));
      Assert.True(dc.VerifyAsk(9251, 2, 500));

      // Match - complete
      {
        var fc1 = new FillChecker(ask3, 600, 9250 * 600);
        var fc2 = new FillChecker(bid0, 100, 9250 * 100);
        var fc3 = new FillChecker(bid1, 500, 9250 * 500);
        Assert.True(AddAndVerify(order_book, ask3, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(9248, 1, 100));
      Assert.True(dc.VerifyAsk(9251, 2, 500));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify remaining
      Assert.Equal(bid2, order_book.Bids.First().Value.Order);
    }

    [Fact]
    public void TestBboAddPartialMatchBid()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 7253, 300);
      var ask1 = new SimpleOrder(false, 7252, 100);
      var ask2 = new SimpleOrder(false, 7251, 200);
      var bid1 = new SimpleOrder(true, 7251, 350);
      var bid0 = new SimpleOrder(true, 7250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(7250, 1, 100));
      Assert.True(dc.VerifyAsk(7251, 1, 200));

      // Match - partial
      {
        var fc1 = new FillChecker(bid1, 200, 7251 * 200);
        var fc2 = new FillChecker(ask2, 200, 7251 * 200);
        Assert.True(AddAndVerify(order_book, bid1, true, false));
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(7251, 1, 150));
      Assert.True(dc.VerifyAsk(7252, 1, 100));

      // Verify remaining
      Assert.Equal(ask1, order_book.Asks.First().Value.Order);
      Assert.Equal(bid1, order_book.Bids.First().Value.Order);
    }

    [Fact]
    public void TestBboAddPartialMatchAsk()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1253, 300);
      var ask1 = new SimpleOrder(false, 1251, 400);
      var bid1 = new SimpleOrder(true, 1251, 350);
      var bid0 = new SimpleOrder(true, 1250, 100);
      var bid2 = new SimpleOrder(true, 1250, 200);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1251, 1, 350));
      Assert.True(dc.VerifyAsk(1253, 1, 300));

      // Match - partial
      {
        var fc1 = new FillChecker(ask1, 350, 1251 * 350);
        var fc2 = new FillChecker(bid1, 350, 1251 * 350);
        Assert.True(AddAndVerify(order_book, ask1, true, false));
      }


      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 2, 300));
      Assert.True(dc.VerifyAsk(1251, 1, 50));

      // Verify remaining
      Assert.Equal(bid0, order_book.Bids.First().Value.Order);
      Assert.Equal(ask1, order_book.Asks.First().Value.Order);
    }

    [Fact]
    public void TestBboAddMultiPartialMatchBid()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 1252, 100);
      var ask2 = new SimpleOrder(false, 1251, 200);
      var ask0 = new SimpleOrder(false, 1251, 300);
      var bid1 = new SimpleOrder(true, 1251, 750);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 2, 500));

      // Match - partial
      {
        var fc1 = new FillChecker(bid1, 500, 1251 * 500);
        var fc2 = new FillChecker(ask0, 300, 1251 * 300);
        var fc3 = new FillChecker(ask2, 200, 1251 * 200);
        Assert.True(AddAndVerify(order_book, bid1, true, false));
      }

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 250));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Verify remaining
      Assert.Equal(ask1, order_book.Asks.First().Value.Order);
      Assert.Equal(bid1, order_book.Bids.First().Value.Order);
    }

    [Fact]
    public void TestBboAddMultiPartialMatchAsk()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1253, 300);
      var ask1 = new SimpleOrder(false, 1251, 700);
      var bid1 = new SimpleOrder(true, 1251, 370);
      var bid2 = new SimpleOrder(true, 1251, 200);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1251, 2, 570));
      Assert.True(dc.VerifyAsk(1253, 1, 300));

      // Match - partial
      {
        var fc1 = new FillChecker(ask1, 570, 1251 * 570);
        var fc2 = new FillChecker(bid1, 370, 1251 * 370);
        var fc3 = new FillChecker(bid2, 200, 1251 * 200);
        Assert.True(AddAndVerify(order_book, ask1, true, false));
      }

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 130));

      // Verify remaining
      Assert.Equal(bid0, order_book.Bids.First().Value.Order);
      Assert.Equal((Quantity)100, order_book.Bids.First().Value.OpenQuantity);
      Assert.Equal(ask1, order_book.Asks.First().Value.Order);
      Assert.Equal((Quantity)130, order_book.Asks.First().Value.OpenQuantity);
    }

    [Fact]
    public void TestBboRepeatMatchBid()
    {
      var order_book = new SimpleOrderBook();
      var ask3 = new SimpleOrder(false, 1251, 400);
      var ask2 = new SimpleOrder(false, 1251, 200);
      var ask1 = new SimpleOrder(false, 1251, 300);
      var ask0 = new SimpleOrder(false, 1251, 100);
      var bid1 = new SimpleOrder(true, 1251, 900);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1251, 1, 900));

      // Match - repeated
      {
        var fc1 = new FillChecker(bid1, 100, 125100);
        var fc2 = new FillChecker(ask0, 100, 125100);
        Assert.True(AddAndVerify(order_book, ask0, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 800));

      {
        var fc1 = new FillChecker(bid1, 300, 1251 * 300);
        var fc2 = new FillChecker(ask1, 300, 1251 * 300);
        Assert.True(AddAndVerify(order_book, ask1, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 500));

      {
        var fc1 = new FillChecker(bid1, 200, 1251 * 200);
        var fc2 = new FillChecker(ask2, 200, 1251 * 200);
        Assert.True(AddAndVerify(order_book, ask2, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 300));

      {
        var fc1 = new FillChecker(bid1, 300, 1251 * 300);
        var fc2 = new FillChecker(ask3, 300, 1251 * 300);
        Assert.True(AddAndVerify(order_book, ask3, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());
    }

    [Fact]
    public void TestBboRepeatMatchAsk()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 900);
      var bid0 = new SimpleOrder(true, 1251, 100);
      var bid1 = new SimpleOrder(true, 1251, 300);
      var bid2 = new SimpleOrder(true, 1251, 200);
      var bid3 = new SimpleOrder(true, 1251, 400);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyAsk(1251, 1, 900));

      Assert.Equal(ask1, order_book.Asks.First().Value.Order);

      // Match - repeated
      {
        var fc1 = new FillChecker(ask1, 100, 125100);
        var fc2 = new FillChecker(bid0, 100, 125100);
        Assert.True(AddAndVerify(order_book, bid0, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1251, 1, 800));

      {
        var fc1 = new FillChecker(ask1, 300, 1251 * 300);
        var fc2 = new FillChecker(bid1, 300, 1251 * 300);
        Assert.True(AddAndVerify(order_book, bid1, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1251, 1, 500));

      {
        var fc1 = new FillChecker(ask1, 200, 1251 * 200);
        var fc2 = new FillChecker(bid2, 200, 1251 * 200);
        Assert.True(AddAndVerify(order_book, bid2, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1251, 1, 300));

      {
        var fc1 = new FillChecker(ask1, 300, 1251 * 300);
        var fc2 = new FillChecker(bid3, 300, 1251 * 300);
        Assert.True(AddAndVerify(order_book, bid3, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());
    }

    [Fact]
    public void TestBboAddMarketOrderBid()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 1252, 100);
      var ask0 = new SimpleOrder(false, 1251, 100);
      var bid1 = new SimpleOrder(true, 0, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));

      // Match - complete
      {
        var fc1 = new FillChecker(bid1, 100, 125100);
        var fc2 = new FillChecker(ask0, 100, 125100);
        Assert.True(AddAndVerify(order_book, bid1, true, true));
      }

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestBboAddMarketOrderAsk()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 0, 100);
      var bid1 = new SimpleOrder(true, 1251, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Match - complete
      {
        var fc1 = new FillChecker(bid1, 100, 125100);
        var fc2 = new FillChecker(ask1, 100, 125100);
        Assert.True(AddAndVerify(order_book, ask1, true, true));
      }

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }

    [Fact]
    public void TestBboAddMarketOrderBidMultipleMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 12520, 300);
      var ask0 = new SimpleOrder(false, 12510, 200);
      var bid1 = new SimpleOrder(true, 0, 500);
      var bid0 = new SimpleOrder(true, 12500, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(12500, 1, 100));
      Assert.True(dc.VerifyAsk(12510, 1, 200));

      // Match - complete
      {
        var fc1 = new FillChecker(bid1, 500, 12510 * 200 + 12520 * 300);
        var fc2 = new FillChecker(ask0, 200, 12510 * 200);
        var fc3 = new FillChecker(ask1, 300, 12520 * 300);
        Assert.True(AddAndVerify(order_book, bid1, true, true));
      }

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(0, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(12500, 1, 100));
      Assert.True(dc.VerifyAsk(0, 0, 0));
    }

    [Fact]
    public void TestBboAddMarketOrderAskMultipleMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 12520, 100);
      var ask1 = new SimpleOrder(false, 0, 600);
      var bid1 = new SimpleOrder(true, 12510, 200);
      var bid0 = new SimpleOrder(true, 12500, 400);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(12510, 1, 200));
      Assert.True(dc.VerifyAsk(12520, 1, 100));

      // Match - complete
      {
        var fc1 = new FillChecker(bid0, 400, 12500 * 400);
        var fc2 = new FillChecker(bid1, 200, 12510 * 200);
        var fc3 = new FillChecker(ask1, 600, 12500 * 400 + 12510 * 200);
        Assert.True(AddAndVerify(order_book, ask1, true, true));
      }

      // Verify sizes
      Assert.Equal(0, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(0, 0, 0));
      Assert.True(dc.VerifyAsk(12520, 1, 100));
    }

    [Fact]
    public void TestBboMatchMarketOrderBid()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1253, 100);
      var bid1 = new SimpleOrder(true, 0, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(0, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(0, 0, 0));

      // Match - complete
      {
        var fc1 = new FillChecker(bid1, 100, 125300);
        var fc2 = new FillChecker(ask0, 100, 125300);
        Assert.True(AddAndVerify(order_book, ask0, true, true));
      }

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(0, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(0, 0, 0));
    }

    [Fact]
    public void TestBboMatchMarketOrderAsk()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 0, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify sizes
      Assert.Equal(0, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyAsk(1252, 1, 100));
      Assert.True(dc.VerifyBid(0, 0, 0));

      // Match - complete
      {
        var fc1 = new FillChecker(bid0, 100, 125000);
        var fc2 = new FillChecker(ask1, 100, 125000);
        Assert.True(AddAndVerify(order_book, bid0, true, true));
      }

      // Verify sizes
      Assert.Equal(0, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1252, 1, 100));
    }


    [Fact]
    public void TestBboMatchMultipleMarketOrderBid()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1253, 400);
      var bid1 = new SimpleOrder(true, 0, 100);
      var bid2 = new SimpleOrder(true, 0, 200);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));

      // Verify sizes
      Assert.Equal(3, order_book.Bids.Count());
      Assert.Equal(0, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyAsk(0, 0, 0));
      Assert.True(dc.VerifyBid(1250, 1, 100));

      // Match - complete
      {
        var fc1 = new FillChecker(bid1, 100, 1253 * 100);
        var fc2 = new FillChecker(bid2, 200, 1253 * 200);
        var fc3 = new FillChecker(ask0, 300, 1253 * 300);
        Assert.True(AddAndVerify(order_book, ask0, true, false));
      }

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1253, 1, 100));
      Assert.True(dc.VerifyBid(1250, 1, 100));
    }


    [Fact]
    public void TestBboMatchMultipleMarketOrderAsk()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1252, 100);
      var ask2 = new SimpleOrder(false, 0, 400);
      var ask1 = new SimpleOrder(false, 0, 100);
      var bid0 = new SimpleOrder(true, 1250, 300);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));

      // Verify sizes
      Assert.Equal(0, order_book.Bids.Count());
      Assert.Equal(3, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyAsk(1252, 1, 100));
      Assert.True(dc.VerifyBid(0, 0, 0));

      // Match - partiaL
      {
        var fc1 = new FillChecker(bid0, 300, 1250 * 300);
        var fc2 = new FillChecker(ask1, 100, 1250 * 100);
        var fc3 = new FillChecker(ask2, 200, 1250 * 200);
        Assert.True(AddAndVerify(order_book, bid0, true, true));
      }

      // Verify sizes
      Assert.Equal(0, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1252, 1, 100));
      Assert.True(dc.VerifyBid(0, 0, 0));
    }

    [Fact]
    public void TestBboCancelBid()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 1252, 100);
      var ask0 = new SimpleOrder(false, 1251, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 100));

      // Cancel bid
      Assert.True(CancelAndVerify(order_book, bid0, OrderState.Cancelled));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(0, 0, 0));
      Assert.True(dc.VerifyAsk(1251, 1, 100));

      // Verify sizes
      Assert.Equal(0, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());
    }

    [Fact]
    public void TestBboCancelAskAndMatch()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 1252, 100);
      var ask0 = new SimpleOrder(false, 1251, 100);
      var bid2 = new SimpleOrder(true, 1252, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);
      var bid1 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify sizes
      Assert.Equal(2, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 2, 200));
      Assert.True(dc.VerifyAsk(1251, 1, 100));

      // Cancel bid
      Assert.True(CancelAndVerify(order_book, ask0, OrderState.Cancelled));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 2, 200));
      Assert.True(dc.VerifyAsk(1252, 1, 100));

      // Match - partiaL
      {
        var fc1 = new FillChecker(bid2, 100, 1252 * 100);
        var fc2 = new FillChecker(ask1, 100, 1252 * 100);
        Assert.True(AddAndVerify(order_book, bid2, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 2, 200));
      Assert.True(dc.VerifyAsk(0, 0, 0));

      // Cancel bid
      Assert.True(CancelAndVerify(order_book, bid0, OrderState.Cancelled));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(0, 0, 0));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(0, order_book.Asks.Count());
    }

    [Fact]
    public void TestBboCancelBidFail()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1251, 100);
      var ask1 = new SimpleOrder(false, 1250, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyBid(1250, 1, 100));

      // Match - complete
      {
        var fc1 = new FillChecker(ask1, 100, 125000);
        var fc2 = new FillChecker(bid0, 100, 125000);
        Assert.True(AddAndVerify(order_book, ask1, true, true));
      }

      // Verify sizes
      Assert.Equal(0, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyBid(0, 0, 0));

      // Cancel a filled order
      Assert.True(CancelAndVerify(order_book, bid0, OrderState.Complete));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyBid(0, 0, 0));
    }

    [Fact]
    public void TestBboCancelAskFail()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 1252, 100);
      var ask0 = new SimpleOrder(false, 1251, 100);
      var bid1 = new SimpleOrder(true, 1251, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(2, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyAsk(1251, 1, 100));
      Assert.True(dc.VerifyBid(1250, 1, 100));

      // Match - complete
      {
        var fc1 = new FillChecker(bid1, 100, 125100);
        var fc2 = new FillChecker(ask0, 100, 125100);
        Assert.True(AddAndVerify(order_book, bid1, true, true));
      }

      // Verify sizes
      Assert.Equal(1, order_book.Bids.Count());
      Assert.Equal(1, order_book.Asks.Count());

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1252, 1, 100));
      Assert.True(dc.VerifyBid(1250, 1, 100));

      // Cancel a filled order
      Assert.True(CancelAndVerify(order_book, ask0, OrderState.Complete));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyAsk(1252, 1, 100));
      Assert.True(dc.VerifyBid(1250, 1, 100));
    }

    [Fact]
    public void TestBboCancelBidRestore()
    {
      var order_book = new SimpleOrderBook();
      var ask10 = new SimpleOrder(false, 1258, 600);
      var ask9 = new SimpleOrder(false, 1257, 700);
      var ask8 = new SimpleOrder(false, 1256, 100);
      var ask7 = new SimpleOrder(false, 1256, 100);
      var ask6 = new SimpleOrder(false, 1255, 500);
      var ask5 = new SimpleOrder(false, 1255, 200);
      var ask4 = new SimpleOrder(false, 1254, 300);
      var ask3 = new SimpleOrder(false, 1252, 200);
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 400);
      var ask0 = new SimpleOrder(false, 1250, 500);

      var bid0 = new SimpleOrder(true, 1249, 100);
      var bid1 = new SimpleOrder(true, 1249, 200);
      var bid2 = new SimpleOrder(true, 1249, 200);
      var bid3 = new SimpleOrder(true, 1248, 400);
      var bid4 = new SimpleOrder(true, 1246, 600);
      var bid5 = new SimpleOrder(true, 1246, 500);
      var bid6 = new SimpleOrder(true, 1245, 200);
      var bid7 = new SimpleOrder(true, 1245, 100);
      var bid8 = new SimpleOrder(true, 1245, 200);
      var bid9 = new SimpleOrder(true, 1244, 700);
      var bid10 = new SimpleOrder(true, 1244, 300);
      var bid11 = new SimpleOrder(true, 1242, 300);
      var bid12 = new SimpleOrder(true, 1241, 400);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, ask3, false));
      Assert.True(AddAndVerify(order_book, ask4, false));
      Assert.True(AddAndVerify(order_book, ask5, false));
      Assert.True(AddAndVerify(order_book, ask6, false));
      Assert.True(AddAndVerify(order_book, ask7, false));
      Assert.True(AddAndVerify(order_book, ask8, false));
      Assert.True(AddAndVerify(order_book, ask9, false));
      Assert.True(AddAndVerify(order_book, ask10, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));
      Assert.True(AddAndVerify(order_book, bid3, false));
      Assert.True(AddAndVerify(order_book, bid4, false));
      Assert.True(AddAndVerify(order_book, bid5, false));
      Assert.True(AddAndVerify(order_book, bid6, false));
      Assert.True(AddAndVerify(order_book, bid7, false));
      Assert.True(AddAndVerify(order_book, bid8, false));
      Assert.True(AddAndVerify(order_book, bid9, false));
      Assert.True(AddAndVerify(order_book, bid10, false));
      Assert.True(AddAndVerify(order_book, bid11, false));
      Assert.True(AddAndVerify(order_book, bid12, false));

      // Verify sizes
      Assert.Equal(13, order_book.Bids.Count());
      Assert.Equal(11, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 3, 500));
      Assert.True(dc.VerifyAsk(1250, 1, 500));

      // Cancel a bid level (erase)
      Assert.True(CancelAndVerify(order_book, bid3, OrderState.Cancelled));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 3, 500));
      Assert.True(dc.VerifyAsk(1250, 1, 500));

      // Cancel common bid levels (not erased)
      Assert.True(CancelAndVerify(order_book, bid7, OrderState.Cancelled));
      Assert.True(CancelAndVerify(order_book, bid4, OrderState.Cancelled));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 3, 500));
      Assert.True(dc.VerifyAsk(1250, 1, 500));

      // Cancel the best bid level (erased)
      Assert.True(CancelAndVerify(order_book, bid1, OrderState.Cancelled));
      Assert.True(CancelAndVerify(order_book, bid0, OrderState.Cancelled));
      Assert.True(CancelAndVerify(order_book, bid2, OrderState.Cancelled));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1246, 1, 500));
      Assert.True(dc.VerifyAsk(1250, 1, 500));
    }

    [Fact]
    public void TestBboCancelAskRestore()
    {
      var order_book = new SimpleOrderBook();
      var ask10 = new SimpleOrder(false, 1258, 600);
      var ask9 = new SimpleOrder(false, 1257, 700);
      var ask8 = new SimpleOrder(false, 1256, 100);
      var ask7 = new SimpleOrder(false, 1256, 100);
      var ask6 = new SimpleOrder(false, 1255, 500);
      var ask5 = new SimpleOrder(false, 1255, 200);
      var ask4 = new SimpleOrder(false, 1254, 300);
      var ask3 = new SimpleOrder(false, 1252, 200);
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 400);
      var ask0 = new SimpleOrder(false, 1250, 500);

      var bid0 = new SimpleOrder(true, 1249, 100);
      var bid1 = new SimpleOrder(true, 1249, 200);
      var bid2 = new SimpleOrder(true, 1249, 200);
      var bid3 = new SimpleOrder(true, 1248, 400);
      var bid4 = new SimpleOrder(true, 1246, 600);
      var bid5 = new SimpleOrder(true, 1246, 500);
      var bid6 = new SimpleOrder(true, 1245, 200);
      var bid7 = new SimpleOrder(true, 1245, 100);
      var bid8 = new SimpleOrder(true, 1245, 200);
      var bid9 = new SimpleOrder(true, 1244, 700);
      var bid10 = new SimpleOrder(true, 1244, 300);
      var bid11 = new SimpleOrder(true, 1242, 300);
      var bid12 = new SimpleOrder(true, 1241, 400);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, ask3, false));
      Assert.True(AddAndVerify(order_book, ask4, false));
      Assert.True(AddAndVerify(order_book, ask5, false));
      Assert.True(AddAndVerify(order_book, ask6, false));
      Assert.True(AddAndVerify(order_book, ask7, false));
      Assert.True(AddAndVerify(order_book, ask8, false));
      Assert.True(AddAndVerify(order_book, ask9, false));
      Assert.True(AddAndVerify(order_book, ask10, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));
      Assert.True(AddAndVerify(order_book, bid3, false));
      Assert.True(AddAndVerify(order_book, bid4, false));
      Assert.True(AddAndVerify(order_book, bid5, false));
      Assert.True(AddAndVerify(order_book, bid6, false));
      Assert.True(AddAndVerify(order_book, bid7, false));
      Assert.True(AddAndVerify(order_book, bid8, false));
      Assert.True(AddAndVerify(order_book, bid9, false));
      Assert.True(AddAndVerify(order_book, bid10, false));
      Assert.True(AddAndVerify(order_book, bid11, false));
      Assert.True(AddAndVerify(order_book, bid12, false));

      // Verify sizes
      Assert.Equal(13, order_book.Bids.Count());
      Assert.Equal(11, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 3, 500));
      Assert.True(dc.VerifyAsk(1250, 1, 500));

      // Cancel an ask level (erase)
      Assert.True(CancelAndVerify(order_book, ask1, OrderState.Cancelled));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 3, 500));
      Assert.True(dc.VerifyAsk(1250, 1, 500));

      // Cancel common ask levels (not erased)
      Assert.True(CancelAndVerify(order_book, ask2, OrderState.Cancelled));
      Assert.True(CancelAndVerify(order_book, ask6, OrderState.Cancelled));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 3, 500));
      Assert.True(dc.VerifyAsk(1250, 1, 500));

      // Cancel the best ask level (erased)
      Assert.True(CancelAndVerify(order_book, ask0, OrderState.Cancelled));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1249, 3, 500));
      Assert.True(dc.VerifyAsk(1252, 1, 200));
    }

    [Fact]
    public void TestBboFillCompleteBidRestoreDepth()
    {
      var order_book = new SimpleOrderBook();
      var ask10 = new SimpleOrder(false, 1258, 600);
      var ask9 = new SimpleOrder(false, 1257, 700);
      var ask8 = new SimpleOrder(false, 1256, 100);
      var ask7 = new SimpleOrder(false, 1256, 100);
      var ask6 = new SimpleOrder(false, 1255, 500);
      var ask5 = new SimpleOrder(false, 1255, 200);
      var ask4 = new SimpleOrder(false, 1254, 300);
      var ask3 = new SimpleOrder(false, 1252, 200);
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 400);
      var ask0 = new SimpleOrder(false, 1250, 500);

      var bid0 = new SimpleOrder(true, 1249, 100);
      var bid1 = new SimpleOrder(true, 1249, 200);
      var bid2 = new SimpleOrder(true, 1249, 200);
      var bid3 = new SimpleOrder(true, 1248, 400);
      var bid4 = new SimpleOrder(true, 1246, 600);
      var bid5 = new SimpleOrder(true, 1246, 500);
      var bid6 = new SimpleOrder(true, 1245, 200);
      var bid7 = new SimpleOrder(true, 1245, 100);
      var bid8 = new SimpleOrder(true, 1245, 200);
      var bid9 = new SimpleOrder(true, 1244, 700);
      var bid10 = new SimpleOrder(true, 1244, 300);
      var bid11 = new SimpleOrder(true, 1242, 300);
      var bid12 = new SimpleOrder(true, 1241, 400);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, ask3, false));
      Assert.True(AddAndVerify(order_book, ask4, false));
      Assert.True(AddAndVerify(order_book, ask5, false));
      Assert.True(AddAndVerify(order_book, ask6, false));
      Assert.True(AddAndVerify(order_book, ask7, false));
      Assert.True(AddAndVerify(order_book, ask8, false));
      Assert.True(AddAndVerify(order_book, ask9, false));
      Assert.True(AddAndVerify(order_book, ask10, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));
      Assert.True(AddAndVerify(order_book, bid3, false));
      Assert.True(AddAndVerify(order_book, bid4, false));
      Assert.True(AddAndVerify(order_book, bid5, false));
      Assert.True(AddAndVerify(order_book, bid6, false));
      Assert.True(AddAndVerify(order_book, bid7, false));
      Assert.True(AddAndVerify(order_book, bid8, false));
      Assert.True(AddAndVerify(order_book, bid9, false));
      Assert.True(AddAndVerify(order_book, bid10, false));
      Assert.True(AddAndVerify(order_book, bid11, false));
      Assert.True(AddAndVerify(order_book, bid12, false));

      // Verify sizes
      Assert.Equal(13, order_book.Bids.Count());
      Assert.Equal(11, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 3, 500));
      Assert.True(dc.VerifyAsk(1250, 1, 500));

      // Fill the top bid level (erase) and add an ask level (insert)
      var cross_ask = new SimpleOrder(false, 1249, 800);
      {
        var fc1 = new FillChecker(bid0, 100, 1249 * 100);
        var fc2 = new FillChecker(bid1, 200, 1249 * 200);
        var fc3 = new FillChecker(bid2, 200, 1249 * 200);
        var fc4 = new FillChecker(cross_ask, 500, 1249 * 500);
        Assert.True(AddAndVerify(order_book, cross_ask, true, false));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1248, 1, 400));
      Assert.True(dc.VerifyAsk(1249, 1, 300)); // Inserted

      // Fill the top bid level (erase) but do not add an ask level (no insert)
      var cross_ask2 = new SimpleOrder(false, 1248, 400);
      {
        var fc1 = new FillChecker(bid3, 400, 1248 * 400);
        var fc4 = new FillChecker(cross_ask2, 400, 1248 * 400);
        Assert.True(AddAndVerify(order_book, cross_ask2, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1246, 2, 1100));
      Assert.True(dc.VerifyAsk(1249, 1, 300));

      // Fill the top bid level (erase) and add ask level (insert),
      //    but nothing to restore
      var cross_ask3 = new SimpleOrder(false, 1246, 2400);
      {
        var fc1 = new FillChecker(bid4, 600, 1246 * 600);
        var fc2 = new FillChecker(bid5, 500, 1246 * 500);
        var fc3 = new FillChecker(cross_ask3, 1100, 1246 * 1100);
        Assert.True(AddAndVerify(order_book, cross_ask3, true, false));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1245, 3, 500));
      Assert.True(dc.VerifyAsk(1246, 1, 1300));

      // Partial fill the top bid level (reduce) 
      var cross_ask4 = new SimpleOrder(false, 1245, 250);
      {
        var fc1 = new FillChecker(bid6, 200, 1245 * 200);
        var fc2 = new FillChecker(bid7, 50, 1245 * 50);
        var fc3 = new FillChecker(cross_ask4, 250, 1245 * 250);
        Assert.True(AddAndVerify(order_book, cross_ask4, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1245, 2, 250)); // 1 filled, 1 reduced
      Assert.True(dc.VerifyAsk(1246, 1, 1300));
    }

    [Fact]
    public void TestBboFillCompleteAskRestoreDepth()
    {
      var order_book = new SimpleOrderBook();
      var ask10 = new SimpleOrder(false, 1258, 600);
      var ask9 = new SimpleOrder(false, 1257, 700);
      var ask8 = new SimpleOrder(false, 1256, 100);
      var ask7 = new SimpleOrder(false, 1256, 100);
      var ask6 = new SimpleOrder(false, 1255, 500);
      var ask5 = new SimpleOrder(false, 1255, 200);
      var ask4 = new SimpleOrder(false, 1254, 300);
      var ask3 = new SimpleOrder(false, 1252, 200);
      var ask2 = new SimpleOrder(false, 1252, 100);
      var ask1 = new SimpleOrder(false, 1251, 400);
      var ask0 = new SimpleOrder(false, 1250, 500);

      var bid0 = new SimpleOrder(true, 1249, 100);
      var bid1 = new SimpleOrder(true, 1249, 200);
      var bid2 = new SimpleOrder(true, 1249, 200);
      var bid3 = new SimpleOrder(true, 1248, 400);
      var bid4 = new SimpleOrder(true, 1246, 600);
      var bid5 = new SimpleOrder(true, 1246, 500);
      var bid6 = new SimpleOrder(true, 1245, 200);
      var bid7 = new SimpleOrder(true, 1245, 100);
      var bid8 = new SimpleOrder(true, 1245, 200);
      var bid9 = new SimpleOrder(true, 1244, 700);
      var bid10 = new SimpleOrder(true, 1244, 300);
      var bid11 = new SimpleOrder(true, 1242, 300);
      var bid12 = new SimpleOrder(true, 1241, 400);

      // No match
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));
      Assert.True(AddAndVerify(order_book, ask2, false));
      Assert.True(AddAndVerify(order_book, ask3, false));
      Assert.True(AddAndVerify(order_book, ask4, false));
      Assert.True(AddAndVerify(order_book, ask5, false));
      Assert.True(AddAndVerify(order_book, ask6, false));
      Assert.True(AddAndVerify(order_book, ask7, false));
      Assert.True(AddAndVerify(order_book, ask8, false));
      Assert.True(AddAndVerify(order_book, ask9, false));
      Assert.True(AddAndVerify(order_book, ask10, false));
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));
      Assert.True(AddAndVerify(order_book, bid3, false));
      Assert.True(AddAndVerify(order_book, bid4, false));
      Assert.True(AddAndVerify(order_book, bid5, false));
      Assert.True(AddAndVerify(order_book, bid6, false));
      Assert.True(AddAndVerify(order_book, bid7, false));
      Assert.True(AddAndVerify(order_book, bid8, false));
      Assert.True(AddAndVerify(order_book, bid9, false));
      Assert.True(AddAndVerify(order_book, bid10, false));
      Assert.True(AddAndVerify(order_book, bid11, false));
      Assert.True(AddAndVerify(order_book, bid12, false));

      // Verify sizes
      Assert.Equal(13, order_book.Bids.Count());
      Assert.Equal(11, order_book.Asks.Count());

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1249, 3, 500));
      Assert.True(dc.VerifyAsk(1250, 1, 500));

      // Fill the top ask level (erase) and add a bid level (insert)
      var cross_bid = new SimpleOrder(true, 1250, 800);
      {
        var fc1 = new FillChecker(ask0, 500, 1250 * 500);
        var fc4 = new FillChecker(cross_bid, 500, 1250 * 500);
        Assert.True(AddAndVerify(order_book, cross_bid, true, false));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 300));
      Assert.True(dc.VerifyAsk(1251, 1, 400));

      // Fill the top ask level (erase) but do not add an bid level (no insert)
      var cross_bid2 = new SimpleOrder(true, 1251, 400);
      {
        var fc1 = new FillChecker(ask1, 400, 1251 * 400);
        var fc4 = new FillChecker(cross_bid2, 400, 1251 * 400);
        Assert.True(AddAndVerify(order_book, cross_bid2, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 300));
      Assert.True(dc.VerifyAsk(1252, 2, 300));

      // Fill the top ask level (erase) and add bid level (insert),
      //    but nothing to restore
      var cross_bid3 = new SimpleOrder(true, 1252, 2400);
      {
        var fc1 = new FillChecker(ask2, 100, 1252 * 100);
        var fc2 = new FillChecker(ask3, 200, 1252 * 200);
        var fc3 = new FillChecker(cross_bid3, 300, 1252 * 300);
        Assert.True(AddAndVerify(order_book, cross_bid3, true, false));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1252, 1, 2100)); // Insert
      Assert.True(dc.VerifyAsk(1254, 1, 300));

      // Fill the top ask level (erase) but nothing to restore
      var cross_bid4 = new SimpleOrder(true, 1254, 300);
      {
        var fc2 = new FillChecker(ask4, 300, 1254 * 300);
        var fc3 = new FillChecker(cross_bid4, 300, 1254 * 300);
        Assert.True(AddAndVerify(order_book, cross_bid4, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1252, 1, 2100));
      Assert.True(dc.VerifyAsk(1255, 2, 700));

      // Partial fill the top ask level (reduce) 
      var cross_bid5 = new SimpleOrder(true, 1255, 550);
      {
        var fc1 = new FillChecker(ask5, 200, 1255 * 200);
        var fc2 = new FillChecker(ask6, 350, 1255 * 350);
        var fc3 = new FillChecker(cross_bid5, 550, 1255 * 550);
        Assert.True(AddAndVerify(order_book, cross_bid5, true, true));
      }

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1252, 1, 2100));
      Assert.True(dc.VerifyAsk(1255, 1, 150)); // 1 filled, 1 reduced
    }

    [Fact]
    public void TestBboReplaceSizeDecrease()
    {
      var order_book = new SimpleOrderBook();
      var cc = new ChangedChecker(order_book.Depth);
      var ask1 = new SimpleOrder(false, 1252, 200);
      var ask0 = new SimpleOrder(false, 1252, 300);
      var bid1 = new SimpleOrder(true, 1251, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 2, 500));

      // Verify changed stamps
      Assert.True(cc.VerifyBboChanged(true, true));
      cc.Reset();

      // Replace size
      Assert.True(ReplaceAndVerify(order_book, bid0, -60));
      Assert.True(ReplaceAndVerify(order_book, ask0, -150));

      // Verify orders
      Assert.Equal((Quantity)40, bid0.OrderQty);
      Assert.Equal((Quantity)150, ask0.OrderQty);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 2, 350));

      // Verify changed stamps
      Assert.True(cc.VerifyBboChanged(false, true));
    }

    [Fact]
    public void TestBboReplaceSizeDecreaseCancel()
    {
      var order_book = new SimpleOrderBook();
      var cc = new ChangedChecker(order_book.Depth);
      var ask1 = new SimpleOrder(false, 1252, 200);
      var ask0 = new SimpleOrder(false, 1252, 300);
      var bid1 = new SimpleOrder(true, 1251, 400);
      var bid0 = new SimpleOrder(true, 1250, 100);
      var bid2 = new SimpleOrder(true, 1249, 700);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, bid2, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1251, 1, 400));
      Assert.True(dc.VerifyAsk(1252, 2, 500));

      // Partial Fill existing book
      var cross_bid = new SimpleOrder(true, 1252, 125);
      var cross_ask = new SimpleOrder(false, 1251, 100);

      {
        var fc1 = new FillChecker(cross_bid, 125, 1252 * 125);
        var fc2 = new FillChecker(ask0, 125, 1252 * 125);
        Assert.True(AddAndVerify(order_book, cross_bid, true, true));
      }
      {
        var fc1 = new FillChecker(cross_ask, 100, 1251 * 100);
        var fc2 = new FillChecker(bid1, 100, 1251 * 100);
        Assert.True(AddAndVerify(order_book, cross_ask, true, true));
      }

      // Verify quantity
      Assert.Equal((Quantity)175, ask0.OpenQuantity);
      Assert.Equal((Quantity)300, bid1.OpenQuantity);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 300));
      Assert.True(dc.VerifyAsk(1252, 2, 375));

      // Replace size - cancel
      Assert.True(ReplaceAndVerify(
        order_book, ask0, -175, Constants.PriceUnchanged, OrderState.Cancelled));

      // Verify orders
      Assert.Equal((Quantity)125, ask0.OrderQty);
      Assert.Equal((Quantity)0, ask0.OpenQuantity);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 300));
      Assert.True(dc.VerifyAsk(1252, 1, 200));

      // Replace size - reduce level
      Assert.True(ReplaceAndVerify(
        order_book, bid1, -100, Constants.PriceUnchanged, OrderState.Accepted));

      // Verify orders
      Assert.Equal((Quantity)300, bid1.OrderQty);
      Assert.Equal((Quantity)200, bid1.OpenQuantity);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 200));
      Assert.True(dc.VerifyAsk(1252, 1, 200));

      // Replace size - cancel and erase level
      Assert.True(ReplaceAndVerify(
        order_book, bid1, -200, Constants.PriceUnchanged, OrderState.Cancelled));

      // Verify orders
      Assert.Equal((Quantity)100, bid1.OrderQty);
      Assert.Equal((Quantity)0, bid1.OpenQuantity);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 200));
    }

    [Fact]
    public void TestBboReplaceSizeDecreaseTooMuch()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 1252, 200);
      var ask0 = new SimpleOrder(false, 1252, 300);
      var bid1 = new SimpleOrder(true, 1251, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 2, 500));

      var cross_bid = new SimpleOrder(true, 1252, 200);
      // Partial fill existing order
      {
        var fc1 = new FillChecker(cross_bid, 200, 1252 * 200);
        var fc2 = new FillChecker(ask0, 200, 1252 * 200);
        Assert.True(AddAndVerify(order_book, cross_bid, true, true));
      }

      // Verify open quantity
      Assert.Equal((Quantity)100, ask0.OpenQuantity);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 2, 300));

      // Replace size - not enough left
      order_book.Replace(ask0, -150, Constants.PriceUnchanged);

      // Verify ask0 state
      Assert.Equal((Quantity)0, ask0.OpenQuantity);
      Assert.Equal((Quantity)200, ask0.OrderQty);
      Assert.Equal(OrderState.Cancelled, ask0.State);

      // Verify depth unchanged
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 100));
      Assert.True(dc.VerifyAsk(1252, 1, 200));
    }

    [Fact]
    public void TestBboReplaceSizeIncreaseDecrease()
    {
      var order_book = new SimpleOrderBook();
      var ask1 = new SimpleOrder(false, 1252, 200);
      var ask0 = new SimpleOrder(false, 1251, 300);
      var bid1 = new SimpleOrder(true, 1251, 100);
      var bid0 = new SimpleOrder(true, 1250, 100);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1250, 1, 100));
      Assert.True(dc.VerifyAsk(1251, 1, 300));

      // Replace size
      Assert.True(ReplaceAndVerify(order_book, ask0, 50));
      Assert.True(ReplaceAndVerify(order_book, bid0, 25));

      Assert.True(ReplaceAndVerify(order_book, ask0, -100));
      Assert.True(ReplaceAndVerify(order_book, bid0, 25));

      Assert.True(ReplaceAndVerify(order_book, ask0, 300));
      Assert.True(ReplaceAndVerify(order_book, bid0, -75));

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1250, 1, 75));
      Assert.True(dc.VerifyAsk(1251, 1, 550));
    }

    [Fact]
    public void TestBboReplaceBidPriceChange()
    {
      var order_book = new SimpleOrderBook();
      var ask0 = new SimpleOrder(false, 1253, 300);
      var ask1 = new SimpleOrder(false, 1252, 200);
      var bid1 = new SimpleOrder(true, 1251, 140);
      var bid0 = new SimpleOrder(true, 1250, 120);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1251, 1, 140));
      Assert.True(dc.VerifyAsk(1252, 1, 200));

      // Replace price increase
      Assert.True(ReplaceAndVerify(order_book, bid0, Constants.SizeUnchanged, 1251));

      Assert.Equal((Price)1251, order_book.Bids.First().Key.Price);
      Assert.Equal(bid1, order_book.Bids.First().Value.Order);
      Assert.Equal((Price)1251, order_book.Bids.ElementAt(1).Key.Price);
      Assert.Equal(bid0, order_book.Bids.ElementAt(1).Value.Order);
      Assert.Equal(2, order_book.Bids.Count());

      // Verify order
      Assert.Equal((Price)1251, bid0.Price);
      Assert.Equal((Quantity)120, bid0.OrderQty);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 2, 260));
      Assert.True(dc.VerifyAsk(1252, 1, 200));

      // Replace price decrease
      Assert.True(ReplaceAndVerify(order_book, bid1, Constants.SizeUnchanged, 1250));

      // Verify price change in book
      Assert.Equal((Price)1251, order_book.Bids.First().Key.Price);
      Assert.Equal(bid0, order_book.Bids.First().Value.Order);
      Assert.Equal((Price)1250, order_book.Bids.ElementAt(1).Key.Price);
      Assert.Equal(bid1, order_book.Bids.ElementAt(1).Value.Order);
      Assert.Equal(2, order_book.Bids.Count());

      // Verify order
      Assert.Equal((Price)1250, bid1.Price);
      Assert.Equal((Quantity)140, bid1.OrderQty);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 120));
      Assert.True(dc.VerifyAsk(1252, 1, 200));
    }

    [Fact]
    public void TestBboReplaceAskPriceChange()
    {
      var order_book = new SimpleOrderBook();
      var cc = new ChangedChecker(order_book.Depth);

      var ask0 = new SimpleOrder(false, 1253, 300);
      var ask1 = new SimpleOrder(false, 1252, 200);
      var bid1 = new SimpleOrder(true, 1251, 140);
      var bid0 = new SimpleOrder(true, 1250, 120);

      // No match
      Assert.True(AddAndVerify(order_book, bid0, false));
      Assert.True(AddAndVerify(order_book, bid1, false));
      Assert.True(AddAndVerify(order_book, ask0, false));
      Assert.True(AddAndVerify(order_book, ask1, false));

      // Verify depth
      var dc = new DepthCheck(order_book.Depth);
      Assert.True(dc.VerifyBid(1251, 1, 140));
      Assert.True(dc.VerifyAsk(1252, 1, 200));

      // Replace price increase 1252 -> 1253
      Assert.True(ReplaceAndVerify(order_book, ask1, Constants.SizeUnchanged, 1253));

      // Verify price change in book
      Assert.Equal((Price)1253, order_book.Asks.First().Key.Price);
      Assert.Equal(ask0, order_book.Asks.First().Value.Order);
      Assert.Equal((Price)1253, order_book.Asks.ElementAt(1).Key.Price);
      Assert.Equal(ask1, order_book.Asks.ElementAt(1).Value.Order);
      Assert.Equal(2, order_book.Asks.Count());

      // Verify order
      Assert.Equal((Price)1253, ask1.Price);
      Assert.Equal((Quantity)200, ask1.OrderQty);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 140));
      Assert.True(dc.VerifyAsk(1253, 2, 500));

      // Replace price decrease 1253 -> 1252
      Assert.True(ReplaceAndVerify(order_book, ask0, Constants.SizeUnchanged, 1252));

      // Verify price change in book
      Assert.Equal((Price)1252, order_book.Asks.First().Key.Price);
      Assert.Equal(ask0, order_book.Asks.First().Value.Order);
      Assert.Equal((Price)1253, order_book.Asks.ElementAt(1).Key.Price);
      Assert.Equal(ask1, order_book.Asks.ElementAt(1).Value.Order);
      Assert.Equal(2, order_book.Asks.Count());

      // Verify order
      Assert.Equal((Price)1252, ask0.Price);
      Assert.Equal((Quantity)300, ask0.OrderQty);

      // Verify depth
      dc.Reset();
      Assert.True(dc.VerifyBid(1251, 1, 140));
      Assert.True(dc.VerifyAsk(1252, 1, 300));
    }
  }
}