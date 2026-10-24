using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polls.Application.Cities.Commands.UpdateCity;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Models;
using Polls.Domain.Cities;
using Polls.Domain.Images;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Tests;

public sealed class UpdateCityCommandHandlerTests : HandlerTestsBase
{
    private const string NewTitle = "Krakow";
    private const string NewDescription = "Old capital of Poland";
    private const double NewLatitude = 50.06;
    private const double NewLongitude = 19.94;
    private const string UploadedFileName = "uploaded-new.jpg";

    private readonly UpdateCityCommandHandler _sut;

    public UpdateCityCommandHandlerTests()
    {
        _sut = new UpdateCityCommandHandler(
            UnitOfWork.Object,
            Mapper,
            Storage.Object,
            NullLogger<UpdateCityCommandHandler>.Instance);
    }

    private static UpdateCityCommand BuildCommand(
        Guid id,
        IReadOnlyList<ImageFile>? imagesToAdd = null,
        IReadOnlyList<Guid>? imagesToDelete = null) =>
        new(id, NewTitle, new CoordinatesDto(NewLatitude, NewLongitude), NewDescription, imagesToAdd, imagesToDelete);

    private void SetupCity(City? city, Guid id) =>
        UnitOfWork.Cities
            .Setup(x => x.GetByIdWithImagesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

    private void SetupTitleLookup(City? cityWithTitle) =>
        UnitOfWork.Cities
            .Setup(x => x.GetByTitleAsync(NewTitle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cityWithTitle);

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFoundErrorAndUpdatesNothing()
    {
        var id = Guid.NewGuid();
        SetupCity(null, id);

        var result = await _sut.Handle(BuildCommand(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.NotFound(id), result.Error);
        UnitOfWork.Cities.Verify(x => x.Update(It.IsAny<City>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TitleBelongsToAnotherCity_ReturnsAlreadyExistsErrorAndKeepsCityUnchanged()
    {
        var city = CityFakers.Cities().Generate();
        var originalTitle = city.Title;
        var another = CityFakers.Cities().Generate();
        another.Id = Guid.NewGuid();
        SetupCity(city, city.Id);
        SetupTitleLookup(another);

        var result = await _sut.Handle(BuildCommand(city.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.AlreadyExists, result.Error);
        Assert.Equal(originalTitle, city.Title);
        UnitOfWork.Cities.Verify(x => x.Update(It.IsAny<City>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TitleBelongsToSameCity_UpdatesCity()
    {
        var city = CityFakers.Cities().Generate();
        SetupCity(city, city.Id);
        SetupTitleLookup(city);
        UnitOfWork.Cities.Setup(x => x.Update(city));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(BuildCommand(city.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(NewTitle, city.Title);
    }

    [Fact]
    public async Task Handle_TitleIsFree_UpdatesDetailsAndSaves()
    {
        var city = CityFakers.Cities().Generate();
        SetupCity(city, city.Id);
        SetupTitleLookup(null);
        UnitOfWork.Cities.Setup(x => x.Update(city));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(BuildCommand(city.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(NewTitle, city.Title);
        Assert.Equal(NewDescription, city.Description);
        Assert.Equal(new Coordinates(NewLatitude, NewLongitude), city.Coordinates);
        UnitOfWork.Cities.Verify(x => x.Update(city), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TitleIsFree_ReturnsDtoWithUpdatedValues()
    {
        var city = CityFakers.Cities().Generate();
        SetupCity(city, city.Id);
        SetupTitleLookup(null);
        UnitOfWork.Cities.Setup(x => x.Update(city));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(BuildCommand(city.Id), CancellationToken.None);

        Assert.Equal(city.Id, result.Value!.Id);
        Assert.Equal(NewTitle, result.Value.Title);
        Assert.Equal(NewDescription, result.Value.Description);
        Assert.Equal(NewLatitude, result.Value.Coordinates.Latitude);
        Assert.Equal(NewLongitude, result.Value.Coordinates.Longitude);
    }

    [Fact]
    public async Task Handle_ImagesToAddProvided_UploadsThemAndAppendsToCity()
    {
        var city = CityFakers.CityWithImages("existing.jpg");
        var image = ImageFiles.Create("new.jpg", ImageFiles.JpegSignature);
        SetupCity(city, city.Id);
        SetupTitleLookup(null);
        UnitOfWork.Cities.Setup(x => x.Update(city));
        UnitOfWork.SetupSaveChanges();
        Storage
            .Setup(x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([UploadedFileName]);

        var result = await _sut.Handle(BuildCommand(city.Id, imagesToAdd: [image]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["existing.jpg", UploadedFileName], city.Images.Select(i => i.FileName));
        Assert.Equal([0, 1], city.Images.Select(i => i.Order));
        Assert.Equal(
            [StorageUrlPrefix + "existing.jpg", StorageUrlPrefix + UploadedFileName],
            result.Value!.Images.Select(i => i.Url));
    }

    [Fact]
    public async Task Handle_ImagesToDeleteProvided_RemovesThemAndQueuesFilesForCleanup()
    {
        var city = CityFakers.CityWithImages("first.jpg", "second.jpg");
        var imageToDelete = city.Images.First();
        List<DeletedImage>? queued = null;
        SetupCity(city, city.Id);
        SetupTitleLookup(null);
        UnitOfWork.Cities.Setup(x => x.Update(city));
        UnitOfWork.SetupSaveChanges();
        UnitOfWork.DeletedImages
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()))
            .Callback<IEnumerable<DeletedImage>>(entries => queued = entries.ToList());

        var result = await _sut.Handle(
            BuildCommand(city.Id, imagesToDelete: [imageToDelete.Id]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(imageToDelete, city.Images);
        Assert.NotNull(queued);
        Assert.Equal(["first.jpg"], queued.Select(entry => entry.FileName));
    }

    [Fact]
    public async Task Handle_ImageToDeleteDoesNotBelongToCity_ReturnsNotBelongsErrorAndDoesNotSave()
    {
        var city = CityFakers.CityWithImages("first.jpg");
        var foreignImageId = Guid.NewGuid();
        SetupCity(city, city.Id);
        SetupTitleLookup(null);
        UnitOfWork.Cities.Setup(x => x.Update(city));

        var result = await _sut.Handle(
            BuildCommand(city.Id, imagesToDelete: [foreignImageId]),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.NotBelongsToEntity(foreignImageId), result.Error);
        Assert.Single(city.Images);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
