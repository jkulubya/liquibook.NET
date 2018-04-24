using Liquibook.NET.Events;

namespace Liquibook.NET.Book
{
    public interface IDepthListener
    {
        void OnDepthChange(object sender, OnDepthChangeEventArgs args);
    }
}