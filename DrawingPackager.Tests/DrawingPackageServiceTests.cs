using DrawingPackager.Core.Automation;
using DrawingPackager.Core.Packaging;

namespace DrawingPackager.Tests;

public sealed class DrawingPackageServiceTests
{
    [Fact]
    public async Task PackageAsync_creates_manifest_report_pdf_and_copied_drawing()
    {
        var workspace = CreateWorkspace();
        var drawingPath = Path.Combine(workspace, "A100.dft");
        var outputRoot = Path.Combine(workspace, "Packages");
        await File.WriteAllTextAsync(drawingPath, "draft");

        var automation = new FakeDrawingAutomationService(
            new DrawingInfo(
                drawingPath,
                "A100",
                "B",
                "Bracket",
                new Dictionary<string, string>
                {
                    ["Document Number"] = "A100",
                    ["Revision"] = "B",
                    ["Title"] = "Bracket"
                },
                Array.Empty<string>()));

        var service = new DrawingPackageService(automation, new FixedDateTimeProvider());

        var result = await service.PackageAsync(new PackageRequest(
            drawingPath,
            outputRoot,
            PackageOptions.Default));

        Assert.True(result.Success);
        Assert.NotNull(result.PackageFolder);
        Assert.Equal("A100_RevB_Package", Path.GetFileName(result.PackageFolder));
        Assert.True(File.Exists(Path.Combine(result.PackageFolder!, "Drawing", "A100.dft")));
        Assert.True(File.Exists(Path.Combine(result.PackageFolder, "Exports", "A100.pdf")));
        Assert.True(File.Exists(Path.Combine(result.PackageFolder, "manifest.json")));
        Assert.True(File.Exists(Path.Combine(result.PackageFolder, "report.md")));
        Assert.Contains(result.CreatedFiles, path => path.EndsWith("manifest.json"));
        Assert.Equal("Bracket", result.Manifest?.Title);
        Assert.Equal("B", result.Manifest?.Properties["Revision"]);
    }

    [Fact]
    public async Task PackageAsync_rejects_non_draft_files()
    {
        var workspace = CreateWorkspace();
        var partPath = Path.Combine(workspace, "A100.par");
        await File.WriteAllTextAsync(partPath, "part");

        var service = new DrawingPackageService(
            new FakeDrawingAutomationService(EmptyDrawingInfo(partPath)),
            new FixedDateTimeProvider());

        var result = await service.PackageAsync(new PackageRequest(
            partPath,
            Path.Combine(workspace, "Packages"),
            PackageOptions.Default));

        Assert.False(result.Success);
        Assert.Contains("Input file must be a Solid Edge draft (.dft).", result.Messages);
    }

    [Fact]
    public async Task PackageAsync_uses_unique_folder_when_package_already_exists()
    {
        var workspace = CreateWorkspace();
        var drawingPath = Path.Combine(workspace, "A100.dft");
        var outputRoot = Path.Combine(workspace, "Packages");
        var existingPackage = Path.Combine(outputRoot, "A100_RevB_Package");
        Directory.CreateDirectory(existingPackage);
        await File.WriteAllTextAsync(drawingPath, "draft");

        var service = new DrawingPackageService(
            new FakeDrawingAutomationService(
                new DrawingInfo(
                    drawingPath,
                    "A100",
                    "B",
                    "Bracket",
                    new Dictionary<string, string>
                    {
                        ["Document Number"] = "A100",
                        ["Revision"] = "B",
                        ["Title"] = "Bracket"
                    },
                    Array.Empty<string>())),
            new FixedDateTimeProvider());

        var result = await service.PackageAsync(new PackageRequest(
            drawingPath,
            outputRoot,
            PackageOptions.Default));

        Assert.True(result.Success);
        Assert.EndsWith("A100_RevB_Package_2", result.PackageFolder);
    }

    [Fact]
    public async Task PackageAsync_adds_metadata_messages_to_result()
    {
        var workspace = CreateWorkspace();
        var drawingPath = Path.Combine(workspace, "A100.dft");
        await File.WriteAllTextAsync(drawingPath, "draft");

        var service = new DrawingPackageService(
            new FakeDrawingAutomationService(
                new DrawingInfo(
                    drawingPath,
                    "A100",
                    "B",
                    "Bracket",
                    new Dictionary<string, string>
                    {
                        ["Document Number"] = "A100",
                        ["Revision"] = "B",
                        ["Title"] = "Bracket"
                    },
                    Array.Empty<string>())),
            new FixedDateTimeProvider());

        var result = await service.PackageAsync(new PackageRequest(
            drawingPath,
            Path.Combine(workspace, "Packages"),
            PackageOptions.Default));

        Assert.Contains("Drawing number: A100", result.Messages);
        Assert.Contains("Revision: B", result.Messages);
        Assert.Contains("Title: Bracket", result.Messages);
    }

    private static string CreateWorkspace()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "DrawingPackager.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        return workspace;
    }

    private static DrawingInfo EmptyDrawingInfo(string drawingPath)
    {
        return new DrawingInfo(
            drawingPath,
            string.Empty,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>(),
            Array.Empty<string>());
    }

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset Now { get; } = new(2026, 5, 10, 20, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeDrawingAutomationService : IDrawingAutomationService
    {
        private readonly DrawingInfo _drawingInfo;

        public FakeDrawingAutomationService(DrawingInfo drawingInfo)
        {
            _drawingInfo = drawingInfo;
        }

        public Task<DrawingInfo> InspectAsync(string drawingPath, CancellationToken cancellationToken)
        {
            return Task.FromResult(_drawingInfo);
        }

        public async Task<ExportedDrawing> ExportPdfAsync(
            string drawingPath,
            string outputPath,
            CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, "pdf", cancellationToken);
            return new ExportedDrawing(drawingPath, outputPath, "PDF");
        }
    }
}
