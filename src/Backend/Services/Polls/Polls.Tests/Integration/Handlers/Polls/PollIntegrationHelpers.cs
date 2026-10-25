using System.Globalization;
using System.Net.Http.Headers;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.TestData;

namespace Polls.Tests.Integration.Handlers.Polls;

internal static class PollIntegrationHelpers
{
    public const string AdminPollsRoute = "api/v1/admin/polls";
    public const string UserPollsRoute = "api/v1/polls";
    public const string CitiesSegment = "cities";
    public const string ImageFileName = "photo.jpg";

    private const string TitleField = "Title";
    private const string DescriptionField = "Description";
    private const string TypeField = "Type";
    private const string EndsAtField = "EndsAt";
    private const string BudgetAmountField = "BudgetAmount";
    private const string ImagesField = "Images";
    private const string ImagesToAddField = "ImagesToAdd";
    private const string ImagesToDeleteField = "ImagesToDelete";
    private const string JpegContentType = "image/jpeg";
    private const string RoundTripDateFormat = "O";

    public static MultipartFormDataContent BuildCreateForm(
        string title,
        PollType type,
        DateTimeOffset endsAt,
        decimal budgetAmount,
        string? description = null,
        IReadOnlyCollection<string>? imageFileNames = null)
    {
        var form = BuildForm(title, endsAt, budgetAmount, description, ImagesField, imageFileNames, null);
        form.Add(new StringContent(((int)type).ToString(CultureInfo.InvariantCulture)), TypeField);

        return form;
    }

    public static MultipartFormDataContent BuildUpdateForm(
        string title,
        DateTimeOffset endsAt,
        decimal budgetAmount,
        string? description = null,
        IReadOnlyCollection<string>? imageFileNamesToAdd = null,
        IReadOnlyCollection<Guid>? imageIdsToDelete = null) =>
        BuildForm(title, endsAt, budgetAmount, description, ImagesToAddField, imageFileNamesToAdd, imageIdsToDelete);

    public static Poll BuildPoll(
        Guid cityId,
        PollStatus status = PollStatus.Active,
        PollType type = PollType.UserVote,
        DateTimeOffset? createdAt = null,
        params string[] imageFileNames)
    {
        var id = Guid.NewGuid();

        var poll = PollFakers.Polls().Generate();
        poll.Id = id;
        poll.CityId = cityId;
        poll.Status = status;
        poll.Type = type;
        poll.CreatedAt = createdAt ?? poll.CreatedAt;
        poll.Images = imageFileNames
            .Select((fileName, order) => new PollImage
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                Order = order,
                PollId = id
            })
            .ToList();

        return poll;
    }

    private static MultipartFormDataContent BuildForm(
        string title,
        DateTimeOffset endsAt,
        decimal budgetAmount,
        string? description,
        string imagesField,
        IReadOnlyCollection<string>? imageFileNames,
        IReadOnlyCollection<Guid>? imageIdsToDelete)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(title), TitleField },
            { new StringContent(endsAt.ToString(RoundTripDateFormat, CultureInfo.InvariantCulture)), EndsAtField },
            { new StringContent(budgetAmount.ToString(CultureInfo.InvariantCulture)), BudgetAmountField }
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
