using System;
using System.Windows.Controls;
using System.Windows.Ink;

namespace FluentCanvas.Services
{
    public sealed class InkBoardService : IInkBoardService
    {
        private InkCanvas inkCanvas;

        public event EventHandler StrokesChanged;

        public bool IsAttached => inkCanvas != null;

        public InkCanvas InkCanvas => inkCanvas;

        public StrokeCollection Strokes => inkCanvas?.Strokes;

        public int StrokeCount => inkCanvas?.Strokes.Count ?? 0;

        public void Attach(InkCanvas canvas)
        {
            if (ReferenceEquals(inkCanvas, canvas))
            {
                return;
            }

            if (inkCanvas != null)
            {
                inkCanvas.Strokes.StrokesChanged -= OnStrokesChanged;
            }

            inkCanvas = canvas;

            if (inkCanvas != null)
            {
                inkCanvas.Strokes.StrokesChanged += OnStrokesChanged;
            }
        }

        private void OnStrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            StrokesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
