using Polls.Application.Common.Models;

namespace Polls.Tests.TestData;

internal static class ImageFiles
{
    private const int DefaultSize = 16;
    private const string JpegContentType = "image/jpeg";
    private const string PngContentType = "image/png";
    private const string WebpContentType = "image/webp";
    private const string UnknownContentType = "application/octet-stream";

    public static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    public static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47];
    public static readonly byte[] WebpSignature = [0x52, 0x49, 0x46, 0x46];

    public static ImageFile Create(string fileName, byte[] signature, int size = DefaultSize)
    {
        var bytes = new byte[Math.Max(size, signature.Length)];
        signature.CopyTo(bytes, 0);

        return CreateFromBytes(fileName, bytes);
    }

    public static ImageFile CreateFromBytes(string fileName, byte[] bytes) => new()
    {
        Content = new MemoryStream(bytes),
        FileName = fileName,
        ContentType = ContentTypeFor(fileName)
    };

    private static string ContentTypeFor(string fileName) =>
        Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => JpegContentType,
            "png" => PngContentType,
            "webp" => WebpContentType,
            _ => UnknownContentType
        };
}
