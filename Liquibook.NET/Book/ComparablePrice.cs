using System;
using System.Net.Http.Headers;
using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public class ComparablePrice
    {
        private Price _price { get; set; }
        private bool _buySide { get; set; }
        
        public ComparablePrice(bool buySide, Price price)
        {
            _buySide = buySide;
            _price = price;
        }

        public bool Matches(Price rhs)
        {
            if (_price == rhs)
            {
                return true;
            }

            if (_buySide)
            {
                return rhs < _price || _price == Constants.MarketOrderPrice;
            }

            return _price < rhs || rhs == Constants.MarketOrderPrice;
        }

        public static bool operator <(ComparablePrice me, Price rhs)
        {
            if (me._price == Constants.MarketOrderPrice)
            {
                return rhs != Constants.MarketOrderPrice;
            }

            if (rhs == Constants.MarketOrderPrice)
            {
                return false;
            }

            if (me._buySide)
            {
                return rhs < me._price;
            }

            return me._price < rhs;
        }

        public static bool operator ==(ComparablePrice me, Price rhs)
        {
            return me._price == rhs;
        }

        public static bool operator !=(ComparablePrice me, Price rhs)
        {
            return !(me._price == rhs);
        }

        public static bool operator >(ComparablePrice me, Price rhs)
        {
            return me._price != Constants.MarketOrderPrice &&
                   ((rhs == Constants.MarketOrderPrice) || (me._buySide ? (rhs > me._price) : (me._price > rhs)));
        }

        public static bool operator <=(ComparablePrice me, Price rhs)
        {
            return me < rhs || me == rhs;
        }

        public static bool operator >=(ComparablePrice me, Price rhs)
        {
            return me > rhs || me == rhs;
        }

        public static bool operator <(ComparablePrice me, ComparablePrice rhs)
        {
            return me < rhs._price;
        }

        public static bool operator ==(ComparablePrice me, ComparablePrice rhs)
        {
            return me == rhs._price;
        }

        public static bool operator !=(ComparablePrice me, ComparablePrice rhs)
        {
            return me != rhs._price;
        }
        
        public static bool operator >(ComparablePrice me, ComparablePrice rhs)
        {
            return me > rhs._price;
        }

        public Price Price => _price;
        public bool IsBuy => _buySide;
        public bool IsMarket => _price == Constants.MarketOrderPrice;

        public static bool operator <(Price price, ComparablePrice key)
        {
            return key < price;
        }

        public static bool operator >(Price price, ComparablePrice key)
        {
            return key < price;
        }
        
        public static bool operator ==(Price price, ComparablePrice key)
        {
            return key == price;
        }

        public static bool operator != (Price price, ComparablePrice key)
        {
            return key != price;
        }

        public static bool operator <= (Price price, ComparablePrice key)
        {
            return key >= price;
        }

        public static bool operator >= (Price price, ComparablePrice key)
        {
            return key <= price;
        }

        public override string ToString()
        {
            //TODO
            return base.ToString();
        }
    }
}