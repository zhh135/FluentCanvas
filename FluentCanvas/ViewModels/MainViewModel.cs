using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentCanvas.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject
    {
        public MainViewModel(NotificationsViewModel notifications, SettingsViewModel settings)
        {
            Notifications = notifications;
            Settings = settings;
        }

        public NotificationsViewModel Notifications { get; }

        public SettingsViewModel Settings { get; }
    }
}
