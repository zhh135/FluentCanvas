using FluentCanvas.Helpers;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Diagnostics;
using System.Timers;
using Windows.Win32;

namespace FluentCanvas.Services
{
    public sealed class PowerPointService : IPowerPointService
    {
        private const string ProgId = "PowerPoint.Application";

        private readonly IInkBoardService inkBoard;
        private Timer pollTimer;
        private bool subscribed;

        public PowerPointService(IInkBoardService inkBoard)
        {
            this.inkBoard = inkBoard;
        }

        public bool IsConnected => Application != null;

        public Application Application { get; private set; }

        public Presentation Presentation { get; private set; }

        public Slides Slides { get; private set; }

        public int SlideCount { get; private set; }

        public int CurrentShowPosition { get; set; } = -1;

        public string PresentationName { get; private set; }

        public event EventHandler Connected;

        public event EventHandler SlideShowBegin;

        public event EventHandler SlideShowEnd;

        public event EventHandler SlideChanged;

        public event EventHandler PresentationClosed;

        public void StartPolling()
        {
            if (pollTimer == null)
            {
                pollTimer = new Timer(1000);
                pollTimer.Elapsed += OnPollElapsed;
            }

            pollTimer.Start();
        }

        public void StopPolling()
        {
            pollTimer?.Stop();
        }

        private void OnPollElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!MainWindow.Settings.PowerPointSettings.IsSupportWPS &&
                    Process.GetProcessesByName("wpp").Length > 0)
                {
                    return;
                }

                if (TryConnect())
                {
                    StopPolling();
                }
            }
            catch
            {
                // Polling must never throw on a background thread.
            }
        }

        public bool TryConnect()
        {
            var application = GetActiveComObject(ProgId) as Application;
            if (application == null)
            {
                return false;
            }

            Application = application;
            Presentation = application.ActivePresentation;
            Slides = Presentation.Slides;
            SlideCount = Slides.Count;
            PresentationName = Presentation.Name;
            CurrentShowPosition = -1;

            Subscribe();
            LogHelper.WriteLogToFile($"PowerPoint connected: {PresentationName} ({SlideCount} slides)", LogHelper.LogType.Event);

            Connected?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private void Subscribe()
        {
            if (subscribed || Application == null)
            {
                return;
            }

            Application.PresentationClose += OnPresentationClose;
            Application.SlideShowBegin += OnSlideShowBegin;
            Application.SlideShowNextSlide += OnSlideShowNextSlide;
            Application.SlideShowEnd += OnSlideShowEnd;
            subscribed = true;
        }

        public void Disconnect()
        {
            if (Application != null && subscribed)
            {
                try
                {
                    Application.PresentationClose -= OnPresentationClose;
                    Application.SlideShowBegin -= OnSlideShowBegin;
                    Application.SlideShowNextSlide -= OnSlideShowNextSlide;
                    Application.SlideShowEnd -= OnSlideShowEnd;
                }
                catch
                {
                    // Ignore teardown failures.
                }
            }

            subscribed = false;
            Application = null;
            Presentation = null;
            Slides = null;
            SlideCount = 0;
            PresentationName = null;
            CurrentShowPosition = -1;
        }

        public void NextSlide()
        {
            RunOnSlideShowWindow(window => window.View.Next());
        }

        public void PreviousSlide()
        {
            RunOnSlideShowWindow(window => window.View.Previous());
        }

        public void EndShow()
        {
            RunOnSlideShowWindow(window => window.View.Exit());
        }

        public void ShowSlideNavigation()
        {
            RunOnSlideShowWindow(window => window.SlideNavigation.Visible = true);
        }

        private void RunOnSlideShowWindow(Action<SlideShowWindow> action)
        {
            var application = Application;
            if (application == null)
            {
                return;
            }

            try
            {
                new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                {
                    try
                    {
                        var window = application.SlideShowWindows[1];
                        window.Activate();
                        action(window);
                    }
                    catch
                    {
                        // Without this catch, the app can crash when the show already ended.
                    }
                })).Start();
            }
            catch
            {
            }
        }

        private void OnPresentationClose(Presentation presentation)
        {
            Disconnect();
            StartPolling();
            PresentationClosed?.Invoke(this, EventArgs.Empty);
        }

        private void OnSlideShowBegin(SlideShowWindow window)
        {
            SlideShowBegin?.Invoke(this, EventArgs.Empty);
        }

        private void OnSlideShowNextSlide(SlideShowWindow window)
        {
            SlideChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSlideShowEnd(Presentation presentation)
        {
            SlideShowEnd?.Invoke(this, EventArgs.Empty);
        }

        private static unsafe object GetActiveComObject(string progId)
        {
            // PowerPoint/WPS may not be installed or running; return null instead of
            // throwing, because this is polled once per second.
            if (!PInvoke.CLSIDFromProgID(progId, out Guid clsid).Succeeded)
            {
                return null;
            }

            if (!PInvoke.GetActiveObject(clsid, null, out object value).Succeeded)
            {
                return null;
            }

            return value;
        }
    }
}
