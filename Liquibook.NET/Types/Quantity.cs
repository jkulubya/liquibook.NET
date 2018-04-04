using System;

namespace Liquibook.NET.Types
{
    public struct Quantity
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