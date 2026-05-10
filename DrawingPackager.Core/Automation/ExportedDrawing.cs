namespace DrawingPackager.Core.Automation;

public sealed record ExportedDrawing(
    string SourceDrawingPath,
    string OutputPath,
    string Format);
