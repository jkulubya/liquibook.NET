using Liquibook.NET.Book;
using Liquibook.NET.Simple;
using Liquibook.NET.Types;
using Xunit;

namespace Test
{
    public class Extras
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
        public void CheckThatPriceAddedTwiceToExcessDoesntFail()
        {
            var book = new SimpleOrderBook();
            var order0 = new SimpleOrder(sideBuy, 58, q100);
            var order1 = new SimpleOrder(sideBuy, prc57, q100);
            var order2 = new SimpleOrder(sideBuy, prc56, q100);
            var order3 = new SimpleOrder(sideBuy, prc55, q100);
            var order4 = new SimpleOrder(sideBuy, prc54, q100);
            var order5 = new SimpleOrder(sideBuy, prc53, q100);
            var order6 = new SimpleOrder(sideBuy, prc53, q100);
            
            Assert.True(Utils.AddAndVerify(book, order0, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order1, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order2, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order3, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order4, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order5, expectNoMatch));
            Assert.True(Utils.AddAndVerify(book, order6, expectNoMatch));
        }
    }
}