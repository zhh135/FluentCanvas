using FluentCanvas.Helpers;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace FluentCanvas
{
    public partial class MainWindow : Window
    {
        private void LoadSettings(bool isStartup = false)
        {
            try
            {
                if (!ViewModel.Settings.Load())
                {
                    SettingsPanel.ResetToSuggestion();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
            }

            if (Settings.Startup == null) Settings.Startup = new Startup();
            if (Settings.Appearance == null) Settings.Appearance = new Appearance();
            if (Settings.PowerPointSettings == null) Settings.PowerPointSettings = new PowerPointSettings();
            if (Settings.Gesture == null) Settings.Gesture = new Gesture();
            if (Settings.Canvas == null) Settings.Canvas = new Canvas();
            if (Settings.Advanced == null) Settings.Advanced = new Advanced();
            if (Settings.InkToShape == null) Settings.InkToShape = new InkToShape();
            if (Settings.RandSettings == null) Settings.RandSettings = new RandSettings();
            if (Settings.Automation == null) Settings.Automation = new Automation();

            // Panel state is owned by the settings view.
            SettingsPanel.LoadFromSettings();

            // Startup (shell)
            if (isStartup)
            {
                CursorIcon_Click(null, null);

                if (Settings.Automation.AutoDelSavedFiles)
                {
                    DelAutoSavedFiles.DeleteFilesOlder(Settings.Automation.AutoSavedStrokesLocation, Settings.Automation.AutoDelSavedFilesDaysThreshold);
                }
                if (Settings.Startup.IsFoldAtStartup)
                {
                    FoldFloatingBar_Click(Fold_Icon, null);
                }
            }
            if (Settings.Startup.IsEnableNibMode)
            {
                ToggleSwitchEnableNibMode.IsOn = true;
                BoardToggleSwitchEnableNibMode.IsOn = true;
                BoundsWidth = Settings.Advanced.NibModeBoundsWidth;
            }
            else
            {
                ToggleSwitchEnableNibMode.IsOn = false;
                BoardToggleSwitchEnableNibMode.IsOn = false;
                BoundsWidth = Settings.Advanced.FingerModeBoundsWidth;
            }
            if (Settings.Startup.IsAutoUpdate)
            {
                AutoUpdate();
            }

            // Appearance (shell)
            if (Settings.Appearance.IsEnableDisPlayFloatBarText)
            {
                FloatBarSelectIconTextBlock.Visibility = Visibility.Visible;
                Icon_Pen.Height = 22;
                Icon_Eraser1.Height = 22;
                Icon_Eraser2.Height = 22;
                Icon_Eraser2.Margin = new Thickness(5, -22, 0, -8);
                Icon_EraserByStrokes1.Height = 22;
                Icon_EraserByStrokes2.Height = 22;
                Icon_EraserByStrokes2.Margin = new Thickness(12, -22, 0, -8);
                Icon_Select1.Height = 22;
                Icon_Select2.Height = 22;
                Icon_Select2.Margin = new Thickness(6, -18, 0, -8);
                Icon_Undo.Margin = new Thickness(0, 1.5, 0, -1.5);
                Icon_Redo.Margin = new Thickness(0, 1.5, 0, -1.5);
            }
            else
            {
                FloatBarSelectIconTextBlock.Visibility = Visibility.Collapsed;
                Icon_Pen.Height = 32;
                Icon_Eraser1.Height = 32;
                Icon_Eraser2.Height = 32;
                Icon_Eraser2.Margin = new Thickness(5, -32, 0, -8);
                Icon_EraserByStrokes1.Height = 32;
                Icon_EraserByStrokes2.Height = 32;
                Icon_EraserByStrokes2.Margin = new Thickness(12, -32, 0, -8);
                Icon_Select1.Height = 32;
                Icon_Select2.Height = 32;
                Icon_Select2.Margin = new Thickness(6, -28, 0, -8);
                Icon_Undo.Margin = new Thickness(0);
                Icon_Redo.Margin = new Thickness(0);
            }
            if (Settings.Appearance.IsEnableDisPlayNibModeToggler)
            {
                NibModeSimpleStackPanel.Visibility = Visibility.Visible;
                BoardNibModeSimpleStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                NibModeSimpleStackPanel.Visibility = Visibility.Collapsed;
                BoardNibModeSimpleStackPanel.Visibility = Visibility.Collapsed;
            }

            SystemEvents_UserPreferenceChanged(null, null);

            if (Settings.Appearance.IsColorfulViewboxFloatingBar)
            {
                LinearGradientBrush gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new Point(0, 0);
                gradientBrush.EndPoint = new Point(1, 1);
                GradientStop blueStop = new GradientStop(Color.FromArgb(0x95, 0x80, 0xB0, 0xFF), 0);
                GradientStop greenStop = new GradientStop(Color.FromArgb(0x95, 0xC0, 0xFF, 0xC0), 1);
                gradientBrush.GradientStops.Add(blueStop);
                gradientBrush.GradientStops.Add(greenStop);
                EnableTwoFingerGestureBorder.Background = gradientBrush;
                BorderFloatingBarMainControls.Background = gradientBrush;
                BorderFloatingBarMoveControls.Background = gradientBrush;
                BtnPPTSlideShowEnd.Background = gradientBrush;
            }
            ApplyScaling();

            // PowerPointSettings (shell)
            if (Settings.PowerPointSettings.PowerPointSupport)
            {
                pptService.StartPolling();
            }
            else
            {
                pptService.StopPolling();
            }

            // Gesture (shell)
            ToggleSwitchEnableMultiTouchMode.IsOn = Settings.Gesture.IsEnableMultiTouchMode;
            ToggleSwitchEnableTwoFingerZoom.IsOn = Settings.Gesture.IsEnableTwoFingerZoom;
            BoardToggleSwitchEnableTwoFingerZoom.IsOn = Settings.Gesture.IsEnableTwoFingerZoom;
            ToggleSwitchEnableTwoFingerTranslate.IsOn = Settings.Gesture.IsEnableTwoFingerTranslate;
            BoardToggleSwitchEnableTwoFingerTranslate.IsOn = Settings.Gesture.IsEnableTwoFingerTranslate;
            ToggleSwitchEnableTwoFingerRotation.IsOn = Settings.Gesture.IsEnableTwoFingerRotation;
            BoardToggleSwitchEnableTwoFingerRotation.IsOn = Settings.Gesture.IsEnableTwoFingerRotation;
            if (Settings.Gesture.AutoSwitchTwoFingerGesture)
            {
                if (Topmost)
                {
                    ToggleSwitchEnableTwoFingerTranslate.IsOn = false;
                    BoardToggleSwitchEnableTwoFingerTranslate.IsOn = false;
                    Settings.Gesture.IsEnableTwoFingerTranslate = false;
                    if (!isInMultiTouchMode) ToggleSwitchEnableMultiTouchMode.IsOn = true;
                }
                else
                {
                    ToggleSwitchEnableTwoFingerTranslate.IsOn = true;
                    BoardToggleSwitchEnableTwoFingerTranslate.IsOn = true;
                    Settings.Gesture.IsEnableTwoFingerTranslate = true;
                    if (isInMultiTouchMode) ToggleSwitchEnableMultiTouchMode.IsOn = false;
                }
            }
            CheckEnableTwoFingerGestureBtnColorPrompt();

            // Canvas (shell)
            drawingAttributes.Height = Settings.Canvas.InkWidth;
            drawingAttributes.Width = Settings.Canvas.InkWidth;

            InkWidthSlider.Value = Settings.Canvas.InkWidth * 2;
            BoardInkWidthSlider.Value = Settings.Canvas.InkWidth * 2;
            InkAlphaSlider.Value = Settings.Canvas.InkAlpha;
            BoardInkAlphaSlider.Value = Settings.Canvas.InkAlpha;

            if (Settings.Canvas.UsingWhiteboard)
            {
                GridBackgroundCover.Background = new SolidColorBrush(StringToColor("#FFF2F2F2"));
                lastBoardInkColor = 0;
            }
            else
            {
                GridBackgroundCover.Background = new SolidColorBrush(StringToColor("#FF1F1F1F"));
                lastBoardInkColor = 5;
            }

            inkCanvas.ForceCursor = Settings.Canvas.IsShowCursor;

            ComboBoxPenStyle.SelectedIndex = Settings.Canvas.InkStyle;
            BoardComboBoxPenStyle.SelectedIndex = Settings.Canvas.InkStyle;

            // Advanced (shell)
            if (Settings.Advanced.IsEnableEdgeGestureUtil)
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10)) EdgeGestureUtil.DisableEdgeGestures(new WindowInteropHelper(this).Handle, true);
            }

            // Automation (shell)
            StartOrStoptimerCheckAutoFold();
            if (Settings.Automation.IsAutoKillEasiNote || Settings.Automation.IsAutoKillPptService)
            {
                timerKillProcess.Start();
            }
            else
            {
                timerKillProcess.Stop();
            }

            ViewboxFloatingBarMarginAnimation();
        }
    }
}
