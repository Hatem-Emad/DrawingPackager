namespace DrawingPackager.Core.Packaging;

public sealed record PackageRequest(
    string DrawingPath,
    string OutputRoot,
    PackageOptions Options);
