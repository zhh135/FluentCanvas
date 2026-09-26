using System.Threading.Tasks;

namespace FluentCanvas.Views
{
    /// <summary>Contract for the transient notification popup.</summary>
    public interface INotificationsView : IFCView
    {
        Task ShowAsync(string notice, bool isShowImmediately = true);
    }
}
