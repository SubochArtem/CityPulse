namespace Feedback.API.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
