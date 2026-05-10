namespace DrawingPackager.Core.Automation;

public interface IDrawingAutomationService
{
    Task<DrawingInfo> InspectAsync(string drawingPath, CancellationToken cancellationToken);

    Task<ExportedDrawing> ExportPdfAsync(
        string drawingPath,
        string outputPath,
        CancellationToken cancellationToken);
}
