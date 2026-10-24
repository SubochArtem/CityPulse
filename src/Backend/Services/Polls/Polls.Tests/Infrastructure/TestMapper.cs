using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Polls.Application.Common.Interfaces;
using Polls.Application.Images.Resolvers;

namespace Polls.Tests.Infrastructure;

internal static class TestMapper
{
    public static MapperConfiguration CreateConfiguration() =>
        new(
            configuration => configuration.AddMaps(typeof(ImageUrlResolver).Assembly),
            NullLoggerFactory.Instance);

    public static IMapper Create(IImageStorageService storageService) =>
        CreateConfiguration().CreateMapper(type =>
            type == typeof(ImageUrlResolver)
                ? new ImageUrlResolver(storageService)
                : Activator.CreateInstance(type)!);
}
