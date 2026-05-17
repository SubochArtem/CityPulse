using System.ComponentModel.DataAnnotations;

namespace Polls.Infrastructure.Persistence.Options;

public class DatabaseOptions
{
    private const int DefaultMaxRetryCount = 5;
    private const int DefaultMaxRetryDelaySeconds = 30;
    public const string SectionName = "DatabaseSettings";

    [Required(AllowEmptyStrings = false)] 
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetryCount { get; set; } = DefaultMaxRetryCount;
    public int MaxRetryDelaySeconds { get; set; } = DefaultMaxRetryDelaySeconds;
}
