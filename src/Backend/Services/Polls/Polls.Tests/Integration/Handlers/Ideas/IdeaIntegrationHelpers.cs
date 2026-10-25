using System.Net.Http.Headers;
using Polls.Domain.Cities;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.Integration.Handlers.Polls;
using Polls.Tests.TestData;

namespace Polls.Tests.Integration.Handlers.Ideas;

internal sealed record IdeaGraph(City City, Poll Poll, Idea Idea)
{
    public object[] Entities => [City, Poll, Idea];
}

internal static class IdeaIntegrationHelpers
{
    public const string AdminIdeasRoute = "api/v1/admin/ideas";
    public const string UserIdeasRoute = "api/v1/ideas";
    public const string PollsSegment = "polls";
    public const string PollSegment = "poll";
    public const string AccessStatusSegment = "access-status";
    public const string ApprovalStatusSegment = "approval-status";
    public const string ImageFileName = "photo.jpg";

    private const string TitleField = "Title";
    private const string DescriptionField = "Description";
    private const string ImagesField = "Images";
    private const string ImagesToAddField = "ImagesToAdd";
    private const string ImagesToDeleteField = "ImagesToDelete";
    private const string JpegContentType = "image/jpeg";

    public static MultipartFormDataContent BuildCreateForm(
        string title,
        string? description = null,
        IReadOnlyCollection<string>? imageFileNames = null) =>
        BuildForm(title, description, ImagesField, imageFileNames, null);

    public static MultipartFormDataContent BuildUpdateForm(
        string title,
        string? description = null,
        IReadOnlyCollection<string>? imageFileNamesToAdd = null,
        IReadOnlyCollection<Guid>? imageIdsToDelete = null) =>
        BuildForm(title, description, ImagesToAddField, imageFileNamesToAdd, imageIdsToDelete);

    public static Idea BuildIdea(
        Guid pollId,
        Guid? userId = null,
        IdeaAccessStatus accessStatus = IdeaAccessStatus.Active,
        IdeaApprovalStatus approvalStatus = IdeaApprovalStatus.Pending,
        params string[] imageFileNames)
    {
        var id = Guid.NewGuid();

        var idea = IdeaFakers.Ideas().Generate();
        idea.Id = id;
        idea.PollId = pollId;
        idea.UserId = userId ?? Guid.NewGuid();
        idea.AccessStatus = accessStatus;
        idea.ApprovalStatus = approvalStatus;
        idea.Title = id.ToString();
        idea.Images = imageFileNames
            .Select((fileName, order) => new IdeaImage
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                Order = order,
                IdeaId = id
            })
            .ToList();

        return idea;
    }

    public static IdeaGraph BuildGraph(
        Guid? userId = null,
        PollStatus pollStatus = PollStatus.Active,
        IdeaAccessStatus accessStatus = IdeaAccessStatus.Active,
        IdeaApprovalStatus approvalStatus = IdeaApprovalStatus.Pending,
        params string[] imageFileNames)
    {
        var city = CityIntegrationHelpers.BuildCity();
        var poll = PollIntegrationHelpers.BuildPoll(city.Id, pollStatus);
        var idea = BuildIdea(poll.Id, userId, accessStatus, approvalStatus, imageFileNames);

        return new IdeaGraph(city, poll, idea);
    }

    private static MultipartFormDataContent BuildForm(
        string title,
        string? description,
        string imagesField,
        IReadOnlyCollection<string>? imageFileNames,
        IReadOnlyCollection<Guid>? imageIdsToDelete)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(title), TitleField }
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
