using System.ComponentModel.DataAnnotations;

namespace Gateway.Tests.Infrastructure;

internal static class SettingsValidator
{
    public static IReadOnlyList<ValidationResult> Validate(object settings)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(settings, new ValidationContext(settings), results, validateAllProperties: true);

        return results;
    }
}
