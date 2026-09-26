using FluentCanvas.Helpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace FluentCanvas
{
    public partial class MainWindow
    {
        internal bool ShellIsSlideShowActive => BtnPPTSlideShowEnd.Visibility == Visibility.Visible;

        internal bool ShellIsHostLoaded
        {
            get => isLoaded;
            set => isLoaded = value;
        }

        internal void ShellAnimateFloatingBarMargin() => ViewboxFloatingBarMarginAnimation();

        internal void ShellHideSubPanels(string mode) => HideSubPanels(mode);

        internal void ShellApplySystemTheme() => SystemEvents_UserPreferenceChanged(null, null);

        internal void ShellRefreshCursorMode() => inkCanvas_EditingModeChanged(inkCanvas, null);

        internal void ShellReloadSettings() => LoadSettings();

        internal int ShellBoundsWidth
        {
            get => BoundsWidth;
            set => BoundsWidth = value;
        }

        internal void ShellApplyScaling() => ApplyScaling();

        internal void ShellRestartAutoFoldTimer() => StartOrStoptimerCheckAutoFold();

        internal void ShellRestartKillProcessTimer()
        {
            if (Settings.Automation.IsAutoKillEasiNote || Settings.Automation.IsAutoKillPptService)
            {
                timerKillProcess.Start();
            }
            else
            {
                timerKillProcess.Stop();
            }
        }

        internal void ShellShowNotification(string notice) => ShowNotificationAsync(notice);

        internal async void ShellSetRunAtStartup(bool enabled)
        {
            if (enabled)
            {
                await StartAutomaticallyCreate("FluentCanvas");
            }
            else
            {
                await StartAutomaticallyDel("FluentCanvas");
            }
        }

        internal void ShellExitApplication()
        {
            CloseIsFromButton = true;
            Close();
        }

        internal void ShellRestartApplication()
        {
            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");

            CloseIsFromButton = true;
            Application.Current.Shutdown();
        }

        internal void ShellSetEdgeGestureUtil(bool enabled)
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                EdgeGestureUtil.DisableEdgeGestures(new WindowInteropHelper(this).Handle, enabled);
            }
        }
    }
}
