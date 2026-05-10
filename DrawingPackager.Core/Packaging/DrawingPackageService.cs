using DrawingPackager.Core.Automation;
using System.Text;
using System.Text.Json;

namespace DrawingPackager.Core.Packaging;

public sealed class DrawingPackageService
{
    private static readonly JsonSerializerOptions ManifestJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IDrawingAutomationService _automationService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DrawingPackageService(
        IDrawingAutomationService automationService,
        IDateTimeProvider? dateTimeProvider = null)
    {
        _automationService = automationService;
        _dateTimeProvider = dateTimeProvider ?? new SystemDateTimeProvider();
    }

    public async Task<PackageResult> PackageAsync(PackageRequest request, CancellationToken cancellationToken = default)
    {
        var validationMessages = Validate(request);
        if (validationMessages.Count > 0)
        {
            return new PackageResult(false, null, null, Array.Empty<string>(), validationMessages);
        }

        var createdFiles = new List<string>();
        var packageFiles = new List<PackageFile>();
        var messages = new List<string>();

        try
        {
            messages.Add($"Inspecting drawing: {request.DrawingPath}");
            var drawingInfo = await _automationService.InspectAsync(request.DrawingPath, cancellationToken);

            var packageName = PackageNameBuilder.Build(drawingInfo);
            var packageFolder = EnsureUniquePackageFolder(request.OutputRoot, packageName);
            var drawingFolder = Path.Combine(packageFolder, "Drawing");
            var exportsFolder = Path.Combine(packageFolder, "Exports");

            Directory.CreateDirectory(drawingFolder);
            Directory.CreateDirectory(exportsFolder);

            if (request.Options.CopySourceDrawing)
            {
                var packageDrawingPath = Path.Combine(drawingFolder, Path.GetFileName(request.DrawingPath));
                File.Copy(request.DrawingPath, packageDrawingPath);
                createdFiles.Add(packageDrawingPath);
                packageFiles.Add(new PackageFile("SourceDrawing", request.DrawingPath, packageDrawingPath));
                messages.Add($"Copied source drawing: {packageDrawingPath}");
            }

            if (request.Options.ExportPdf)
            {
                var pdfPath = Path.Combine(exportsFolder, $"{Path.GetFileNameWithoutExtension(request.DrawingPath)}.pdf");
                var exportedDrawing = await _automationService.ExportPdfAsync(request.DrawingPath, pdfPath, cancellationToken);
                createdFiles.Add(exportedDrawing.OutputPath);
                packageFiles.Add(new PackageFile("PdfExport", request.DrawingPath, exportedDrawing.OutputPath));
                messages.Add($"Exported PDF: {exportedDrawing.OutputPath}");
            }

            if (request.Options.IncludeReferencedDocuments)
            {
                messages.Add("Referenced document packaging is planned but not implemented yet.");
            }

            if (request.Options.CreateZipArchive)
            {
                messages.Add("Zip archive creation is planned but not implemented yet.");
            }

            var manifest = new PackageManifest(
                packageName,
                request.DrawingPath,
                _dateTimeProvider.Now,
                drawingInfo.DrawingNumber,
                drawingInfo.Revision,
                drawingInfo.Title,
                packageFiles,
                drawingInfo.ReferencedDocuments,
                messages);

            var manifestPath = Path.Combine(packageFolder, "manifest.json");
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(manifest, ManifestJsonOptions),
                cancellationToken);
            createdFiles.Add(manifestPath);

            var reportPath = Path.Combine(packageFolder, "report.md");
            await File.WriteAllTextAsync(reportPath, BuildReport(manifest), cancellationToken);
            createdFiles.Add(reportPath);

            messages.Add($"Created manifest: {manifestPath}");
            messages.Add($"Created report: {reportPath}");
            messages.Add("Package complete.");

            return new PackageResult(true, packageFolder, manifest, createdFiles, messages);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            messages.Add(ex.Message);
            return new PackageResult(false, null, null, createdFiles, messages);
        }
    }

    private static List<string> Validate(PackageRequest request)
    {
        var messages = new List<string>();

        if (string.IsNullOrWhiteSpace(request.DrawingPath))
        {
            messages.Add("Drawing path is required.");
        }
        else
        {
            if (!File.Exists(request.DrawingPath))
            {
                messages.Add($"Drawing file does not exist: {request.DrawingPath}");
            }

            if (!string.Equals(Path.GetExtension(request.DrawingPath), ".dft", StringComparison.OrdinalIgnoreCase))
            {
                messages.Add("Input file must be a Solid Edge draft (.dft).");
            }
        }

        if (string.IsNullOrWhiteSpace(request.OutputRoot))
        {
            messages.Add("Output folder is required.");
        }

        return messages;
    }

    private static string EnsureUniquePackageFolder(string outputRoot, string packageName)
    {
        Directory.CreateDirectory(outputRoot);

        var packageFolder = Path.Combine(outputRoot, packageName);
        if (!Directory.Exists(packageFolder))
        {
            Directory.CreateDirectory(packageFolder);
            return packageFolder;
        }

        for (var index = 2; ; index++)
        {
            var candidate = Path.Combine(outputRoot, $"{packageName}_{index}");
            if (Directory.Exists(candidate))
            {
                continue;
            }

            Directory.CreateDirectory(candidate);
            return candidate;
        }
    }

    private static string BuildReport(PackageManifest manifest)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {manifest.PackageName}");
        builder.AppendLine();
        builder.AppendLine($"- Source drawing: `{manifest.SourceDrawingPath}`");
        builder.AppendLine($"- Drawing number: `{Display(manifest.DrawingNumber)}`");
        builder.AppendLine($"- Revision: `{Display(manifest.Revision)}`");
        builder.AppendLine($"- Title: `{Display(manifest.Title)}`");
        builder.AppendLine($"- Created: `{manifest.CreatedAt:O}`");
        builder.AppendLine();
        builder.AppendLine("## Files");
        builder.AppendLine();

        foreach (var file in manifest.Files)
        {
            builder.AppendLine($"- {file.Role}: `{file.PackagePath}`");
        }

        builder.AppendLine();
        builder.AppendLine("## Referenced Documents");
        builder.AppendLine();

        if (manifest.ReferencedDocuments.Count == 0)
        {
            builder.AppendLine("- None detected.");
        }
        else
        {
            foreach (var referencedDocument in manifest.ReferencedDocuments)
            {
                builder.AppendLine($"- `{referencedDocument}`");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Log");
        builder.AppendLine();

        foreach (var message in manifest.Messages)
        {
            builder.AppendLine($"- {message}");
        }

        return builder.ToString();
    }

    private static string Display(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Not specified" : value;
    }
}
