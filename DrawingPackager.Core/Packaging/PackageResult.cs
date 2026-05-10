namespace DrawingPackager.Core.Packaging;

public sealed record PackageResult(
    bool Success,
    string? PackageFolder,
    PackageManifest? Manifest,
    IReadOnlyList<string> CreatedFiles,
    IReadOnlyList<string> Messages)
{
    public static PackageResult Failed(params string[] messages)
    {
        return new PackageResult(false, null, null, Array.Empty<string>(), messages);
    }
}
