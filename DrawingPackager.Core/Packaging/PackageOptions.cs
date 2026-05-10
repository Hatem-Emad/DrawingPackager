namespace DrawingPackager.Core.Packaging;

public sealed record PackageOptions
{
    public bool ExportPdf { get; init; } = true;

    public bool CopySourceDrawing { get; init; } = true;

    public bool IncludeReferencedDocuments { get; init; }

    public bool CreateZipArchive { get; init; }

    public static PackageOptions Default { get; } = new();
}
