using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentCanvas.ViewModels
{
    /// <summary>
    /// Owns the transient notification state. The view subscribes to
    /// <see cref="VisibilityChanged"/> to run the show/hide animation.
    /// </summary>
    public sealed partial class NotificationsViewModel : ObservableObject
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public NotificationsViewModel()
        {
            Message = string.Empty;
        }

        [ObservableProperty]
        public partial string Message { get; set; }

        [ObservableProperty]
        public partial bool IsVisible { get; set; }

        public event EventHandler<bool> VisibilityChanged;

        public async Task ShowAsync(string notice, bool isShowImmediately = true)
        {
            Message = notice;
            SetVisibility(true);

            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            try
            {
                await Task.Delay(2000, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            SetVisibility(false);
        }

        public void Hide()
        {
            cancellationTokenSource.Cancel();
            SetVisibility(false);
        }

        private void SetVisibility(bool visible)
        {
            IsVisible = visible;
            VisibilityChanged?.Invoke(this, visible);
        }
    }
}
