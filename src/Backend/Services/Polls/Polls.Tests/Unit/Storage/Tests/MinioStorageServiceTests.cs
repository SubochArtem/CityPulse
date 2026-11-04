using Microsoft.Extensions.Options;
using Minio;
using Moq;
using Polls.Infrastructure.Persistence.Options;
using Polls.Infrastructure.Storage;
using Polls.Tests.Unit.Storage.TestData;
using Xunit;

namespace Polls.Tests.Unit.Storage.Tests;

public sealed class MinioStorageServiceTests
{
    private const string BucketName = "polls-images";
    private const string FileName = "photo.jpg";
    private const string InternalEndpoint = "minio:9000";

    private static MinioStorageService CreateSut(string publicBaseUrl) =>
        new(
            new Mock<IMinioClient>(MockBehavior.Strict).Object,
            Options.Create(new ImageStorageOptions
            {
                Endpoint = InternalEndpoint,
                AccessKey = "access",
                SecretKey = "secret",
                BucketName = BucketName,
                PublicBaseUrl = publicBaseUrl
            }));

    [Theory]
    [MemberData(nameof(ImageStorageTestData.PublicBaseUrls), MemberType = typeof(ImageStorageTestData))]
    public void GetUrl_PublicBaseUrlConfigured_BuildsUrlFromBaseUrlBucketAndFileName(string publicBaseUrl)
    {
        var sut = CreateSut(publicBaseUrl);

        var url = sut.GetUrl(FileName);

        Assert.Equal($"{publicBaseUrl.TrimEnd('/')}/{BucketName}/{FileName}", url);
    }

    [Fact]
    public void GetUrl_InternalEndpointDiffersFromPublicBaseUrl_DoesNotLeakInternalEndpoint()
    {
        var sut = CreateSut("http://localhost:9000");

        var url = sut.GetUrl(FileName);

        Assert.DoesNotContain(InternalEndpoint, url);
        Assert.StartsWith("http://", url);
    }
}
