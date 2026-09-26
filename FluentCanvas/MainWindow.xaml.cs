using FluentCanvas.Helpers;
using FluentCanvas.Services;
using FluentCanvas.ViewModels;
using iNKORE.UI.WPF.Modern;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using File = System.IO.File;
using MessageBox = System.Windows.MessageBox;

namespace FluentCanvas
{
    public partial class MainWindow : Window
    {
        #region Window Initialization

        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            /*
                处于画板模式内：Topmost == false / currentMode != 0
                处于 PPT 放映内：BtnPPTSlideShowEnd.Visibility
            */
            InitializeComponent();

            ViewModel = Locator.Get<MainViewModel>();
            DataContext = ViewModel;

            Locator.Get<IInkBoardService>().Attach(inkCanvas);
            ShellService.Window = this;

            SettingsPanel.ToggleSwitchEnableTwoFingerRotationOnSelection.Toggled += ToggleSwitchEnableTwoFingerRotation_Toggled;
            SettingsPanel.ToggleSwitchEnableInkToShape.Toggled += ToggleSwitchEnableInkToShape_Toggled;

            BlackboardLeftSide.Visibility = Visibility.Collapsed;
            BlackboardCenterSide.Visibility = Visibility.Collapsed;
            BlackboardRightSide.Visibility = Visibility.Collapsed;

            BorderTools.Visibility = Visibility.Collapsed;
            SettingsPanel.Visibility = Visibility.Collapsed;

            BtnPPTSlideShowEnd.Visibility = Visibility.Collapsed;

            SettingsPanel.Margin = new Thickness(0, 150, 0, 150);

            TwoFingerGestureBorder.Visibility = Visibility.Collapsed;
            BoardTwoFingerGestureBorder.Visibility = Visibility.Collapsed;
            BorderDrawShape.Visibility = Visibility.Collapsed;
            BoardBorderDrawShape.Visibility = Visibility.Collapsed;

            GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

            ViewboxFloatingBar.Margin = new Thickness((SystemParameters.WorkArea.Width - 284) / 2, SystemParameters.WorkArea.Height - 60, -2000, -200);
            ViewboxFloatingBarMarginAnimation();

            try
            {
                if (File.Exists("Log.txt"))
                {
                    FileInfo fileInfo = new FileInfo("Log.txt");
                    long fileSizeInKB = fileInfo.Length / 1024;
                    if (fileSizeInKB > 512)
                    {
                        try
                        {
                            File.Delete("Log.txt");
                            LogHelper.WriteLogToFile("The Log.txt file has been successfully deleted. Original file size: " + fileSizeInKB + " KB", LogHelper.LogType.Info);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLogToFile(ex + " | Can not delete the Log.txt file. File size: " + fileSizeInKB + " KB", LogHelper.LogType.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
            }

            InitTimers();
            InitPowerPoint();
            timeMachine.OnRedoStateChanged += TimeMachine_OnRedoStateChanged;
            timeMachine.OnUndoStateChanged += TimeMachine_OnUndoStateChanged;
            inkCanvas.Strokes.StrokesChanged += StrokesOnStrokesChanged;

            Microsoft.Win32.SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            try
            {
                if (File.Exists("SpecialVersion.ini")) SettingsPanel.SpecialVersionReset();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
            }

            CheckColorTheme(true);
        }

        #endregion

        #region FluentCanvas Functions

        DrawingAttributes drawingAttributes;
        private void loadPenCanvas()
        {
            try
            {
                drawingAttributes = inkCanvas.DefaultDrawingAttributes;
                drawingAttributes.Color = Colors.Red;

                drawingAttributes.Height = 2.5;
                drawingAttributes.Width = 2.5;

                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                inkCanvas.Gesture += InkCanvas_Gesture;
            }
            catch { }
        }

        private void InkCanvas_Gesture(object sender, InkCanvasGestureEventArgs e)
        {
            ReadOnlyCollection<GestureRecognitionResult> gestures = e.GetGestureRecognitionResults();
            try
            {
                foreach (GestureRecognitionResult gest in gestures)
                {
                    if (BtnPPTSlideShowEnd.Visibility == Visibility.Visible)
                    {
                        if (gest.ApplicationGesture == ApplicationGesture.Left)
                        {
                            BtnPPTSlidesDown_Click(null, null);
                        }
                        if (gest.ApplicationGesture == ApplicationGesture.Right)
                        {
                            BtnPPTSlidesUp_Click(null, null);
                        }
                    }
                }
            }
            catch { }
        }

        private void inkCanvas_EditingModeChanged(object sender, RoutedEventArgs e)
        {
            var inkCanvas1 = sender as InkCanvas;
            if (inkCanvas1 == null) return;
            if (Settings.Canvas.IsShowCursor)
            {
                if (inkCanvas1.EditingMode == InkCanvasEditingMode.Ink || drawingShapeMode != 0)
                {
                    inkCanvas1.ForceCursor = true;
                }
                else
                {
                    inkCanvas1.ForceCursor = false;
                }
            }
            else
            {
                inkCanvas1.ForceCursor = false;
            }
            if (inkCanvas1.EditingMode == InkCanvasEditingMode.Ink) forcePointEraser = !forcePointEraser;
        }

        #endregion FluentCanvas Functions

        #region Definations and Loading

        public static Settings Settings = new Settings();
        public static string settingsFileName = "Settings.json";
        bool isLoaded = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadPenCanvas();
            //加载设置
            LoadSettings(true);
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            SystemEvents_UserPreferenceChanged(null, null);

            SettingsPanel.AppVersionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            LogHelper.WriteLogToFile("FluentCanvas Loaded", LogHelper.LogType.Event);
            isLoaded = true;
            RegisterGlobalHotkeys();
            OpenActivationFile();
        }

        private void OpenActivationFile()
        {
            var filePath = App.TakePendingActivationFile();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return;
            }

            try
            {
                LoadInkCanvasFile(filePath);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"Failed to open activated file '{filePath}': {ex}", LogHelper.LogType.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogHelper.WriteLogToFile("FluentCanvas closing", LogHelper.LogType.Event);
            if (!CloseIsFromButton && Settings.Advanced.IsSecondConfimeWhenShutdownApp)
            {
                e.Cancel = true;
                if (MessageBox.Show("是否继续关闭 FluentCanvas 画板，这将丢失当前未保存的工作。", "FluentCanvas 画板", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    if (MessageBox.Show("真的狠心关闭 FluentCanvas 画板吗？", "FluentCanvas 画板", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        if (MessageBox.Show("是否取消关闭 FluentCanvas 画板？", "FluentCanvas 画板", MessageBoxButton.OKCancel, MessageBoxImage.Error) != MessageBoxResult.OK)
                        {
                            e.Cancel = false;
                        }
                    }
                }
            }
            if (e.Cancel)
            {
                LogHelper.WriteLogToFile("FluentCanvas closing cancelled", LogHelper.LogType.Event);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            LogHelper.WriteLogToFile("FluentCanvas closed", LogHelper.LogType.Event);
        }

        #endregion Definations and Loading
    }
}
