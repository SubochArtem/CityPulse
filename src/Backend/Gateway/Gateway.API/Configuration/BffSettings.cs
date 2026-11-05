using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Configuration;

public class BffSettings : IValidatableObject
{
    public const string SectionName = "Bff";

    private const string RequiredWhenEnabledMessage = "{0} is required when the BFF is enabled.";

    public bool Enabled { get; init; }

    public string Domain { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    [Range(1, 365)]
    public int SessionLifetimeDays { get; init; } = 7;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enabled)
        {
            yield break;
        }

        var required = new (string Name, string Value)[]
        {
            (nameof(Domain), Domain),
            (nameof(ClientId), ClientId),
            (nameof(ClientSecret), ClientSecret),
            (nameof(Audience), Audience)
        };

        foreach (var (name, value) in required)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                yield return new ValidationResult(string.Format(RequiredWhenEnabledMessage, name), [name]);
            }
        }
    }
}
