using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;

namespace FluentCanvas
{
    public partial class MainWindow : Window
    {
        private const string StartupTaskId = "FluentCanvasStartup";
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static async Task<bool> StartAutomaticallyCreate(string exeName)
        {
            if (App.IsPackaged)
            {
                try
                {
                    var task = await global::Windows.ApplicationModel.StartupTask.GetAsync(StartupTaskId);
                    var state = await task.RequestEnableAsync();
                    return state == global::Windows.ApplicationModel.StartupTaskState.Enabled;
                }
                catch (Exception ex)
                {
                    Helpers.LogHelper.WriteLogToFile($"StartupTask enable failed: {ex}", Helpers.LogHelper.LogType.Error);
                    return false;
                }
            }

            try
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
                key.SetValue(exeName, $"\"{Environment.ProcessPath}\"");
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public static async Task<bool> StartAutomaticallyDel(string exeName)
        {
            if (App.IsPackaged)
            {
                try
                {
                    var task = await global::Windows.ApplicationModel.StartupTask.GetAsync(StartupTaskId);
                    task.Disable();
                    return true;
                }
                catch (Exception ex)
                {
                    Helpers.LogHelper.WriteLogToFile($"StartupTask disable failed: {ex}", Helpers.LogHelper.LogType.Error);
                    return false;
                }
            }

            try
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
                key?.DeleteValue(exeName, false);
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public static bool StartAutomaticallyIsEnabled(string exeName)
        {
            if (App.IsPackaged)
            {
                try
                {
                    var task = Task.Run(() => global::Windows.ApplicationModel.StartupTask.GetAsync(StartupTaskId).AsTask()).GetAwaiter().GetResult();
                    return task.State == global::Windows.ApplicationModel.StartupTaskState.Enabled;
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
                return key?.GetValue(exeName) != null;
            }
            catch (Exception) { }
            return false;
        }
    }
}
