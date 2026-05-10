using DrawingPackager.Core.Automation;

namespace DrawingPackager.SolidEdge;

public sealed class SolidEdgeDrawingAutomationService : IDrawingAutomationService
{
    private readonly SolidEdgePropertyReader _propertyReader = new();
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

        var session = _sessionFactory();
        dynamic documents = session.Application.Documents;
        dynamic document = documents.Open(drawingPath);

        try
        {
            DrawingInfo drawingInfo = _propertyReader.Read(document, drawingPath);
            if (!string.IsNullOrWhiteSpace(drawingInfo.DrawingNumber))
            {
                return Task.FromResult(drawingInfo);
            }

            return Task.FromResult(drawingInfo with
            {
                DrawingNumber = Path.GetFileNameWithoutExtension(drawingPath)
            });
        }
        finally
        {
            document.Close(false);
        }
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
