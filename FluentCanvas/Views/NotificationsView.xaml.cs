using FluentCanvas.Helpers;
using FluentCanvas.Services;
using FluentCanvas.ViewModels;
using System.Threading.Tasks;
using System.Windows;

namespace FluentCanvas.Views
{
    public partial class NotificationsView : FCViewBase, INotificationsView
    {
        private readonly NotificationsViewModel viewModel;

        public NotificationsView()
        {
            InitializeComponent();

            viewModel = Locator.Get<NotificationsViewModel>();
            DataContext = viewModel;
            viewModel.VisibilityChanged += OnVisibilityChanged;
        }

        public override string ViewName => nameof(NotificationsView);

        public override void ShowView() => AnimationsHelper.ShowWithSlideFromBottomAndFade(GridNotifications);

        public override void HideView() => AnimationsHelper.HideWithSlideAndFade(GridNotifications);

        public Task ShowAsync(string notice, bool isShowImmediately = true)
            => viewModel.ShowAsync(notice, isShowImmediately);

        private void OnVisibilityChanged(object sender, bool visible)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => OnVisibilityChanged(sender, visible));
                return;
            }

            if (visible) ShowView();
            else HideView();
        }
    }
}
