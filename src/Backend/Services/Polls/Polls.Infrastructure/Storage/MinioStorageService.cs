using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Infrastructure.Persistence.Options;

namespace Polls.Infrastructure.Storage;

public class MinioStorageService(
    IMinioClient minioClient,
    IOptions<ImageStorageOptions> options) : IImageStorageService
{
    private readonly ImageStorageOptions _options = options.Value;

    public async Task<IReadOnlyList<string>> UploadRangeAsync(
        IReadOnlyList<ImageFile> files,
        CancellationToken cancellationToken = default)
    {
        var uploadTasks = files.Select(file => UploadAsync(file, cancellationToken));
        var fileNames = await Task.WhenAll(uploadTasks);
        return fileNames;
    }

    public async Task DeleteByNamesAsync(
        IEnumerable<string> objectNames,
        CancellationToken cancellationToken = default)
    {
        var objects = objectNames
            .Select(name => new KeyValuePair<string, string>(_options.BucketName, name))
            .ToList();

        var args = new RemoveObjectsArgs()
            .WithBucket(_options.BucketName)
            .WithObjects(objects.Select(o => o.Value).ToList());

        await minioClient.RemoveObjectsAsync(args, cancellationToken);
    }

    public string GetUrl(string fileName)
        => $"{_options.Endpoint.TrimEnd('/')}/{_options.BucketName}/{fileName}";

    private async Task<string> UploadAsync(ImageFile file, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        var objectName = $"{Guid.NewGuid()}{extension}";

        var args = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectName)
            .WithStreamData(file.Content)
            .WithObjectSize(file.Content.Length)
            .WithContentType(file.ContentType);

        await minioClient.PutObjectAsync(args, cancellationToken);

        return objectName;
    }
}
