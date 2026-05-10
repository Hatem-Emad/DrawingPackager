using DrawingPackager.Core.Automation;
using System.Text;

namespace DrawingPackager.Core.Packaging;

public static class PackageNameBuilder
{
    public static string Build(DrawingInfo drawingInfo)
    {
        var drawingNumber = GetValueOrFallback(drawingInfo.DrawingNumber, Path.GetFileNameWithoutExtension(drawingInfo.DrawingPath));
        var revision = GetValueOrFallback(drawingInfo.Revision, "NoRev");
        return Sanitize($"{drawingNumber}_Rev{revision}_Package");
    }

    private static string GetValueOrFallback(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string Sanitize(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            builder.Append(invalidCharacters.Contains(character) ? '_' : character);
        }

        return builder.ToString();
    }
}
