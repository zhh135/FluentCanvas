using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentCanvas.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject
    {
        public NotificationsViewModel Notifications { get; } = new NotificationsViewModel();

        public SettingsViewModel Settings { get; } = new SettingsViewModel();
    }
}
