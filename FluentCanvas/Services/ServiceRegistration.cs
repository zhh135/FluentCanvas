using FluentCanvas.Helpers;
using FluentCanvas.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCanvas.Services
{
    /// <summary>
    /// Central registration point for the dependency injection container.
    /// </summary>
    public static class ServiceRegistration
    {
        public static void RegisterServices()
        {
            var services = Locator.Services;

            services.AddSingleton<IAppViewService, AppViewService>();
            services.AddSingleton<IInkBoardService, InkBoardService>();
            services.AddSingleton<IShellService, ShellService>();
            services.AddSingleton<IPowerPointService, PowerPointService>();

            if (App.IsPackaged)
            {
                services.AddSingleton<ISettingsService, WinRTSettingsService>();
                LogHelper.WriteLogToFile("ServiceRegistration | ISettingsService = WinRTSettingsService (packaged)", LogHelper.LogType.Info);
            }
            else
            {
                services.AddSingleton<ISettingsService, LocalSettingsService>();
                LogHelper.WriteLogToFile("ServiceRegistration | ISettingsService = LocalSettingsService (unpackaged)", LogHelper.LogType.Info);
            }

            services.AddSingleton<NotificationsViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<PptNavigationViewModel>();
            services.AddSingleton<MainViewModel>();
        }
    }
}
