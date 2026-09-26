namespace FluentCanvas.Services
{
    /// <summary>
    /// Shell operations that extracted views need but that stay owned by the
    /// main window (floating bar, cursor mode, theming, updates).
    /// </summary>
    public interface IShellService
    {
        bool IsSlideShowActive { get; }

        bool IsHostLoaded { get; set; }

        void AnimateFloatingBarMargin();

        void HideSubPanels(string mode = null);

        void ApplySystemTheme();

        void RefreshCursorMode();

        void ReloadSettings();

        int BoundsWidth { get; set; }

        void ApplyScaling();

        void RestartAutoFoldTimer();

        void RestartKillProcessTimer();

        void ShowNotification(string notice);

        void SetRunAtStartup(bool enabled);

        void ExitApplication();

        void RestartApplication();

        void SetEdgeGestureUtil(bool enabled);
    }
}
