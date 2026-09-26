using FluentCanvas.Helpers;
using FluentCanvas.Services;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Windows.ApplicationModel.Activation;

namespace FluentCanvas
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AppInstance appInstance;
        private static string pendingActivationFile;

        public static string[] StartArgs = null;
        public static string RootPath =>
            Helpers.StorageHelper.UserDataFolder + System.IO.Path.DirectorySeparatorChar;

        public static string TakePendingActivationFile()
        {
            var filePath = pendingActivationFile;
            pendingActivationFile = null;
            return filePath;
        }

        private static string FindInkFile(string[] args)
        {
            if (args == null)
            {
                return null;
            }

            return args.FirstOrDefault(arg =>
                arg != null &&
                (arg.EndsWith(".icart", StringComparison.OrdinalIgnoreCase) ||
                 arg.EndsWith(".icstk", StringComparison.OrdinalIgnoreCase)));
        }

        private static string FindInkFile(IActivatedEventArgs args)
        {
            if (args is IFileActivatedEventArgs fileArgs)
            {
                return fileArgs.Files
                    .OfType<global::Windows.Storage.IStorageFile>()
                    .Select(file => file.Path)
                    .FirstOrDefault();
            }

            return null;
        }

        public static bool IsPackaged
        {
            get
            {
                try
                {
                    _ = global::Windows.ApplicationModel.Package.Current.Id;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            FluentCanvas.MainWindow.ShowNewMessage("抱歉，出现未预期的异常，可能导致 FluentCanvas 画板运行不稳定。\n建议保存墨迹后重启应用。", true);
            LogHelper.NewLog(e.Exception.ToString());
            e.Handled = true;
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            InstallHelper.RunStartupTasks();

            LogHelper.NewLog(string.Format("FluentCanvas Starting (Version: {0})", Assembly.GetExecutingAssembly().GetName().Version.ToString()));

            try
            {
                var currentInstance = AppInstance.GetCurrent();
                appInstance = AppInstance.FindOrRegisterForKey("FluentCanvas");
                if (!appInstance.IsCurrent && !Array.Exists(e.Args, arg => arg == "-m"))
                {
                    LogHelper.NewLog("Detected existing instance");
                    appInstance.RedirectActivationToAsync(currentInstance.GetActivatedEventArgs()).AsTask().GetAwaiter().GetResult();
                    LogHelper.NewLog("FluentCanvas automatically closed");
                    Shutdown();
                    return;
                }

                appInstance.Activated += AppInstance_Activated;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile(
                    $"Windows App SDK app lifecycle unavailable; running without single-instance redirection: {ex.Message}",
                    LogHelper.LogType.Error);
            }

            StartArgs = e.Args;
            pendingActivationFile = FindInkFile(e.Args);

            ServiceRegistration.RegisterServices();
            Locator.Initialize();

            var mainWindow = new MainWindow();
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }

        private void AppInstance_Activated(object sender, AppActivationArguments args)
        {
            var activatedFile = args.Kind == ExtendedActivationKind.File
                ? FindInkFile(args.Data as IActivatedEventArgs)
                : null;

            Dispatcher.BeginInvoke(() =>
            {
                if (Current.MainWindow == null)
                    return;

                if (Current.MainWindow.WindowState == WindowState.Minimized)
                    Current.MainWindow.WindowState = WindowState.Normal;

                Current.MainWindow.Show();
                Current.MainWindow.Activate();

                if (!string.IsNullOrEmpty(activatedFile) && Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.LoadInkCanvasFile(activatedFile);
                }
            });
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            try
            {
                if (System.Windows.Forms.SystemInformation.MouseWheelScrollLines == -1)
                    e.Handled = false;
                else
                    try
                    {
                        ScrollViewerEx SenderScrollViewer = (ScrollViewerEx)sender;
                        SenderScrollViewer.ScrollToVerticalOffset(SenderScrollViewer.VerticalOffset - e.Delta * 10 * System.Windows.Forms.SystemInformation.MouseWheelScrollLines / (double)120);
                        e.Handled = true;
                    }
                    catch {  }
            }
            catch {  }
        }
    }
}
