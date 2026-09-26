using FluentCanvas.Helpers;
using FluentCanvas.Services;
using FluentCanvas.ViewModels;
using FluentCanvas.Views;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Application = System.Windows.Application;
using File = System.IO.File;
using Microsoft.Office.Core;

namespace FluentCanvas
{
    public partial class MainWindow : Window
    {
        private IPowerPointService pptService;

        /// <summary>Resolved lazily because the view registers itself when loaded.</summary>
        private IPptNavigationView PptNavView => Locator.Get<IAppViewService>().Get<IPptNavigationView>();

        public static bool isWPSSupportOn => Settings.PowerPointSettings.IsSupportWPS;

        public static bool IsShowingRestoreHiddenSlidesWindow = false;

        /// <summary>
        /// Wires the shell to the PowerPoint service. The service owns the COM
        /// connection and polling; the shell only reacts to its events.
        /// </summary>
        private void InitPowerPoint()
        {
            pptService = Locator.Get<IPowerPointService>();
            pptService.Connected += OnPowerPointConnected;
            pptService.SlideShowBegin += OnPowerPointSlideShowBegin;
            pptService.SlideChanged += OnPowerPointSlideChanged;
            pptService.SlideShowEnd += OnPowerPointSlideShowEnd;
            pptService.PresentationClosed += OnPowerPointPresentationClosed;

            var pptNavViewModel = Locator.Get<PptNavigationViewModel>();
            pptNavViewModel.NextRequested += (s, e) => BtnPPTSlidesDown_Click(null, null);
            pptNavViewModel.PreviousRequested += (s, e) => BtnPPTSlidesUp_Click(null, null);
            pptNavViewModel.NavigationToggleRequested += OnPptNavigationToggleRequested;
        }

        private async void OnPptNavigationToggleRequested(object sender, EventArgs e)
        {
            Main_Grid.Background = new SolidColorBrush(StringToColor("#01FFFFFF"));
            CursorIcon_Click(null, null);
            pptService.ShowSlideNavigation();
            if (!isFloatingBarFolded)
            {
                await Task.Delay(100);
                ViewboxFloatingBarMarginAnimation();
            }
        }

        private void OnPowerPointConnected(object sender, EventArgs e)
        {
            var presentation = pptService.Presentation;
            if (presentation == null) return;

            memoryStreams = new MemoryStream[pptService.SlideCount + 2];

            LogHelper.NewLog("Name: " + pptService.PresentationName);
            LogHelper.NewLog("Slides Count: " + pptService.SlideCount.ToString());

            try
            {
                // 跳转到上次播放页
                if (Settings.PowerPointSettings.IsNotifyPreviousPage)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        string folderPath = Settings.Automation.AutoSavedStrokesLocation + @"\Auto Saved - Presentations\" + presentation.Name + "_" + presentation.Slides.Count;
                        try
                        {
                            if (File.Exists(folderPath + "/Position"))
                            {
                                if (int.TryParse(File.ReadAllText(folderPath + "/Position"), out var page))
                                {
                                    if (page <= 0) return;
                                    new YesOrNoNotificationWindow($"上次播放到了第 {page} 页, 是否立即跳转", () =>
                                    {
                                        if (pptService.Application.SlideShowWindows.Count >= 1)
                                        {
                                            presentation.SlideShowWindow.View.GotoSlide(page);
                                        }
                                        else
                                        {
                                            presentation.Windows[1].View.GotoSlide(page);
                                        }
                                    }).ShowDialog();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLogToFile(ex.ToString(), LogHelper.LogType.Error);
                        }
                    }));
                }

                //检查是否有隐藏幻灯片
                if (Settings.PowerPointSettings.IsNotifyHiddenPage)
                {
                    bool isHaveHiddenSlide = false;
                    foreach (Slide slide in pptService.Slides)
                    {
                        if (slide.SlideShowTransition.Hidden == MsoTriState.msoTrue)
                        {
                            isHaveHiddenSlide = true;
                            break;
                        }
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (isHaveHiddenSlide && !IsShowingRestoreHiddenSlidesWindow)
                        {
                            IsShowingRestoreHiddenSlidesWindow = true;
                            new YesOrNoNotificationWindow("检测到此演示文档中包含隐藏的幻灯片，是否取消隐藏？",
                                () =>
                                {
                                    foreach (Slide slide in pptService.Slides)
                                    {
                                        if (slide.SlideShowTransition.Hidden == MsoTriState.msoTrue)
                                        {
                                            slide.SlideShowTransition.Hidden = MsoTriState.msoFalse;
                                        }
                                    }
                                }).ShowDialog();
                        }
                    }));
                }

                //检测是否有自动播放
                if (Settings.PowerPointSettings.IsNotifyAutoPlayPresentation
                    && BtnPPTSlideShowEnd.Visibility != Visibility.Visible)
                {
                    bool hasSlideTimings = false;
                    foreach (Slide slide in pptService.Slides)
                    {
                        if (slide.SlideShowTransition.AdvanceOnTime == MsoTriState.msoTrue && slide.SlideShowTransition.AdvanceTime > 0)
                        {
                            hasSlideTimings = true;
                            break;
                        }
                    }
                    if (hasSlideTimings)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            new YesOrNoNotificationWindow("检测到此演示文档中自动播放或排练计时已经启用，可能导致幻灯片自动翻页，是否取消？",
                                () =>
                                {
                                    presentation.SlideShowSettings.AdvanceMode = PpSlideShowAdvanceMode.ppSlideShowManualAdvance;
                                }).ShowDialog();
                        }));
                        presentation.SlideShowSettings.AdvanceMode = PpSlideShowAdvanceMode.ppSlideShowManualAdvance;
                    }
                }

                //如果检测到已经开始放映，则立即进入画板模式
                if (pptService.Application.SlideShowWindows.Count >= 1)
                {
                    OnPowerPointSlideShowBegin(this, EventArgs.Empty);
                }
            }
            catch
            {
                pptService.StartPolling();
            }
        }

        private void OnPowerPointPresentationClosed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BtnPPTSlideShowEnd.Visibility = Visibility.Collapsed;
            });
        }

        private string pptName = null;
        int currentShowPosition = -1;

        private void OnPowerPointSlideShowBegin(object sender, EventArgs e)
        {
            if (Settings.Automation.IsAutoFoldInPPTSlideShow && !isFloatingBarFolded)
            {
                FoldFloatingBar_Click(null, null);
            }
            else if (isFloatingBarFolded)
            {
                UnFoldFloatingBar_MouseUp(null, null);
            }

            var Wn = pptService.Application.SlideShowWindows[1];
            var presentation = pptService.Presentation;

            LogHelper.WriteLogToFile("PowerPoint Application Slide Show Begin", LogHelper.LogType.Event);
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (currentMode != 0)
                {
                    ImageBlackboard_Click(null, null);
                }

                lastDesktopInkColor = 1;

                previousSlideID = 0;
                memoryStreams = new MemoryStream[pptService.SlideCount + 2];

                pptName = presentation.Name;
                LogHelper.NewLog("Name: " + presentation.Name);
                LogHelper.NewLog("Slides Count: " + pptService.SlideCount.ToString());

                //检查是否有已有墨迹，并加载
                if (Settings.PowerPointSettings.IsAutoSaveStrokesInPowerPoint)
                {
                    if (Directory.Exists(Settings.Automation.AutoSavedStrokesLocation + @"\Auto Saved - Presentations\" + presentation.Name + "_" + presentation.Slides.Count))
                    {
                        LogHelper.WriteLogToFile("Found saved strokes", LogHelper.LogType.Trace);
                        FileInfo[] files = new DirectoryInfo(Settings.Automation.AutoSavedStrokesLocation + @"\Auto Saved - Presentations\" + presentation.Name + "_" + presentation.Slides.Count).GetFiles();
                        int count = 0;
                        foreach (FileInfo file in files)
                        {
                            if (file.Name != "Position")
                            {
                                int i = -1;
                                try
                                {
                                    i = int.Parse(System.IO.Path.GetFileNameWithoutExtension(file.Name));
                                    memoryStreams[i] = new MemoryStream(File.ReadAllBytes(file.FullName));
                                    memoryStreams[i].Position = 0;
                                    count++;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.WriteLogToFile(string.Format("Failed to load strokes on Slide {0}\n{1}", i, ex.ToString()), LogHelper.LogType.Error);
                                }
                            }
                        }
                        LogHelper.WriteLogToFile(string.Format("Loaded {0} saved strokes", count.ToString()));
                    }
                }

                BtnPPTSlideShowEnd.Visibility = Visibility.Visible;

                PptNavView.ShowPanels(
                    Settings.PowerPointSettings.IsShowBottomPPTNavigationPanel,
                    Settings.PowerPointSettings.IsShowSidePPTNavigationPanel);

                if (Settings.Appearance.IsColorfulViewboxFloatingBar)
                {
                    ViewboxFloatingBar.Opacity = 0.8;
                }
                else
                {
                    ViewboxFloatingBar.Opacity = 0.75;
                }

                if (Settings.PowerPointSettings.IsShowCanvasAtNewSlideShow && Main_Grid.Background == Brushes.Transparent)
                {
                    if (currentMode != 0)
                    {
                        currentMode = 0;
                        GridBackgroundCover.Visibility = Visibility.Collapsed;
                        AnimationsHelper.HideWithSlideAndFade(BlackboardLeftSide);
                        AnimationsHelper.HideWithSlideAndFade(BlackboardCenterSide);
                        AnimationsHelper.HideWithSlideAndFade(BlackboardRightSide);

                        ClearStrokes(true);
                    }
                    BtnHideInkCanvas_Click(null, null);
                }

                ClearStrokes(true);

                BorderFloatingBarMainControls.Visibility = Visibility.Visible;

                if (Settings.PowerPointSettings.IsShowCanvasAtNewSlideShow)
                {
                    BtnColorRed_Click(null, null);
                }

                isEnteredSlideShowEndEvent = false;
                PptNavView.SetPosition(Wn.View.CurrentShowPosition, pptService.SlideCount);
                LogHelper.NewLog("PowerPoint Slide Show Loading process complete");

                new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(100);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ViewboxFloatingBarMarginAnimation();
                    });
                })).Start();
            });
        }

        bool isEnteredSlideShowEndEvent = false; //防止重复调用本函数导致墨迹保存失效

        private async void OnPowerPointSlideShowEnd(object sender, EventArgs e)
        {
            var Pres = pptService.Presentation;

            if (isFloatingBarFolded) UnFoldFloatingBar_MouseUp(null, null);

            LogHelper.WriteLogToFile(string.Format("PowerPoint Slide Show End"), LogHelper.LogType.Event);
            if (isEnteredSlideShowEndEvent)
            {
                LogHelper.WriteLogToFile("Detected previous entrance, returning");
                return;
            }
            isEnteredSlideShowEndEvent = true;
            if (Settings.PowerPointSettings.IsAutoSaveStrokesInPowerPoint && Pres != null)
            {
                string folderPath = Settings.Automation.AutoSavedStrokesLocation + @"\Auto Saved - Presentations\" + Pres.Name + "_" + Pres.Slides.Count;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                try
                {
                    File.WriteAllText(folderPath + "/Position", previousSlideID.ToString());
                }
                catch { }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        MemoryStream ms = new MemoryStream();
                        inkCanvas.Strokes.Save(ms);
                        ms.Position = 0;
                        memoryStreams[currentShowPosition] = ms;
                    }
                    catch { }
                });
                for (int i = 1; i <= Pres.Slides.Count; i++)
                {
                    if (memoryStreams[i] != null)
                    {
                        try
                        {
                            string baseFilePath = folderPath + @"\" + i.ToString("0000");
                            string icartFilePath = baseFilePath + ".icart";
                            string icstkFilePath = baseFilePath + ".icstk";

                            if (memoryStreams[i].Length > 8)
                            {
                                byte[] srcBuf = new byte[memoryStreams[i].Length];
                                int byteLength = memoryStreams[i].Read(srcBuf, 0, srcBuf.Length);

                                if (File.Exists(icartFilePath))
                                {
                                    File.WriteAllBytes(icartFilePath, srcBuf);
                                    LogHelper.WriteLogToFile(string.Format("Saved strokes for Slide {0} as .icart, size={1}, byteLength={2}", i.ToString(), memoryStreams[i].Length, byteLength));
                                }
                                else
                                {
                                    File.WriteAllBytes(icstkFilePath, srcBuf);
                                    LogHelper.WriteLogToFile(string.Format("Saved strokes for Slide {0} as .icstk, size={1}, byteLength={2}", i.ToString(), memoryStreams[i].Length, byteLength));
                                }
                            }
                            else
                            {
                                File.Delete(icartFilePath);
                                File.Delete(icstkFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLogToFile(string.Format("Failed to save strokes for Slide {0}\n{1}", i, ex.ToString()), LogHelper.LogType.Error);
                            File.Delete(folderPath + @"\" + i.ToString("0000") + ".icstk");
                        }
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                BtnPPTSlideShowEnd.Visibility = Visibility.Collapsed;
                PptNavView.HidePanels();

                if (currentMode != 0)
                {
                    ImageBlackboard_Click(null, null);
                }

                ClearStrokes(true);

                if (Main_Grid.Background != Brushes.Transparent)
                {
                    BtnHideInkCanvas_Click(null, null);
                }

                if (Settings.Appearance.IsColorfulViewboxFloatingBar)
                {
                    ViewboxFloatingBar.Opacity = 0.95;
                }
                else
                {
                    ViewboxFloatingBar.Opacity = 1;
                }
            });

            await Task.Delay(150);
            ViewboxFloatingBarMarginAnimation();
        }

        int previousSlideID = 0;
        MemoryStream[] memoryStreams = new MemoryStream[50];

        private void OnPowerPointSlideChanged(object sender, EventArgs e)
        {
            var Wn = pptService.Application.SlideShowWindows[1];
            LogHelper.WriteLogToFile(string.Format("PowerPoint Next Slide (Slide {0})", Wn.View.CurrentShowPosition), LogHelper.LogType.Event);
            if (Wn.View.CurrentShowPosition != previousSlideID)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MemoryStream ms = new MemoryStream();
                    inkCanvas.Strokes.Save(ms);
                    ms.Position = 0;
                    memoryStreams[previousSlideID] = ms;

                    if (inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber && Settings.PowerPointSettings.IsAutoSaveScreenShotInPowerPoint && !_isPptClickingBtnTurned)
                        SavePPTScreenshot(Wn.Presentation.Name + "/" + Wn.View.CurrentShowPosition);
                    _isPptClickingBtnTurned = false;

                    ClearStrokes(true);
                    timeMachine.ClearStrokeHistory();

                    try
                    {
                        if (memoryStreams[Wn.View.CurrentShowPosition] != null && memoryStreams[Wn.View.CurrentShowPosition].Length > 0)
                        {
                            inkCanvas.Strokes.Add(new StrokeCollection(memoryStreams[Wn.View.CurrentShowPosition]));
                        }
                        currentShowPosition = Wn.View.CurrentShowPosition;
                    }
                    catch { }

                        PptNavView.SetPosition(Wn.View.CurrentShowPosition, Wn.Presentation.Slides.Count);
                });
                previousSlideID = Wn.View.CurrentShowPosition;
            }
        }

        private bool _isPptClickingBtnTurned = false;

        private void BtnPPTSlidesUp_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == 1)
            {
                GridBackgroundCover.Visibility = Visibility.Collapsed;
                AnimationsHelper.HideWithSlideAndFade(BlackboardLeftSide);
                AnimationsHelper.HideWithSlideAndFade(BlackboardCenterSide);
                AnimationsHelper.HideWithSlideAndFade(BlackboardRightSide);
                currentMode = 0;
            }

            _isPptClickingBtnTurned = true;

            if (inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber &&
                Settings.PowerPointSettings.IsAutoSaveScreenShotInPowerPoint &&
                pptService.Application?.SlideShowWindows.Count >= 1)
                SavePPTScreenshot(pptService.Application.SlideShowWindows[1].Presentation.Name + "/" + pptService.Application.SlideShowWindows[1].View.CurrentShowPosition);

            pptService.PreviousSlide();
        }

        private void BtnPPTSlidesDown_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == 1)
            {
                GridBackgroundCover.Visibility = Visibility.Collapsed;
                AnimationsHelper.HideWithSlideAndFade(BlackboardLeftSide);
                AnimationsHelper.HideWithSlideAndFade(BlackboardCenterSide);
                AnimationsHelper.HideWithSlideAndFade(BlackboardRightSide);
                currentMode = 0;
            }
            _isPptClickingBtnTurned = true;
            if (inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber &&
                Settings.PowerPointSettings.IsAutoSaveScreenShotInPowerPoint &&
                pptService.Application?.SlideShowWindows.Count >= 1)
                SavePPTScreenshot(pptService.Application.SlideShowWindows[1].Presentation.Name + "/" + pptService.Application.SlideShowWindows[1].View.CurrentShowPosition);

            pptService.NextSlide();
        }

        private async void BtnPPTSlideShowEnd_Click(object sender, RoutedEventArgs e)
        {
            pptService.EndShow();

            HideSubPanels("cursor");
            await Task.Delay(150);
            ViewboxFloatingBarMarginAnimation();
        }
    }
}
