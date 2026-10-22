namespace Feedback.API.Common.Services;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
