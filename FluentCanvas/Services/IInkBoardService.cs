using System;
using System.Windows.Controls;
using System.Windows.Ink;

namespace FluentCanvas.Services
{
    /// <summary>
    /// Bridges the single <see cref="InkCanvas"/> that stays in the shell
    /// (<c>MainWindow</c>) to the extracted parts, so they do not reference
    /// <c>MainWindow</c> directly.
    /// </summary>
    public interface IInkBoardService
    {
        bool IsAttached { get; }

        InkCanvas InkCanvas { get; }

        StrokeCollection Strokes { get; }

        int StrokeCount { get; }

        void Attach(InkCanvas inkCanvas);

        event EventHandler StrokesChanged;
    }
}
