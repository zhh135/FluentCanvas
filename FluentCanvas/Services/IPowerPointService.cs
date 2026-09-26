using Microsoft.Office.Interop.PowerPoint;
using System;

namespace FluentCanvas.Services
{
    /// <summary>
    /// Owns the PowerPoint/WPS COM connection, polling and navigation.
    /// UI orchestration stays in the shell, which subscribes to the events
    /// raised here instead of touching COM directly.
    /// </summary>
    public interface IPowerPointService
    {
        bool IsConnected { get; }

        Application Application { get; }

        Presentation Presentation { get; }

        Slides Slides { get; }

        int SlideCount { get; }

        int CurrentShowPosition { get; }

        string PresentationName { get; }

        void StartPolling();

        void StopPolling();

        bool TryConnect();

        void Disconnect();

        void NextSlide();

        void PreviousSlide();

        void EndShow();

        void ShowSlideNavigation();

        event EventHandler Connected;

        event EventHandler SlideShowBegin;

        event EventHandler SlideShowEnd;

        event EventHandler SlideChanged;

        event EventHandler PresentationClosed;
    }
}
