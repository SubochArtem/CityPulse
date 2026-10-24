using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polls.Application.Cities.Commands.CreateCity;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Images;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Cities.Tests;

public sealed class CreateCityCommandHandlerTests : HandlerTestsBase
{
    private const string Title = "Warsaw";
    private const string Description = "Capital of Poland";
    private const double Latitude = 52.23;
    private const double Longitude = 21.01;
    private const string UploadedFileName = "uploaded-a.jpg";

    private readonly CreateCityCommandHandler _sut;

    public CreateCityCommandHandlerTests()
    {
        _sut = new CreateCityCommandHandler(
            UnitOfWork.Object,
            Mapper,
            Storage.Object,
            NullLogger<CreateCityCommandHandler>.Instance);
    }

    private static CreateCityCommand BuildCommand(IReadOnlyList<ImageFile>? images = null) =>
        new(Title, new CoordinatesDto(Latitude, Longitude), Description, images);

    private void SetupNoExistingCity() =>
        UnitOfWork.Cities
            .Setup(x => x.GetByTitleAsync(Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

    [Fact]
    public async Task Handle_CityWithSameTitleExists_ReturnsAlreadyExistsErrorAndCreatesNothing()
    {
        var existing = CityFakers.Cities().Generate();
        UnitOfWork.Cities
            .Setup(x => x.GetByTitleAsync(Title, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.AlreadyExists, result.Error);
        UnitOfWork.Cities.Verify(x => x.Create(It.IsAny<City>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TitleIsFree_CreatesActiveCityWithGivenDetailsAndSaves()
    {
        City? created = null;
        SetupNoExistingCity();
        UnitOfWork.Cities
            .Setup(x => x.Create(It.IsAny<City>()))
            .Callback<City>(city => created = city);
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(created);
        Assert.Equal(Title, created.Title);
        Assert.Equal(Description, created.Description);
        Assert.Equal(new Coordinates(Latitude, Longitude), created.Coordinates);
        Assert.Equal(CityStatus.Active, created.Status);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TitleIsFree_ReturnsDtoOfCreatedCity()
    {
        SetupNoExistingCity();
        UnitOfWork.Cities.Setup(x => x.Create(It.IsAny<City>()));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.Equal(Title, result.Value!.Title);
        Assert.Equal(Description, result.Value.Description);
        Assert.Equal(Latitude, result.Value.Coordinates.Latitude);
        Assert.Equal(Longitude, result.Value.Coordinates.Longitude);
        Assert.Equal((int)CityStatus.Active, result.Value.Status);
        Assert.Empty(result.Value.Images);
    }

    [Fact]
    public async Task Handle_ImagesProvided_UploadsThemAndAttachesToCity()
    {
        City? created = null;
        var image = ImageFiles.Create("a.jpg", ImageFiles.JpegSignature);
        SetupNoExistingCity();
        UnitOfWork.Cities
            .Setup(x => x.Create(It.IsAny<City>()))
            .Callback<City>(city => created = city);
        UnitOfWork.SetupSaveChanges();
        Storage
            .Setup(x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([UploadedFileName]);

        var result = await _sut.Handle(BuildCommand([image]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var attached = Assert.Single(created!.Images);
        Assert.Equal(UploadedFileName, attached.FileName);
        Assert.Equal(0, attached.Order);
        var dtoImage = Assert.Single(result.Value!.Images);
        Assert.Equal(StorageUrlPrefix + UploadedFileName, dtoImage.Url);
    }

    [Fact]
    public async Task Handle_TooManyImages_ReturnsErrorAndDoesNotUploadOrSave()
    {
        var images = Enumerable
            .Range(0, ValidationConstants.Image.MaxCount + 1)
            .Select(index => ImageFiles.Create($"image-{index}.jpg", ImageFiles.JpegSignature))
            .ToList();
        SetupNoExistingCity();
        UnitOfWork.Cities.Setup(x => x.Create(It.IsAny<City>()));

        var result = await _sut.Handle(BuildCommand(images), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.MaxImagesExceeded(ValidationConstants.Image.MaxCount), result.Error);
        Storage.Verify(
            x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesItToRepositoryAndSave()
    {
        using var cts = new CancellationTokenSource();
        UnitOfWork.Cities
            .Setup(x => x.GetByTitleAsync(Title, cts.Token))
            .ReturnsAsync((City?)null);
        UnitOfWork.Cities.Setup(x => x.Create(It.IsAny<City>()));
        UnitOfWork.Mock.Setup(x => x.SaveChangesAsync(cts.Token)).Returns(Task.CompletedTask);

        await _sut.Handle(BuildCommand(), cts.Token);

        UnitOfWork.Cities.Verify(x => x.GetByTitleAsync(Title, cts.Token), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(cts.Token), Times.Once);
    }
}
