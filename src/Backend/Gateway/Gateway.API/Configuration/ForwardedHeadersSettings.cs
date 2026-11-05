using System.ComponentModel.DataAnnotations;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace Gateway.API.Configuration;

public class ForwardedHeadersSettings : IValidatableObject
{
    public const string SectionName = "ForwardedHeaders";

    private const string InvalidNetworkMessage = "'{0}' is not a valid network in CIDR notation.";

    public string[] KnownNetworks { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var network in KnownNetworks)
        {
            if (!IPNetwork.TryParse(network, out _))
            {
                yield return new ValidationResult(
                    string.Format(InvalidNetworkMessage, network),
                    [nameof(KnownNetworks)]);
            }
        }
    }
}
