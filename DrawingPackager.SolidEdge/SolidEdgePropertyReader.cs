using DrawingPackager.Core.Automation;
using System.Runtime.InteropServices;

namespace DrawingPackager.SolidEdge;

internal sealed class SolidEdgePropertyReader
{
    private static readonly string[] DrawingNumberCandidates =
    {
        "Document Number",
        "Drawing Number",
        "Part Number",
        "Number"
    };

    private static readonly string[] RevisionCandidates =
    {
        "Revision",
        "Rev",
        "Document Revision",
        "Revision Number"
    };

    private static readonly string[] TitleCandidates =
    {
        "Title",
        "Subject",
        "Description"
    };

    public DrawingInfo Read(dynamic document, string drawingPath)
    {
        var properties = ReadProperties(document);

        return new DrawingInfo(
            drawingPath,
            FirstValue(properties, DrawingNumberCandidates),
            FirstValue(properties, RevisionCandidates),
            FirstValue(properties, TitleCandidates),
            properties,
            Array.Empty<string>());
    }

    private static IReadOnlyDictionary<string, string> ReadProperties(dynamic document)
    {
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            dynamic propertySets = document.Properties;
            var propertySetCount = Convert.ToInt32(propertySets.Count);

            for (var propertySetIndex = 1; propertySetIndex <= propertySetCount; propertySetIndex++)
            {
                dynamic propertySet = propertySets.Item(propertySetIndex);
                var propertySetName = ReadString(() => propertySet.Name);
                var propertyCount = Convert.ToInt32(propertySet.Count);

                for (var propertyIndex = 1; propertyIndex <= propertyCount; propertyIndex++)
                {
                    dynamic property = propertySet.Item(propertyIndex);
                    var propertyName = ReadString(() => property.Name);
                    var propertyValue = ReadString(() => property.Value);

                    if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(propertyValue))
                    {
                        continue;
                    }

                    AddProperty(properties, propertyName, propertyValue);

                    if (!string.IsNullOrWhiteSpace(propertySetName))
                    {
                        AddProperty(properties, $"{propertySetName}.{propertyName}", propertyValue);
                    }
                }
            }
        }
        catch (Exception ex) when (IsExpectedComReadFailure(ex))
        {
            return properties;
        }

        return properties;
    }

    private static void AddProperty(IDictionary<string, string> properties, string name, string value)
    {
        properties.TryAdd(name.Trim(), value.Trim());
    }

    private static string FirstValue(
        IReadOnlyDictionary<string, string> properties,
        IReadOnlyList<string> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (properties.TryGetValue(candidate, out var directValue) && !string.IsNullOrWhiteSpace(directValue))
            {
                return directValue;
            }

            var setQualifiedValue = properties
                .Where(property => property.Key.EndsWith($".{candidate}", StringComparison.OrdinalIgnoreCase))
                .Select(property => property.Value)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

            if (!string.IsNullOrWhiteSpace(setQualifiedValue))
            {
                return setQualifiedValue;
            }
        }

        return string.Empty;
    }

    private static string ReadString(Func<object?> readValue)
    {
        try
        {
            var value = readValue();
            return value?.ToString() ?? string.Empty;
        }
        catch (Exception ex) when (IsExpectedComReadFailure(ex))
        {
            return string.Empty;
        }
    }

    private static bool IsExpectedComReadFailure(Exception ex)
    {
        return ex is COMException || ex.GetType().FullName == "Microsoft.CSharp.RuntimeBinder.RuntimeBinderException";
    }
}
