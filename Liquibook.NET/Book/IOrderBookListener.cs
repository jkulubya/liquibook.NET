using System;
using Liquibook.NET.Events;

namespace Liquibook.NET.Book
{
    public interface IOrderBookListener
    {
        void OnOrderBookChange(object sender, OnOrderBookChangeEventArgs args);
    }
}