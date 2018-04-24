using Liquibook.NET.Events;

namespace Liquibook.NET.Book
{
    public interface IOrderListener
    {
        void OnAccept(object sender, OnAcceptEventArgs args);
        void OnReject(object sender, OnRejectEventArgs args);
        void OnFill(object sender, OnFillEventArgs args);
        void OnCancel(object sender, OnCancelEventArgs args);
        void OnCancelReject(object sender, OnCancelRejectEventArgs args);
        void OnReplace(object sender, OnReplaceEventArgs args);
        void OnReplaceReject(object sender, OnReplaceRejectEventArgs args);
    }
}