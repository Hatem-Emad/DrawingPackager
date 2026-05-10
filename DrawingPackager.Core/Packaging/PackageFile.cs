namespace DrawingPackager.Core.Packaging;

public sealed record PackageFile(
    string Role,
    string SourcePath,
    string PackagePath);
