using Liquibook.NET.Events;

namespace Liquibook.NET.Book
{
    public interface IBboListener
    {
        void OnBboChange(object sender, OnBboChangeEventArgs args);
    }
}