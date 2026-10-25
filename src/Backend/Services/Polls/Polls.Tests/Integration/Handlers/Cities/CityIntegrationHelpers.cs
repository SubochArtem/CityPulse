using System.Globalization;
using System.Net.Http.Headers;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Images;
using Polls.Tests.TestData;

namespace Polls.Tests.Integration.Handlers.Cities;

internal static class CityIntegrationHelpers
{
    public const string AdminCitiesRoute = "api/v1/admin/cities";
    public const string ImageFileName = "photo.jpg";

    private const string TitleField = "Title";
    private const string DescriptionField = "Description";
    private const string LatitudeField = "Coordinates.Latitude";
    private const string LongitudeField = "Coordinates.Longitude";
    private const string ImagesField = "Images";
    private const string ImagesToAddField = "ImagesToAdd";
    private const string ImagesToDeleteField = "ImagesToDelete";
    private const string JpegContentType = "image/jpeg";

    public static MultipartFormDataContent BuildCreateForm(
        string title,
        double latitude,
        double longitude,
        string? description = null,
        IReadOnlyCollection<string>? imageFileNames = null) =>
        BuildForm(title, latitude, longitude, description, ImagesField, imageFileNames, null);

    public static MultipartFormDataContent BuildUpdateForm(
        string title,
        double latitude,
        double longitude,
        string? description = null,
        IReadOnlyCollection<string>? imageFileNamesToAdd = null,
        IReadOnlyCollection<Guid>? imageIdsToDelete = null) =>
        BuildForm(title, latitude, longitude, description, ImagesToAddField, imageFileNamesToAdd, imageIdsToDelete);

    public static City BuildCity(
        string? title = null,
        CityStatus status = CityStatus.Active,
        params string[] imageFileNames)
    {
        var id = Guid.NewGuid();

        var city = CityFakers.Cities().Generate();
        city.Id = id;
        city.Title = title ?? id.ToString();
        city.Status = status;
        city.Images = imageFileNames
            .Select((fileName, order) => new CityImage
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                Order = order,
                CityId = id
            })
            .ToList();

        return city;
    }

    private static MultipartFormDataContent BuildForm(
        string title,
        double latitude,
        double longitude,
        string? description,
        string imagesField,
        IReadOnlyCollection<string>? imageFileNames,
        IReadOnlyCollection<Guid>? imageIdsToDelete)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(title), TitleField },
            { new StringContent(latitude.ToString(CultureInfo.InvariantCulture)), LatitudeField },
            { new StringContent(longitude.ToString(CultureInfo.InvariantCulture)), LongitudeField }
        };

        if (description is not null)
            form.Add(new StringContent(description), DescriptionField);

        foreach (var fileName in imageFileNames ?? [])
        {
            var image = new ByteArrayContent(ImageFiles.JpegSignature);
            image.Headers.ContentType = new MediaTypeHeaderValue(JpegContentType);
            form.Add(image, imagesField, fileName);
        }

        foreach (var imageId in imageIdsToDelete ?? [])
            form.Add(new StringContent(imageId.ToString()), ImagesToDeleteField);

        return form;
    }
}
