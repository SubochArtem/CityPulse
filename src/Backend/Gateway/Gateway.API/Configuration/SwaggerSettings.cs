using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Configuration;

public class SwaggerSettings : IValidatableObject
{
    public const string SectionName = "Swagger";

    private const string ClientIdRequiredMessage = "ClientId is required when Swagger is enabled.";
    private const string AudienceRequiredMessage = "Audience is required when Swagger is enabled.";

    public bool Enabled { get; init; }

    public string ClientId { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enabled)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            yield return new ValidationResult(ClientIdRequiredMessage, [nameof(ClientId)]);
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            yield return new ValidationResult(AudienceRequiredMessage, [nameof(Audience)]);
        }
    }
}
