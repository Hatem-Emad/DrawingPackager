namespace DrawingPackager.Core.Packaging;

public interface IDateTimeProvider
{
    DateTimeOffset Now { get; }
}
