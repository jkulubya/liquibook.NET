using System;
using Liquibook.NET.Types;

namespace Liquibook.NET.Book
{
    public static class DepthConstants
    {
        public static Price InvalidLevelPrice = 0;
        public static Price MarketOrderBidSortPrice = int.MaxValue;
        public static Price MarketOrderAskSortPrice = 0;
    }
}