using AutoMapper;
using Polls.Application.Common.Interfaces;
using Polls.Application.Images.DTOs;
using Polls.Domain.Images;

namespace Polls.Application.Images.Resolvers;

public class ImageUrlResolver(IImageStorageService storageService)
    : IValueResolver<Image, ImageDto, string>
{
    public string Resolve(
        Image source,
        ImageDto destination,
        string destMember,
        ResolutionContext context)
        => storageService.GetUrl(source.FileName);
}
