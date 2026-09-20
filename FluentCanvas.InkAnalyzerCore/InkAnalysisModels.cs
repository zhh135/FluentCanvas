using System.Collections.Immutable;

namespace FluentCanvas.InkAnalyzerCore;

public readonly record struct InkPointData(double X, double Y);

public sealed record InkStrokeData(int SourceId, ImmutableArray<InkPointData> Points);

public readonly record struct PointData(double X, double Y);

public readonly record struct RectData(double X, double Y, double Width, double Height);

public enum InkShapeKind
{
    Unknown,
    Circle,
    Ellipse,
    Triangle,
    Rectangle,
    Square,
    Diamond,
    Trapezoid,
    Parallelogram,
    Quadrilateral,
    Pentagon,
    Hexagon
}

public sealed record InkShapeAnalysisResult(
    InkShapeKind Kind,
    PointData Center,
    RectData Bounds,
    ImmutableArray<PointData> Points,
    ImmutableArray<int> SourceStrokeIds);
