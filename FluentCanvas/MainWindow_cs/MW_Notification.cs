using System.Linq;
using System.Windows;

namespace FluentCanvas
{
    public partial class MainWindow : Window
    {
        public static void ShowNewMessage(string notice, bool isShowImmediately = true)
        {
            (Application.Current?.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow)?.ShowNotificationAsync(notice, isShowImmediately);
        }

        public async void ShowNotificationAsync(string notice, bool isShowImmediately = true)
        {
            if (ViewModel?.Notifications == null)
            {
                return;
            }

            await ViewModel.Notifications.ShowAsync(notice, isShowImmediately);
        }
    }
}
