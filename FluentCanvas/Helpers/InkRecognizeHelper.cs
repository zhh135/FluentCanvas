using FluentCanvas.InkAnalyzerCore;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace FluentCanvas.Helpers
{
    public static class InkRecognizeHelper
    {
        private static readonly InkShapeAnalyzer Analyzer = new InkShapeAnalyzer();

        public static async Task<ShapeRecognizeResult> RecognizeShapeAsync(
            StrokeCollection strokes,
            CancellationToken cancellationToken = default)
        {
            if (strokes == null || strokes.Count == 0)
                return null;

            var sourceStrokes = strokes.ToArray();
            var strokeData = sourceStrokes
                .Select((stroke, index) => new InkStrokeData(
                    index,
                    stroke.StylusPoints
                        .Select(point => new InkPointData(point.X, point.Y))
                        .ToImmutableArray()))
                .ToArray();

            var result = await Analyzer.AnalyzeLatestShapeAsync(strokeData, cancellationToken);
            if (result == null)
                return null;

            var recognizedStrokes = new StrokeCollection(
                result.SourceStrokeIds.Select(sourceId => sourceStrokes[sourceId]));

            return new ShapeRecognizeResult(
                result.Kind,
                new Point(result.Center.X, result.Center.Y),
                new Rect(result.Bounds.X, result.Bounds.Y, result.Bounds.Width, result.Bounds.Height),
                new PointCollection(result.Points.Select(point => new Point(point.X, point.Y))),
                recognizedStrokes);
        }
    }

    public sealed class ShapeRecognizeResult
    {
        public ShapeRecognizeResult(
            InkShapeKind kind,
            Point centroid,
            Rect bounds,
            PointCollection hotPoints,
            StrokeCollection strokes)
        {
            Kind = kind;
            Centroid = centroid;
            Bounds = bounds;
            HotPoints = hotPoints;
            Strokes = strokes;
        }

        public InkShapeKind Kind { get; }

        public string ShapeName => Kind.ToString();

        public Point Centroid { get; set; }

        public Rect Bounds { get; }

        public PointCollection HotPoints { get; }

        public StrokeCollection Strokes { get; }
    }

    public class Circle
    {
        public Circle(Point centroid, double r, Stroke stroke)
        {
            Centroid = centroid;
            R = r;
            Stroke = stroke;
        }

        public Point Centroid { get; set; }

        public double R { get; set; }

        public Stroke Stroke { get; set; }
    }
}
