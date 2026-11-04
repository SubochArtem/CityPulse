using Xunit;

namespace Polls.Tests.Unit.Storage.TestData;

public static class ImageStorageTestData
{
    public static TheoryData<string> PublicBaseUrls => new()
    {
        "http://localhost:9000",
        "http://localhost:9000/",
        "https://cdn.example.com"
    };

    public static TheoryData<string?> InvalidPublicBaseUrls => new()
    {
        null,
        "",
        " ",
        "minio:9000",
        "localhost:9000",
        "not a url"
    };
}
