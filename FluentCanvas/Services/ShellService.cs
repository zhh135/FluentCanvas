namespace FluentCanvas.Services
{
    /// <summary>
    /// Bridges <see cref="IShellService"/> to the running main window.
    /// The window assigns itself through <see cref="Window"/> when constructed.
    /// </summary>
    public sealed class ShellService : IShellService
    {
        public static MainWindow Window { get; set; }

        public bool IsSlideShowActive => Window?.ShellIsSlideShowActive ?? false;

        public bool IsHostLoaded
        {
            get => Window?.ShellIsHostLoaded ?? false;
            set { if (Window != null) Window.ShellIsHostLoaded = value; }
        }

        public void AnimateFloatingBarMargin() => Window?.ShellAnimateFloatingBarMargin();

        public void HideSubPanels(string mode = null) => Window?.ShellHideSubPanels(mode);

        public void ApplySystemTheme() => Window?.ShellApplySystemTheme();

        public void RefreshCursorMode() => Window?.ShellRefreshCursorMode();

        public void ReloadSettings() => Window?.ShellReloadSettings();

        public int BoundsWidth
        {
            get => Window?.ShellBoundsWidth ?? 0;
            set { if (Window != null) Window.ShellBoundsWidth = value; }
        }

        public void ApplyScaling() => Window?.ShellApplyScaling();

        public void RestartAutoFoldTimer() => Window?.ShellRestartAutoFoldTimer();

        public void RestartKillProcessTimer() => Window?.ShellRestartKillProcessTimer();

        public void ShowNotification(string notice) => Window?.ShellShowNotification(notice);

        public void SetRunAtStartup(bool enabled) => Window?.ShellSetRunAtStartup(enabled);

        public void ExitApplication() => Window?.ShellExitApplication();

        public void RestartApplication() => Window?.ShellRestartApplication();

        public void SetEdgeGestureUtil(bool enabled) => Window?.ShellSetEdgeGestureUtil(enabled);
    }
}
