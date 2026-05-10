namespace DrawingPackager.Core.Packaging;

public sealed record PackageManifest(
    string PackageName,
    string SourceDrawingPath,
    DateTimeOffset CreatedAt,
    string DrawingNumber,
    string Revision,
    string Title,
    IReadOnlyDictionary<string, string> Properties,
    IReadOnlyList<PackageFile> Files,
    IReadOnlyList<string> ReferencedDocuments,
    IReadOnlyList<string> Messages);
