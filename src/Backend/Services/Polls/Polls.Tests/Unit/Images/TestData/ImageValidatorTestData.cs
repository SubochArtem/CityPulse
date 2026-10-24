using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Unit.Images.TestData;

public static class ImageValidatorTestData
{
    public static TheoryData<int, int> OverLimitCounts => new()
    {
        { 6, 0 },
        { 3, 3 },
        { 1, 5 },
        { 5, 1 }
    };

    public static TheoryData<int, int> WithinLimitCounts => new()
    {
        { 0, 0 },
        { 5, 0 },
        { 2, 3 },
        { 0, 5 }
    };

    public static TheoryData<string, byte[]> ValidImages => new()
    {
        { "photo.jpg", ImageFiles.JpegSignature },
        { "photo.jpeg", ImageFiles.JpegSignature },
        { "photo.png", ImageFiles.PngSignature },
        { "photo.webp", ImageFiles.WebpSignature },
        { "PHOTO.JPG", ImageFiles.JpegSignature },
        { "photo.PnG", ImageFiles.PngSignature },
        { "my.photo.png", ImageFiles.PngSignature }
    };

    public static TheoryData<string> UnsupportedFileNames => new()
    {
        "photo.gif",
        "photo.bmp",
        "notes.txt",
        "photo",
        "photo.",
        "archive.png.exe"
    };

    public static TheoryData<string, byte[]> MismatchedSignatures => new()
    {
        { "photo.png", ImageFiles.JpegSignature },
        { "photo.jpg", ImageFiles.PngSignature },
        { "photo.webp", ImageFiles.JpegSignature },
        { "photo.jpeg", ImageFiles.WebpSignature }
    };

    public static TheoryData<string, byte[]> TooShortFiles => new()
    {
        { "photo.jpg", [] },
        { "photo.jpg", [0xFF] },
        { "photo.jpg", [0xFF, 0xD8] },
        { "photo.png", [0x89, 0x50, 0x4E] },
        { "photo.webp", [0x52, 0x49, 0x46] }
    };
}
