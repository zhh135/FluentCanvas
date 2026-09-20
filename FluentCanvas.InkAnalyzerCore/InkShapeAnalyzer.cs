using System.Collections.Immutable;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;

namespace FluentCanvas.InkAnalyzerCore;

public sealed class InkShapeAnalyzer
{
    public async Task<InkShapeAnalysisResult?> AnalyzeLatestShapeAsync(
        IReadOnlyList<InkStrokeData> strokes,
        CancellationToken cancellationToken = default)
    {
        if (strokes.Count == 0)
        {
            return null;
        }

        for (var startIndex = 0; startIndex < strokes.Count; startIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await AnalyzeAsync(strokes, startIndex, strokes[^1].SourceId, cancellationToken);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static async Task<InkShapeAnalysisResult?> AnalyzeAsync(
        IReadOnlyList<InkStrokeData> strokes,
        int startIndex,
        int requiredSourceId,
        CancellationToken cancellationToken)
    {
        var analyzer = new InkAnalyzer();
        var sourceIdsByInkStrokeId = new Dictionary<uint, int>();
        var builder = new InkStrokeBuilder();

        for (var index = startIndex; index < strokes.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourceStroke = strokes[index];
            if (sourceStroke.Points.Length < 2)
            {
                continue;
            }

            var points = sourceStroke.Points.Select(point => new Point((float)point.X, (float)point.Y));
            var inkStroke = builder.CreateStroke(points);
            sourceIdsByInkStrokeId[inkStroke.Id] = sourceStroke.SourceId;
            analyzer.AddDataForStroke(inkStroke);
            analyzer.SetStrokeDataKind(inkStroke.Id, InkAnalysisStrokeKind.Drawing);
        }

        if (sourceIdsByInkStrokeId.Count == 0)
        {
            return null;
        }

        var status = await analyzer.AnalyzeAsync();
        cancellationToken.ThrowIfCancellationRequested();
        if (status.Status != InkAnalysisStatus.Updated)
        {
            return null;
        }

        return analyzer.AnalysisRoot
            .FindNodes(InkAnalysisNodeKind.InkDrawing)
            .OfType<InkAnalysisInkDrawing>()
            .Select(drawing => CreateResult(drawing, sourceIdsByInkStrokeId))
            .FirstOrDefault(result => result is not null && result.SourceStrokeIds.Contains(requiredSourceId));
    }

    private static InkShapeAnalysisResult? CreateResult(
        InkAnalysisInkDrawing drawing,
        IReadOnlyDictionary<uint, int> sourceIdsByInkStrokeId)
    {
        var kind = MapKind(drawing.DrawingKind);
        if (kind == InkShapeKind.Unknown)
        {
            return null;
        }

        var sourceIds = drawing.GetStrokeIds()
            .Where(sourceIdsByInkStrokeId.ContainsKey)
            .Select(id => sourceIdsByInkStrokeId[id])
            .ToImmutableArray();

        if (sourceIds.Length == 0)
        {
            return null;
        }

        var bounds = drawing.BoundingRect;
        return new InkShapeAnalysisResult(
            kind,
            new PointData(drawing.Center.X, drawing.Center.Y),
            new RectData(bounds.X, bounds.Y, bounds.Width, bounds.Height),
            drawing.Points.Select(point => new PointData(point.X, point.Y)).ToImmutableArray(),
            sourceIds);
    }

    private static InkShapeKind MapKind(InkAnalysisDrawingKind kind) => kind switch
    {
        InkAnalysisDrawingKind.Circle => InkShapeKind.Circle,
        InkAnalysisDrawingKind.Ellipse => InkShapeKind.Ellipse,
        InkAnalysisDrawingKind.Triangle or
        InkAnalysisDrawingKind.IsoscelesTriangle or
        InkAnalysisDrawingKind.EquilateralTriangle or
        InkAnalysisDrawingKind.RightTriangle => InkShapeKind.Triangle,
        InkAnalysisDrawingKind.Rectangle => InkShapeKind.Rectangle,
        InkAnalysisDrawingKind.Square => InkShapeKind.Square,
        InkAnalysisDrawingKind.Diamond => InkShapeKind.Diamond,
        InkAnalysisDrawingKind.Trapezoid => InkShapeKind.Trapezoid,
        InkAnalysisDrawingKind.Parallelogram => InkShapeKind.Parallelogram,
        InkAnalysisDrawingKind.Quadrilateral => InkShapeKind.Quadrilateral,
        InkAnalysisDrawingKind.Pentagon => InkShapeKind.Pentagon,
        InkAnalysisDrawingKind.Hexagon => InkShapeKind.Hexagon,
        _ => InkShapeKind.Unknown
    };
}
