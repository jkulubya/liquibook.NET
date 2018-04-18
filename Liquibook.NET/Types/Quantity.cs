using System;
using System.Diagnostics;

namespace Liquibook.NET.Types
{
    [DebuggerDisplay("{m_value, nq}")]
    public struct Quantity : IComparable<Quantity>, IEquatable<Quantity>
    {
        private readonly int m_value;

        private Quantity(int quantity)
        {
            m_value = quantity;
        }
        
        public static implicit operator Quantity(int quantity)
        {
            return new Quantity(quantity);
        }

        public static implicit operator int(Quantity c)
        {
            return c.m_value;
        }

        public override string ToString()
        {
            return Convert.ToString(m_value);
        }

        public static Quantity operator +(Quantity a, Quantity b)
        {
            return a.m_value + b.m_value;
        }

        public static Quantity operator -(Quantity a, Quantity b)
        {
            return a.m_value - b.m_value;
        }
        
        public static Price operator *(Quantity a, Quantity b)
        {
            return a.m_value * b.m_value;
        }

        public static int operator *(Quantity a, Price b)
        {
            return a.m_value * b;
        }

        public static bool operator >(Quantity a, Quantity b)
        {
            return (a.m_value > b.m_value);
        }

        public static bool operator <(Quantity a, Quantity b)
        {
            return (a.m_value < b.m_value);
        }

        public static bool operator ==(Quantity a, Quantity b)
        {
            return (a.m_value == b.m_value);
        }

        public static bool operator !=(Quantity a, Quantity b)
        {
            return (a.m_value != b.m_value);
        }

        public override int GetHashCode()
        {
            return m_value;
        }

        public int CompareTo(Quantity other)
        {
            if (m_value < other) return -1;
            if (m_value > other) return 1;
            return 0;
        }

        public override bool Equals(object obj)
        {
            return m_value == (obj as Quantity?)?.m_value;
        }
        
        public bool Equals(Quantity quantity)
        {
            return m_value == quantity;
        }
    }
}