namespace DrawingPackager.Core.Automation;

public sealed record DrawingInfo(
    string DrawingPath,
    string DrawingNumber,
    string Revision,
    string Title,
    IReadOnlyDictionary<string, string> Properties,
    IReadOnlyList<string> ReferencedDocuments);
