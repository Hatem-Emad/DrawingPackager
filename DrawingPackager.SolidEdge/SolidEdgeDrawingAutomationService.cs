using DrawingPackager.Core.Automation;

namespace DrawingPackager.SolidEdge;

public sealed class SolidEdgeDrawingAutomationService : IDrawingAutomationService
{
    private readonly Func<SolidEdgeSession> _sessionFactory;

    public SolidEdgeDrawingAutomationService()
        : this(() => SolidEdgeSession.AttachOrStart())
    {
    }

    public SolidEdgeDrawingAutomationService(Func<SolidEdgeSession> sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public Task<DrawingInfo> InspectAsync(string drawingPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new DrawingInfo(
            drawingPath,
            Path.GetFileNameWithoutExtension(drawingPath),
            string.Empty,
            string.Empty,
            Array.Empty<string>()));
    }

    public Task<ExportedDrawing> ExportPdfAsync(
        string drawingPath,
        string outputPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var session = _sessionFactory();
        dynamic documents = session.Application.Documents;
        dynamic document = documents.Open(drawingPath);

        try
        {
            document.SaveAs(outputPath);
        }
        finally
        {
            document.Close(false);
        }

        return Task.FromResult(new ExportedDrawing(drawingPath, outputPath, "PDF"));
    }
}
