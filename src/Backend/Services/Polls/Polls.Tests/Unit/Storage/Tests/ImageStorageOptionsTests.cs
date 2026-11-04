using System.ComponentModel.DataAnnotations;
using Polls.Infrastructure.Persistence.Options;
using Polls.Tests.Unit.Storage.TestData;
using Xunit;

namespace Polls.Tests.Unit.Storage.Tests;

public sealed class ImageStorageOptionsTests
{
    private const string ValidValue = "value";
    private const string ValidPublicBaseUrl = "http://localhost:9000";

    private static List<ValidationResult> Validate(ImageStorageOptions options)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
        return results;
    }

    private static ImageStorageOptions CreateOptions(string? publicBaseUrl = ValidPublicBaseUrl) =>
        new()
        {
            Endpoint = ValidValue,
            AccessKey = ValidValue,
            SecretKey = ValidValue,
            BucketName = ValidValue,
            PublicBaseUrl = publicBaseUrl!
        };

    [Fact]
    public void Validate_AllOptionsSet_HasNoErrors()
    {
        var results = Validate(CreateOptions());

        Assert.Empty(results);
    }

    [Theory]
    [MemberData(nameof(ImageStorageTestData.InvalidPublicBaseUrls), MemberType = typeof(ImageStorageTestData))]
    public void Validate_PublicBaseUrlIsMissingOrNotAbsoluteHttpUrl_ReportsPublicBaseUrlError(string? publicBaseUrl)
    {
        var results = Validate(CreateOptions(publicBaseUrl));

        Assert.Contains(results, x => x.MemberNames.Contains(nameof(ImageStorageOptions.PublicBaseUrl)));
    }

    [Fact]
    public void Validate_RequiredStringOptionsAreEmpty_ReportsErrorForEach()
    {
        var options = new ImageStorageOptions
        {
            Endpoint = string.Empty,
            AccessKey = string.Empty,
            SecretKey = string.Empty,
            BucketName = string.Empty,
            PublicBaseUrl = ValidPublicBaseUrl
        };

        var results = Validate(options);

        var members = results.SelectMany(x => x.MemberNames).ToList();
        Assert.Contains(nameof(ImageStorageOptions.Endpoint), members);
        Assert.Contains(nameof(ImageStorageOptions.AccessKey), members);
        Assert.Contains(nameof(ImageStorageOptions.SecretKey), members);
        Assert.Contains(nameof(ImageStorageOptions.BucketName), members);
    }
}
