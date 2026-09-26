using FluentCanvas.Helpers;
using FluentCanvas.Services;
using FluentCanvas.ViewModels;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using File = System.IO.File;

namespace FluentCanvas
{
    public partial class MainWindow : Window
    {

        private void ToggleSwitchEnableNibMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            if (sender == ToggleSwitchEnableNibMode)
            {
                BoardToggleSwitchEnableNibMode.IsOn = ToggleSwitchEnableNibMode.IsOn;
            }
            else
            {
                ToggleSwitchEnableNibMode.IsOn = BoardToggleSwitchEnableNibMode.IsOn;
            }
            Settings.Startup.IsEnableNibMode = ToggleSwitchEnableNibMode.IsOn;

            if (Settings.Startup.IsEnableNibMode)
            {
                BoundsWidth = Settings.Advanced.NibModeBoundsWidth;
            }
            else
            {
                BoundsWidth = Settings.Advanced.FingerModeBoundsWidth;
            }
            SaveSettingsToFile();
        }

        private void ApplyScaling()
        {
            // Apply Floating Bar Scale
            // Divide by 100 to convert percentage to scale factor
            double floatingBarScaleFactor = Settings.Appearance.FloatingBarScale / 100.0;
            ViewboxFloatingBarScaleTransform.ScaleX = floatingBarScaleFactor;
            ViewboxFloatingBarScaleTransform.ScaleY = floatingBarScaleFactor;

            // Apply Blackboard Scale
            // Divide by 100 to convert percentage to scale factor
            double blackboardScaleFactor = Settings.Appearance.BlackboardScale / 100.0;
            ViewboxBlackboardLeftSideScaleTransform.ScaleX = blackboardScaleFactor;
            ViewboxBlackboardLeftSideScaleTransform.ScaleY = blackboardScaleFactor;
            ViewboxBlackboardCenterSideScaleTransform.ScaleX = blackboardScaleFactor;
            ViewboxBlackboardCenterSideScaleTransform.ScaleY = blackboardScaleFactor;
            ViewboxBlackboardRightSideScaleTransform.ScaleX = blackboardScaleFactor;
            ViewboxBlackboardRightSideScaleTransform.ScaleY = blackboardScaleFactor;

            // auto align
            ViewboxFloatingBarMarginAnimation();
        }

        private void ComboBoxPenStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            if (sender == ComboBoxPenStyle)
            {
                Settings.Canvas.InkStyle = ComboBoxPenStyle.SelectedIndex;
                BoardComboBoxPenStyle.SelectedIndex = ComboBoxPenStyle.SelectedIndex;
            }
            else
            {
                Settings.Canvas.InkStyle = BoardComboBoxPenStyle.SelectedIndex;
                ComboBoxPenStyle.SelectedIndex = BoardComboBoxPenStyle.SelectedIndex;
            }
            SaveSettingsToFile();
        }

        private void StartOrStoptimerCheckAutoFold()
        {
            if (Settings.Automation.IsEnableAutoFold)
            {
                timerCheckAutoFold.Start();
            }
            else
            {
                timerCheckAutoFold.Stop();
            }
        }

        private void ToggleSwitchEnableTwoFingerZoom_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            if (sender == ToggleSwitchEnableTwoFingerZoom)
            {
                BoardToggleSwitchEnableTwoFingerZoom.IsOn = ToggleSwitchEnableTwoFingerZoom.IsOn;
            }
            else
            {
                ToggleSwitchEnableTwoFingerZoom.IsOn = BoardToggleSwitchEnableTwoFingerZoom.IsOn;
            }
            Settings.Gesture.IsEnableTwoFingerZoom = ToggleSwitchEnableTwoFingerZoom.IsOn;
            CheckEnableTwoFingerGestureBtnColorPrompt();
            SaveSettingsToFile();
        }

        private void ToggleSwitchEnableMultiTouchMode_Toggled(object sender, RoutedEventArgs e)
        {
            //if (!isLoaded) return;
            if (sender == ToggleSwitchEnableMultiTouchMode)
            {
                BoardToggleSwitchEnableMultiTouchMode.IsOn = ToggleSwitchEnableMultiTouchMode.IsOn;
            }
            else
            {
                ToggleSwitchEnableMultiTouchMode.IsOn = BoardToggleSwitchEnableMultiTouchMode.IsOn;
            }
            if (ToggleSwitchEnableMultiTouchMode.IsOn)
            {
                if (!isInMultiTouchMode) BorderMultiTouchMode_MouseUp(null, null);
            }
            else
            {
                if (isInMultiTouchMode) BorderMultiTouchMode_MouseUp(null, null);
            }
            Settings.Gesture.IsEnableMultiTouchMode = ToggleSwitchEnableMultiTouchMode.IsOn;
            CheckEnableTwoFingerGestureBtnColorPrompt();
            SaveSettingsToFile();
        }

        private void ToggleSwitchEnableTwoFingerTranslate_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            if (sender == ToggleSwitchEnableTwoFingerTranslate)
            {
                BoardToggleSwitchEnableTwoFingerTranslate.IsOn = ToggleSwitchEnableTwoFingerTranslate.IsOn;
            }
            else
            {
                ToggleSwitchEnableTwoFingerTranslate.IsOn = BoardToggleSwitchEnableTwoFingerTranslate.IsOn;
            }
            Settings.Gesture.IsEnableTwoFingerTranslate = ToggleSwitchEnableTwoFingerTranslate.IsOn;
            CheckEnableTwoFingerGestureBtnColorPrompt();
            SaveSettingsToFile();
        }

        private void ToggleSwitchEnableTwoFingerRotation_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;

            if (sender == ToggleSwitchEnableTwoFingerRotation)
            {
                BoardToggleSwitchEnableTwoFingerRotation.IsOn = ToggleSwitchEnableTwoFingerRotation.IsOn;
            }
            else
            {
                ToggleSwitchEnableTwoFingerRotation.IsOn = BoardToggleSwitchEnableTwoFingerRotation.IsOn;
            }
            Settings.Gesture.IsEnableTwoFingerRotation = ToggleSwitchEnableTwoFingerRotation.IsOn;
            Settings.Gesture.IsEnableTwoFingerRotationOnSelection = SettingsPanel.ToggleSwitchEnableTwoFingerRotationOnSelection.IsOn;
            CheckEnableTwoFingerGestureBtnColorPrompt();
            SaveSettingsToFile();
        }

        public static void SetSettingsToRecommendation()
        {
            bool AutoDelSavedFilesDays = Settings.Automation.AutoDelSavedFiles;
            int AutoDelSavedFilesDaysThreshold = Settings.Automation.AutoDelSavedFilesDaysThreshold;
            Settings = new Settings();
            Settings.Advanced.IsSpecialScreen = true;
            Settings.Advanced.IsQuadIR = false;
            Settings.Advanced.TouchMultiplier = 0.3;
            Settings.Advanced.NibModeBoundsWidth = 5;
            Settings.Advanced.FingerModeBoundsWidth = 20;
            Settings.Advanced.NibModeBoundsWidthThresholdValue = 2.5;
            Settings.Advanced.FingerModeBoundsWidthThresholdValue = 2.5;
            Settings.Advanced.NibModeBoundsWidthEraserSize = 0.8;
            Settings.Advanced.FingerModeBoundsWidthEraserSize = 0.8;
            Settings.Advanced.IsLogEnabled = true;
            Settings.Advanced.IsSecondConfimeWhenShutdownApp = false;
            Settings.Advanced.IsEnableEdgeGestureUtil = false;

            Settings.Appearance.IsEnableDisPlayFloatBarText = false;
            Settings.Appearance.IsEnableDisPlayNibModeToggler = false;
            Settings.Appearance.IsColorfulViewboxFloatingBar = false;
            Settings.Appearance.FloatingBarScale = 100.0;
            Settings.Appearance.BlackboardScale = 100.0;
            Settings.Appearance.IsTransparentButtonBackground = true;
            Settings.Appearance.IsShowExitButton = true;
            Settings.Appearance.IsShowEraserButton = true;
            Settings.Appearance.IsShowHideControlButton = false;
            Settings.Appearance.IsShowLRSwitchButton = false;
            Settings.Appearance.IsShowModeFingerToggleSwitch = true;
            Settings.Appearance.Theme = 0;

            Settings.Automation.IsAutoFoldInEasiNote = true;
            Settings.Automation.IsAutoFoldInEasiNoteIgnoreDesktopAnno = true;
            Settings.Automation.IsAutoFoldInEasiCamera = true;
            Settings.Automation.IsAutoFoldInEasiNote3C = false;
            Settings.Automation.IsAutoFoldInSeewoPincoTeacher = false;
            Settings.Automation.IsAutoFoldInHiteTouchPro = false;
            Settings.Automation.IsAutoFoldInHiteCamera = false;
            Settings.Automation.IsAutoFoldInWxBoardMain = false;
            Settings.Automation.IsAutoFoldInOldZyBoard = false;
            Settings.Automation.IsAutoFoldInMSWhiteboard = false;
            Settings.Automation.IsAutoFoldInPPTSlideShow = false;
            Settings.Automation.IsAutoKillPptService = false;
            Settings.Automation.IsAutoKillEasiNote = false;
            Settings.Automation.IsSaveScreenshotsInDateFolders = false;
            Settings.Automation.IsAutoSaveStrokesAtScreenshot = true;
            Settings.Automation.IsAutoSaveStrokesAtClear = true;
            Settings.Automation.IsAutoClearWhenExitingWritingMode = false;
            Settings.Automation.MinimumAutomationStrokeNumber = 0;
            Settings.Automation.AutoDelSavedFiles = AutoDelSavedFilesDays;
            Settings.Automation.AutoDelSavedFilesDaysThreshold = AutoDelSavedFilesDaysThreshold;

            Settings.PowerPointSettings.IsShowPPTNavigationBottom = false;
            Settings.PowerPointSettings.IsShowBottomPPTNavigationPanel = true;
            Settings.PowerPointSettings.IsShowSidePPTNavigationPanel = true;
            Settings.PowerPointSettings.PowerPointSupport = true;
            Settings.PowerPointSettings.IsShowCanvasAtNewSlideShow = true;
            Settings.PowerPointSettings.IsNoClearStrokeOnSelectWhenInPowerPoint = true;
            Settings.PowerPointSettings.IsShowStrokeOnSelectInPowerPoint = false;
            Settings.PowerPointSettings.IsAutoSaveStrokesInPowerPoint = true;
            Settings.PowerPointSettings.IsAutoSaveScreenShotInPowerPoint = true;
            Settings.PowerPointSettings.IsNotifyPreviousPage = false;
            Settings.PowerPointSettings.IsNotifyHiddenPage = false;
            Settings.PowerPointSettings.IsNotifyAutoPlayPresentation = true;
            Settings.PowerPointSettings.IsEnableTwoFingerGestureInPresentationMode = false;
            Settings.PowerPointSettings.IsEnableFingerGestureSlideShowControl = false;
            Settings.PowerPointSettings.IsSupportWPS = true;

            Settings.Canvas.InkWidth = 2.5;
            Settings.Canvas.InkAlpha = 80;
            Settings.Canvas.IsShowCursor = false;
            Settings.Canvas.InkStyle = 0;
            Settings.Canvas.EraserSize = 1;
            Settings.Canvas.EraserType = 0;
            Settings.Canvas.HideStrokeWhenSelecting = false;
            Settings.Canvas.UsingWhiteboard = false;
            Settings.Canvas.HyperbolaAsymptoteOption = 0;

            Settings.Gesture.MatrixTransformCenterPoint = MatrixTransformCenterPointOptions.CanvasCenterPoint;
            Settings.Gesture.AutoSwitchTwoFingerGesture = true;
            Settings.Gesture.IsEnableTwoFingerTranslate = true;
            Settings.Gesture.IsEnableTwoFingerZoom = false;
            Settings.Gesture.IsEnableTwoFingerRotation = false;
            Settings.Gesture.IsEnableTwoFingerRotationOnSelection = true;

            Settings.InkToShape.IsInkToShapeEnabled = true;

            Settings.Startup.IsEnableNibMode = false;
            Settings.Startup.IsAutoUpdate = true;
            Settings.Startup.IsAutoUpdateWithSilence = true;
            Settings.Startup.AutoUpdateWithSilenceStartTime = "18:20";
            Settings.Startup.AutoUpdateWithSilenceEndTime = "07:40";
            Settings.Startup.IsFoldAtStartup = false;
        }

        private void ToggleSwitchEnableInkToShape_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.InkToShape.IsInkToShapeEnabled = SettingsPanel.ToggleSwitchEnableInkToShape.IsOn;
            SaveSettingsToFile();
        }

        public static void SaveSettingsToFile()
        {
            SettingsStorage.Write(SettingsStorage.Serialize(Settings));
        }
    }
}
