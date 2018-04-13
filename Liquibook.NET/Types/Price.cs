using System;

namespace Liquibook.NET.Types
{
    public struct Price : IComparable<Price>
    {
        private readonly int m_value;

        private Price(int price)
        {
            m_value = price;
        }
        
        public static implicit operator Price(int price)
        {
            return new Price(price);
        }

        public static implicit operator int(Price c)
        {
            return c.m_value;
        }

        public override string ToString()
        {
            return Convert.ToString(m_value);
        }

        public static Price operator +(Price a, Price b)
        {
            return a.m_value + b.m_value;
        }

        public static Price operator -(Price a, Price b)
        {
            return a.m_value - b.m_value;
        }

        public static Price operator *(Price a, Price b)
        {
            return a.m_value * b.m_value;
        }

        public static int operator *(Price a, Quantity b)
        {
            return a.m_value * b;
        }

        public static bool operator >(Price a, Price b)
        {
            return (a.m_value > b.m_value);
        }

        public static bool operator <(Price a, Price b)
        {
            return (a.m_value < b.m_value);
        }

        public static bool operator ==(Price a, Price b)
        {
            return (a.m_value == b.m_value);
        }

        public static bool operator !=(Price a, Price b)
        {
            return (a.m_value != b.m_value);
        }

        public override int GetHashCode()
        {
            return m_value;
        }
        
        public override bool Equals(object obj)
        {
            return m_value == (obj as Price?)?.m_value;
        }
        
        public bool Equals(Price price)
        {
            return m_value == price;
        }

        public int CompareTo(Price value)
        {
            if (m_value < value) return -1;
            if (m_value > value) return 1;
            return 0;
        }
    }
}