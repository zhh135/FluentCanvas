using FluentCanvas.Helpers;
using FluentCanvas.Services;
using FluentCanvas.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FluentCanvas.Views
{
    public partial class SettingsPanelView : FCViewBase, ISettingsPanelView
    {
        private static IShellService Shell => Locator.Get<IShellService>();

        private static Settings SettingsModel => MainWindow.Settings;

        public SettingsPanelView()
        {
            InitializeComponent();
        }

        public override string ViewName => nameof(SettingsPanelView);

        public bool IsPanelVisible => IsViewVisible;

        public void ShowPanel() => ShowView();

        public void HidePanel() => HideView();

        public void TogglePanel() => ToggleView();

        public void ResetToSuggestion() => BtnResetToSuggestion_Click(null, null);

        public void SpecialVersionReset() => SpecialVersionResetToSuggestion_Click();


        public override void ShowView() => AnimationsHelper.ShowWithSlideFromBottomAndFade(this, 0.5);

        public override void HideView() => AnimationsHelper.HideWithSlideAndFade(this, 0.5);

        private void BtnExit_Click(object sender, RoutedEventArgs e) => Shell.ExitApplication();

        private void BtnRestart_Click(object sender, RoutedEventArgs e) => Shell.RestartApplication();

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible) HideView();
            else ShowView();
        }

        private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Appearance.Theme = ComboBoxTheme.SelectedIndex;
            Shell.ApplySystemTheme();
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchSupportWPS_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsSupportWPS = ToggleSwitchSupportWPS.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchIsAutoUpdate_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Startup.IsAutoUpdate = ToggleSwitchIsAutoUpdate.IsOn;
            IsAutoUpdateWithSilenceBlock.Visibility = ToggleSwitchIsAutoUpdate.IsOn ? Visibility.Visible : Visibility.Collapsed;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchIsAutoUpdateWithSilence_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Startup.IsAutoUpdateWithSilence = ToggleSwitchIsAutoUpdateWithSilence.IsOn;
            AutoUpdateTimePeriodBlock.Visibility = SettingsModel.Startup.IsAutoUpdateWithSilence ? Visibility.Visible : Visibility.Collapsed;
            MainWindow.SaveSettingsToFile();
        }

        private void AutoUpdateWithSilenceStartTimeComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Startup.AutoUpdateWithSilenceStartTime = (string)AutoUpdateWithSilenceStartTimeComboBox.SelectedItem;
            MainWindow.SaveSettingsToFile();
        }

        private void AutoUpdateWithSilenceEndTimeComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Startup.AutoUpdateWithSilenceEndTime = (string)AutoUpdateWithSilenceEndTimeComboBox.SelectedItem;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchRunAtStartup_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            Shell.SetRunAtStartup(ToggleSwitchRunAtStartup.IsOn);
        }

        private void ToggleSwitchFoldAtStartup_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Startup.IsFoldAtStartup = ToggleSwitchFoldAtStartup.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchSupportPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;

            SettingsModel.PowerPointSettings.PowerPointSupport = ToggleSwitchSupportPowerPoint.IsOn;
            MainWindow.SaveSettingsToFile();

            if (SettingsModel.PowerPointSettings.PowerPointSupport)
            {
                Locator.Get<IPowerPointService>().StartPolling();
            }
            else
            {
                Locator.Get<IPowerPointService>().StopPolling();
            }
        }

        private void ToggleSwitchShowCanvasAtNewSlideShow_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;

            SettingsModel.PowerPointSettings.IsShowCanvasAtNewSlideShow = ToggleSwitchShowCanvasAtNewSlideShow.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchEnableDisPlayFloatBarText_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Appearance.IsEnableDisPlayFloatBarText = ToggleSwitchEnableDisPlayFloatBarText.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.ReloadSettings();
        }

        private void ToggleSwitchEnableDisPlayNibModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Appearance.IsEnableDisPlayNibModeToggler = ToggleSwitchEnableDisPlayNibModeToggle.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.ReloadSettings();
        }

        private void ToggleSwitchIsColorfulViewboxFloatingBar_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Appearance.IsColorfulViewboxFloatingBar = ToggleSwitchColorfulViewboxFloatingBar.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.ReloadSettings();
        }

        private void SliderFloatingBarBottomMargin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Appearance.FloatingBarBottomMargin = e.NewValue;
            Shell.AnimateFloatingBarMargin();
            MainWindow.SaveSettingsToFile();
        }

        private void SliderFloatingBarScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Appearance.FloatingBarScale = e.NewValue;
            Shell.ApplyScaling(); // Apply the change visually
            MainWindow.SaveSettingsToFile();
        }

        private void SliderBlackboardScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Appearance.BlackboardScale = e.NewValue;
            Shell.ApplyScaling(); // Apply the change visually
            MainWindow.SaveSettingsToFile();
        }

        private void BtnSetFloatingBarScale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && double.TryParse(btn.Tag.ToString(), out double scalePercent))
            {
                SliderFloatingBarScale.Value = scalePercent;
            }
        }

        private void BtnSetBlackboardScale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && double.TryParse(btn.Tag.ToString(), out double scalePercent))
            {
                SliderBlackboardScale.Value = scalePercent;
            }
        }

        private void BtnSetFloatingBarMargin_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && double.TryParse(btn.Tag.ToString(), out double margin))
            {
                SliderFloatingBarBottomMargin.Value = margin;
            }
        }

        private void ToggleSwitchShowButtonPPTNavigationBottom_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsShowPPTNavigationBottom = ToggleSwitchShowButtonPPTNavigationBottom.IsOn;
            Locator.Get<PptNavigationViewModel>().IsBottomButtonVisible = SettingsModel.PowerPointSettings.IsShowPPTNavigationBottom;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchShowButtonPPTNavigationSides_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsShowPPTNavigationSides = ToggleSwitchShowButtonPPTNavigationSides.IsOn;
            Locator.Get<PptNavigationViewModel>().IsSideButtonVisible = SettingsModel.PowerPointSettings.IsShowPPTNavigationSides;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchShowPPTNavigationPanelBottom_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsShowBottomPPTNavigationPanel = ToggleSwitchShowPPTNavigationPanelBottom.IsOn;
            if (Shell.IsSlideShowActive)
            {
                Locator.Get<IAppViewService>().Get<IPptNavigationView>().ShowPanels(
                    SettingsModel.PowerPointSettings.IsShowBottomPPTNavigationPanel,
                    SettingsModel.PowerPointSettings.IsShowSidePPTNavigationPanel);
            }
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchShowPPTNavigationPanelSide_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsShowSidePPTNavigationPanel = ToggleSwitchShowPPTNavigationPanelSide.IsOn;
            if (Shell.IsSlideShowActive)
            {
                Locator.Get<IAppViewService>().Get<IPptNavigationView>().ShowPanels(
                    SettingsModel.PowerPointSettings.IsShowBottomPPTNavigationPanel,
                    SettingsModel.PowerPointSettings.IsShowSidePPTNavigationPanel);
            }
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchCompressPicturesUploaded_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Canvas.IsCompressPicturesUploaded = ToggleSwitchCompressPicturesUploaded.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchShowCursor_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Canvas.IsShowCursor = ToggleSwitchShowCursor.IsOn;
            Shell.RefreshCursorMode();
            MainWindow.SaveSettingsToFile();
        }

        private void ComboBoxEraserSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Canvas.EraserSize = ComboBoxEraserSize.SelectedIndex;
            MainWindow.SaveSettingsToFile();
        }

        private void ComboBoxHyperbolaAsymptoteOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Canvas.HyperbolaAsymptoteOption = (OptionalOperation)ComboBoxHyperbolaAsymptoteOption.SelectedIndex;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchAutoFoldInEasiNote_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInEasiNote = ToggleSwitchAutoFoldInEasiNote.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoFoldInEasiNoteIgnoreDesktopAnno_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInEasiNoteIgnoreDesktopAnno = ToggleSwitchAutoFoldInEasiNoteIgnoreDesktopAnno.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchAutoFoldInEasiCamera_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInEasiCamera = ToggleSwitchAutoFoldInEasiCamera.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoFoldInEasiNote3C_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInEasiNote3C = ToggleSwitchAutoFoldInEasiNote3C.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoFoldInSeewoPincoTeacher_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInSeewoPincoTeacher = ToggleSwitchAutoFoldInSeewoPincoTeacher.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoFoldInHiteTouchPro_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInHiteTouchPro = ToggleSwitchAutoFoldInHiteTouchPro.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoFoldInHiteCamera_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInHiteCamera = ToggleSwitchAutoFoldInHiteCamera.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoFoldInWxBoardMain_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInWxBoardMain = ToggleSwitchAutoFoldInWxBoardMain.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoFoldInOldZyBoard_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInOldZyBoard = ToggleSwitchAutoFoldInOldZyBoard.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoFoldInMSWhiteboard_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInMSWhiteboard = ToggleSwitchAutoFoldInMSWhiteboard.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoFoldInPPTSlideShow_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoFoldInPPTSlideShow = ToggleSwitchAutoFoldInPPTSlideShow.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartAutoFoldTimer();
        }

        private void ToggleSwitchAutoKillPptService_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoKillPptService = ToggleSwitchAutoKillPptService.IsOn;
            MainWindow.SaveSettingsToFile();

            Shell.RestartKillProcessTimer();
        }

        private void ToggleSwitchAutoKillEasiNote_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoKillEasiNote = ToggleSwitchAutoKillEasiNote.IsOn;
            MainWindow.SaveSettingsToFile();
            Shell.RestartKillProcessTimer();
        }

        private void ToggleSwitchSaveScreenshotsInDateFolders_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsSaveScreenshotsInDateFolders = ToggleSwitchSaveScreenshotsInDateFolders.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSaveStrokesAtScreenshot_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoSaveStrokesAtScreenshot = ToggleSwitchAutoSaveStrokesAtScreenshot.IsOn;
            ToggleSwitchAutoSaveStrokesAtClear.Header = ToggleSwitchAutoSaveStrokesAtScreenshot.IsOn ? "清屏时自动截图并保存墨迹" : "清屏时自动截图";
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSaveStrokesAtClear_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.IsAutoSaveStrokesAtClear = ToggleSwitchAutoSaveStrokesAtClear.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchHideStrokeWhenSelecting_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Canvas.HideStrokeWhenSelecting = ToggleSwitchHideStrokeWhenSelecting.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSaveStrokesInPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsAutoSaveStrokesInPowerPoint = ToggleSwitchAutoSaveStrokesInPowerPoint.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchNotifyPreviousPage_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsNotifyPreviousPage = ToggleSwitchNotifyPreviousPage.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchNotifyHiddenPage_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsNotifyHiddenPage = ToggleSwitchNotifyHiddenPage.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchNotifyAutoPlayPresentation_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsNotifyAutoPlayPresentation = ToggleSwitchNotifyAutoPlayPresentation.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void SideControlMinimumAutomationSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.MinimumAutomationStrokeNumber = (int)SideControlMinimumAutomationSlider.Value;
            MainWindow.SaveSettingsToFile();
        }

        private void AutoSavedStrokesLocationTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.AutoSavedStrokesLocation = AutoSavedStrokesLocation.Text;
            MainWindow.SaveSettingsToFile();
        }

        private void AutoSavedStrokesLocationButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (folderBrowser.SelectedPath.Length > 0) AutoSavedStrokesLocation.Text = folderBrowser.SelectedPath;
        }

        private void SetAutoSavedStrokesLocationToDiskDButton_Click(object sender, RoutedEventArgs e)
        {
            AutoSavedStrokesLocation.Text = @"D:\FluentCanvas";
        }

        private void SetAutoSavedStrokesLocationToDocumentFolderButton_Click(object sender, RoutedEventArgs e)
        {
            AutoSavedStrokesLocation.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\FluentCanvas";
        }

        private void ToggleSwitchAutoDelSavedFiles_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.AutoDelSavedFiles = ToggleSwitchAutoDelSavedFiles.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ComboBoxAutoDelSavedFilesDaysThreshold_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Automation.AutoDelSavedFilesDaysThreshold = int.Parse(((ComboBoxItem)ComboBoxAutoDelSavedFilesDaysThreshold.SelectedItem).Content.ToString());
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSaveScreenShotInPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsAutoSaveScreenShotInPowerPoint = ToggleSwitchAutoSaveScreenShotInPowerPoint.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ComboBoxMatrixTransformCenterPoint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Gesture.MatrixTransformCenterPoint = (MatrixTransformCenterPointOptions)ComboBoxMatrixTransformCenterPoint.SelectedIndex;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchEnableFingerGestureSlideShowControl_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsEnableFingerGestureSlideShowControl = ToggleSwitchEnableFingerGestureSlideShowControl.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSwitchTwoFingerGesture_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Gesture.AutoSwitchTwoFingerGesture = ToggleSwitchAutoSwitchTwoFingerGesture.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchEnableTwoFingerGestureInPresentationMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.PowerPointSettings.IsEnableTwoFingerGestureInPresentationMode = ToggleSwitchEnableTwoFingerGestureInPresentationMode.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void BtnResetToSuggestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Shell.IsHostLoaded = false;
                MainWindow.SetSettingsToRecommendation();
                MainWindow.SaveSettingsToFile();
                Shell.ReloadSettings();
                Shell.IsHostLoaded = true;
                ToggleSwitchRunAtStartup.IsOn = true;
            }
            catch { }
            Shell.ShowNotification("设置已重置为默认推荐设置~");
        }

        private async void SpecialVersionResetToSuggestion_Click()
        {
            await Task.Delay(1000);
            try
            {
                Shell.IsHostLoaded = false;
                MainWindow.SetSettingsToRecommendation();
                SettingsModel.Automation.AutoDelSavedFiles = true;
                SettingsModel.Automation.AutoDelSavedFilesDaysThreshold = 15;
                SettingsModel.Appearance.IsEnableDisPlayFloatBarText = true;
                SetAutoSavedStrokesLocationToDiskDButton_Click(null, null);
                MainWindow.SaveSettingsToFile();
                Shell.ReloadSettings();
                Shell.IsHostLoaded = true;
            }
            catch { }
        }

        private void ToggleSwitchIsSpecialScreen_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.IsSpecialScreen = ToggleSwitchIsSpecialScreen.IsOn;
            TouchMultiplierSlider.Visibility = ToggleSwitchIsSpecialScreen.IsOn ? Visibility.Visible : Visibility.Collapsed;
            MainWindow.SaveSettingsToFile();
        }

        private void TouchMultiplierSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.TouchMultiplier = e.NewValue;
            MainWindow.SaveSettingsToFile();
        }

        private void BorderCalculateMultiplier_TouchDown(object sender, TouchEventArgs e)
        {
            var args = e.GetTouchPoint(null).Bounds;
            double value;
            if (!SettingsModel.Advanced.IsQuadIR) value = args.Width;
            else value = Math.Sqrt(args.Width * args.Height); //四边红外
            TextBlockShowCalculatedMultiplier.Text = (5 / (value * 1.1)).ToString();
        }

        private void NibModeBoundsWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.NibModeBoundsWidth = (int)e.NewValue;
            Shell.BoundsWidth = SettingsModel.Startup.IsEnableNibMode ? Shell.BoundsWidth = SettingsModel.Advanced.NibModeBoundsWidth : Shell.BoundsWidth = SettingsModel.Advanced.FingerModeBoundsWidth;
            MainWindow.SaveSettingsToFile();
        }

        private void FingerModeBoundsWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.FingerModeBoundsWidth = (int)e.NewValue;
            Shell.BoundsWidth = SettingsModel.Startup.IsEnableNibMode ? Shell.BoundsWidth = SettingsModel.Advanced.NibModeBoundsWidth : Shell.BoundsWidth = SettingsModel.Advanced.FingerModeBoundsWidth;
            MainWindow.SaveSettingsToFile();
        }

        private void NibModeBoundsWidthThresholdValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.NibModeBoundsWidthThresholdValue = (double)e.NewValue;
            MainWindow.SaveSettingsToFile();
        }

        private void FingerModeBoundsWidthThresholdValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.FingerModeBoundsWidthThresholdValue = (double)e.NewValue;
            MainWindow.SaveSettingsToFile();
        }

        private void NibModeBoundsWidthEraserSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.NibModeBoundsWidthEraserSize = (double)e.NewValue;
            MainWindow.SaveSettingsToFile();
        }

        private void FingerModeBoundsWidthEraserSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.FingerModeBoundsWidthEraserSize = (double)e.NewValue;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchIsQuadIR_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.IsQuadIR = ToggleSwitchIsQuadIR.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchIsLogEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.IsLogEnabled = ToggleSwitchIsLogEnabled.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchIsSecondConfimeWhenShutdownApp_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.IsSecondConfimeWhenShutdownApp = ToggleSwitchIsSecondConfimeWhenShutdownApp.IsOn;
            MainWindow.SaveSettingsToFile();
        }

        private void ToggleSwitchIsEnableEdgeGestureUtil_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Shell.IsHostLoaded) return;
            SettingsModel.Advanced.IsEnableEdgeGestureUtil = ToggleSwitchIsEnableEdgeGestureUtil.IsOn;
            if (OperatingSystem.IsWindowsVersionAtLeast(10)) Shell.SetEdgeGestureUtil(ToggleSwitchIsEnableEdgeGestureUtil.IsOn);
            MainWindow.SaveSettingsToFile();
        }

        private void SCManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void HyperlinkSourceToPresentRepository_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/InkCanvas/Ink-Canvas-Artistry") { UseShellExecute = true });
            Shell.HideSubPanels();
        }

        private void HyperlinkSourceToOringinalRepository_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/WXRIW/Ink-Canvas") { UseShellExecute = true });
            Shell.HideSubPanels();
        }

        public void LoadFromSettings()
        {
            // Startup
            try
            {
                ToggleSwitchRunAtStartup.IsOn = MainWindow.StartAutomaticallyIsEnabled("FluentCanvas");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
            }
            ToggleSwitchIsAutoUpdate.IsOn = SettingsModel.Startup.IsAutoUpdate;
            IsAutoUpdateWithSilenceBlock.Visibility = SettingsModel.Startup.IsAutoUpdate ? Visibility.Visible : Visibility.Collapsed;
            ToggleSwitchIsAutoUpdateWithSilence.IsOn = SettingsModel.Startup.IsAutoUpdateWithSilence;
            AutoUpdateTimePeriodBlock.Visibility = SettingsModel.Startup.IsAutoUpdateWithSilence ? Visibility.Visible : Visibility.Collapsed;
            AutoUpdateWithSilenceTimeComboBox.InitializeAutoUpdateWithSilenceTimeComboBoxOptions(AutoUpdateWithSilenceStartTimeComboBox, AutoUpdateWithSilenceEndTimeComboBox);
            AutoUpdateWithSilenceStartTimeComboBox.SelectedItem = SettingsModel.Startup.AutoUpdateWithSilenceStartTime;
            AutoUpdateWithSilenceEndTimeComboBox.SelectedItem = SettingsModel.Startup.AutoUpdateWithSilenceEndTime;
            ToggleSwitchFoldAtStartup.IsOn = SettingsModel.Startup.IsFoldAtStartup;

            // Appearance
            ComboBoxTheme.SelectedIndex = SettingsModel.Appearance.Theme;
            ToggleSwitchEnableDisPlayFloatBarText.IsOn = SettingsModel.Appearance.IsEnableDisPlayFloatBarText;
            ToggleSwitchEnableDisPlayNibModeToggle.IsOn = SettingsModel.Appearance.IsEnableDisPlayNibModeToggler;
            ToggleSwitchColorfulViewboxFloatingBar.IsOn = SettingsModel.Appearance.IsColorfulViewboxFloatingBar;
            SliderFloatingBarScale.Value = SettingsModel.Appearance.FloatingBarScale;
            SliderBlackboardScale.Value = SettingsModel.Appearance.BlackboardScale;
            SliderFloatingBarBottomMargin.Value = SettingsModel.Appearance.FloatingBarBottomMargin;

            // PowerPointSettings
            var pptNavViewModel = Locator.Get<PptNavigationViewModel>();
            pptNavViewModel.IsBottomButtonVisible = SettingsModel.PowerPointSettings.IsShowPPTNavigationBottom;
            pptNavViewModel.IsSideButtonVisible = SettingsModel.PowerPointSettings.IsShowPPTNavigationSides;
            ToggleSwitchShowButtonPPTNavigationBottom.IsOn = SettingsModel.PowerPointSettings.IsShowPPTNavigationBottom;
            ToggleSwitchShowButtonPPTNavigationSides.IsOn = SettingsModel.PowerPointSettings.IsShowPPTNavigationSides;
            ToggleSwitchShowPPTNavigationPanelBottom.IsOn = SettingsModel.PowerPointSettings.IsShowBottomPPTNavigationPanel;
            ToggleSwitchShowPPTNavigationPanelSide.IsOn = SettingsModel.PowerPointSettings.IsShowSidePPTNavigationPanel;
            ToggleSwitchSupportPowerPoint.IsOn = SettingsModel.PowerPointSettings.PowerPointSupport;
            ToggleSwitchShowCanvasAtNewSlideShow.IsOn = SettingsModel.PowerPointSettings.IsShowCanvasAtNewSlideShow;
            ToggleSwitchEnableTwoFingerGestureInPresentationMode.IsOn = SettingsModel.PowerPointSettings.IsEnableTwoFingerGestureInPresentationMode;
            ToggleSwitchEnableFingerGestureSlideShowControl.IsOn = SettingsModel.PowerPointSettings.IsEnableFingerGestureSlideShowControl;
            ToggleSwitchAutoSaveStrokesInPowerPoint.IsOn = SettingsModel.PowerPointSettings.IsAutoSaveStrokesInPowerPoint;
            ToggleSwitchNotifyPreviousPage.IsOn = SettingsModel.PowerPointSettings.IsNotifyPreviousPage;
            ToggleSwitchNotifyHiddenPage.IsOn = SettingsModel.PowerPointSettings.IsNotifyHiddenPage;
            ToggleSwitchNotifyAutoPlayPresentation.IsOn = SettingsModel.PowerPointSettings.IsNotifyAutoPlayPresentation;
            ToggleSwitchSupportWPS.IsOn = SettingsModel.PowerPointSettings.IsSupportWPS;
            ToggleSwitchAutoSaveScreenShotInPowerPoint.IsOn = SettingsModel.PowerPointSettings.IsAutoSaveScreenShotInPowerPoint;

            // Gesture
            ComboBoxMatrixTransformCenterPoint.SelectedIndex = (int)SettingsModel.Gesture.MatrixTransformCenterPoint;
            ToggleSwitchAutoSwitchTwoFingerGesture.IsOn = SettingsModel.Gesture.AutoSwitchTwoFingerGesture;
            ToggleSwitchEnableTwoFingerRotationOnSelection.IsOn = SettingsModel.Gesture.IsEnableTwoFingerRotationOnSelection;

            // Canvas
            ComboBoxHyperbolaAsymptoteOption.SelectedIndex = (int)SettingsModel.Canvas.HyperbolaAsymptoteOption;
            ToggleSwitchShowCursor.IsOn = SettingsModel.Canvas.IsShowCursor;
            ComboBoxEraserSize.SelectedIndex = SettingsModel.Canvas.EraserSize;
            ToggleSwitchHideStrokeWhenSelecting.IsOn = SettingsModel.Canvas.HideStrokeWhenSelecting;

            // Advanced
            TouchMultiplierSlider.Value = SettingsModel.Advanced.TouchMultiplier;
            FingerModeBoundsWidthSlider.Value = SettingsModel.Advanced.FingerModeBoundsWidth;
            NibModeBoundsWidthSlider.Value = SettingsModel.Advanced.NibModeBoundsWidth;
            FingerModeBoundsWidthThresholdValueSlider.Value = SettingsModel.Advanced.FingerModeBoundsWidthThresholdValue;
            NibModeBoundsWidthThresholdValueSlider.Value = SettingsModel.Advanced.NibModeBoundsWidthThresholdValue;
            FingerModeBoundsWidthEraserSizeSlider.Value = SettingsModel.Advanced.FingerModeBoundsWidthEraserSize;
            NibModeBoundsWidthEraserSizeSlider.Value = SettingsModel.Advanced.NibModeBoundsWidthEraserSize;
            ToggleSwitchIsLogEnabled.IsOn = SettingsModel.Advanced.IsLogEnabled;
            ToggleSwitchIsSecondConfimeWhenShutdownApp.IsOn = SettingsModel.Advanced.IsSecondConfimeWhenShutdownApp;
            ToggleSwitchIsSpecialScreen.IsOn = SettingsModel.Advanced.IsSpecialScreen;
            TouchMultiplierSlider.Visibility = ToggleSwitchIsSpecialScreen.IsOn ? Visibility.Visible : Visibility.Collapsed;
            ToggleSwitchIsQuadIR.IsOn = SettingsModel.Advanced.IsQuadIR;
            ToggleSwitchIsEnableEdgeGestureUtil.IsOn = SettingsModel.Advanced.IsEnableEdgeGestureUtil;

            // InkToShape
            ToggleSwitchEnableInkToShape.IsOn = SettingsModel.InkToShape.IsInkToShapeEnabled;

            // Automation
            ToggleSwitchAutoFoldInEasiNote.IsOn = SettingsModel.Automation.IsAutoFoldInEasiNote;
            ToggleSwitchAutoFoldInEasiCamera.IsOn = SettingsModel.Automation.IsAutoFoldInEasiCamera;
            ToggleSwitchAutoFoldInEasiNote3C.IsOn = SettingsModel.Automation.IsAutoFoldInEasiNote3C;
            ToggleSwitchAutoFoldInSeewoPincoTeacher.IsOn = SettingsModel.Automation.IsAutoFoldInSeewoPincoTeacher;
            ToggleSwitchAutoFoldInHiteTouchPro.IsOn = SettingsModel.Automation.IsAutoFoldInHiteTouchPro;
            ToggleSwitchAutoFoldInHiteCamera.IsOn = SettingsModel.Automation.IsAutoFoldInHiteCamera;
            ToggleSwitchAutoFoldInWxBoardMain.IsOn = SettingsModel.Automation.IsAutoFoldInWxBoardMain;
            ToggleSwitchAutoFoldInOldZyBoard.IsOn = SettingsModel.Automation.IsAutoFoldInOldZyBoard;
            ToggleSwitchAutoFoldInMSWhiteboard.IsOn = SettingsModel.Automation.IsAutoFoldInMSWhiteboard;
            ToggleSwitchAutoFoldInPPTSlideShow.IsOn = SettingsModel.Automation.IsAutoFoldInPPTSlideShow;
            ToggleSwitchAutoKillEasiNote.IsOn = SettingsModel.Automation.IsAutoKillEasiNote;
            ToggleSwitchAutoKillPptService.IsOn = SettingsModel.Automation.IsAutoKillPptService;
            ToggleSwitchAutoSaveStrokesAtClear.IsOn = SettingsModel.Automation.IsAutoSaveStrokesAtClear;
            ToggleSwitchSaveScreenshotsInDateFolders.IsOn = SettingsModel.Automation.IsSaveScreenshotsInDateFolders;
            ToggleSwitchAutoSaveStrokesAtScreenshot.IsOn = SettingsModel.Automation.IsAutoSaveStrokesAtScreenshot;
            SideControlMinimumAutomationSlider.Value = SettingsModel.Automation.MinimumAutomationStrokeNumber;
            AutoSavedStrokesLocation.Text = SettingsModel.Automation.AutoSavedStrokesLocation;
            ToggleSwitchAutoDelSavedFiles.IsOn = SettingsModel.Automation.AutoDelSavedFiles;
            ComboBoxAutoDelSavedFilesDaysThreshold.Text = SettingsModel.Automation.AutoDelSavedFilesDaysThreshold.ToString();
        }

    }
}
