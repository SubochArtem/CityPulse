using ErrorOr;
using Feedback.API.Common.Errors;
using Feedback.API.Features.Feedback.AddFeedback;
using Feedback.Tests.Infrastructure;
using Feedback.Tests.Integration.Handlers.TestData;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Feedback.Tests.Integration.Handlers.Tests;

public sealed class AddFeedbackCommandHandlerTests(MongoFixture fixture) : MongoTestsBase(fixture)
{
    private const string CommentText = "Great idea!";
    private const string PaddedText = "  Great idea!  ";
    private const int ConcurrentLikes = 20;
    private const string TextField = "Text";

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CityId = Guid.NewGuid();
    private static readonly Guid IdeaId = Guid.NewGuid();

    private AddFeedbackCommandHandler CreateSut() =>
        new(Context, DateTimeProvider.Object, IdeaService.Object);

    private static AddFeedbackCommand BuildCommand(
        string? text,
        Guid? userId = null,
        Guid? ideaId = null,
        Guid? userCityId = null,
        bool bypassRestrictions = false) =>
        new(userId ?? UserId, userCityId ?? CityId, ideaId ?? IdeaId, text, bypassRestrictions);

    private void SetupIdeaCity(ErrorOr<Guid> result, Guid? ideaId = null) =>
        IdeaService
            .Setup(x => x.GetIdeaCityAsync(
                ideaId ?? IdeaId,
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    private void SetupIdeaCityInCityOfUser() => SetupIdeaCity(CityId);

    [Theory]
    [MemberData(nameof(AddFeedbackTestData.IdeaServiceErrors), MemberType = typeof(AddFeedbackTestData))]
    public async Task Handle_IdeaServiceFails_ReturnsItsErrorAndStoresNothing(Error error)
    {
        SetupIdeaCity(error);

        var result = await CreateSut().Handle(BuildCommand(CommentText), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(error, result.FirstError);
        Assert.Empty(await ReadAllAsync());
    }

    [Theory]
    [MemberData(nameof(AddFeedbackTestData.BypassRestrictionsValues), MemberType = typeof(AddFeedbackTestData))]
    public async Task Handle_Always_AsksIdeaServiceForActiveIdeasOnlyUnlessRestrictionsAreBypassed(
        bool bypassRestrictions)
    {
        using var cts = new CancellationTokenSource();
        SetupIdeaCity(CityId);

        await CreateSut().Handle(BuildCommand(CommentText, bypassRestrictions: bypassRestrictions), cts.Token);

        IdeaService.Verify(
            x => x.GetIdeaCityAsync(IdeaId, !bypassRestrictions, cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_IdeaBelongsToAnotherCity_ReturnsNotFromUserCityErrorAndStoresNothing()
    {
        SetupIdeaCity(Guid.NewGuid());

        var result = await CreateSut().Handle(BuildCommand(CommentText), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.NotFromUserCity, result.FirstError);
        Assert.Empty(await ReadAllAsync());
    }

    [Fact]
    public async Task Handle_IdeaBelongsToAnotherCityButRestrictionsAreBypassed_StoresFeedback()
    {
        SetupIdeaCity(Guid.NewGuid());

        var result = await CreateSut().Handle(
            BuildCommand(CommentText, bypassRestrictions: true),
            CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Single(await ReadAllAsync());
    }

    [Fact]
    public async Task Handle_Comment_StoresItWithUserIdeaTextAndCreationTime()
    {
        SetupIdeaCityInCityOfUser();

        var result = await CreateSut().Handle(BuildCommand(CommentText), CancellationToken.None);

        var stored = Assert.Single(await ReadAllAsync());
        Assert.Equal(UserId, stored.UserId);
        Assert.Equal(IdeaId, stored.IdeaId);
        Assert.Equal(CommentText, stored.Text);
        Assert.Equal(Now, stored.CreatedAt);
        Assert.Null(stored.UpdatedAt);
        Assert.NotEqual(Guid.Empty, stored.Id);
        Assert.Equal(stored.Id, result.Value.Id);
    }

    [Fact]
    public async Task Handle_Comment_ReturnsResponseMatchingStoredItem()
    {
        SetupIdeaCityInCityOfUser();

        var result = await CreateSut().Handle(BuildCommand(CommentText), CancellationToken.None);

        var stored = Assert.Single(await ReadAllAsync());
        Assert.False(result.IsError);
        Assert.Equal(stored.Id, result.Value.Id);
        Assert.Equal(UserId, result.Value.UserId);
        Assert.Equal(IdeaId, result.Value.IdeaId);
        Assert.Equal(CommentText, result.Value.Text);
        Assert.Equal(Now, result.Value.CreatedAt);
        Assert.Null(result.Value.UpdatedAt);
    }

    [Fact]
    public async Task Handle_CommentWithSurroundingWhitespace_StoresTrimmedText()
    {
        SetupIdeaCityInCityOfUser();

        await CreateSut().Handle(BuildCommand(PaddedText), CancellationToken.None);

        var stored = Assert.Single(await ReadAllAsync());
        Assert.Equal(CommentText, stored.Text);
    }

    [Fact]
    public async Task Handle_SameCommentTwice_StoresBothAsSeparateItems()
    {
        SetupIdeaCityInCityOfUser();
        var sut = CreateSut();

        await sut.Handle(BuildCommand(CommentText), CancellationToken.None);
        await sut.Handle(BuildCommand(CommentText), CancellationToken.None);

        var stored = await ReadAllAsync();
        Assert.Equal(2, stored.Count);
        Assert.Equal(2, stored.Select(item => item.Id).Distinct().Count());
    }

    [Fact]
    public async Task Handle_Like_StoresItWithoutTextField()
    {
        SetupIdeaCityInCityOfUser();

        var result = await CreateSut().Handle(BuildCommand(null), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Null(result.Value.Text);
        var document = await RawItems.Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();
        Assert.False(document.Contains(TextField));
    }

    [Fact]
    public async Task Handle_SecondLikeFromSameUserOnSameIdea_ReturnsLikeAlreadyExistsAndKeepsSingleLike()
    {
        SetupIdeaCityInCityOfUser();
        var sut = CreateSut();
        await sut.Handle(BuildCommand(null), CancellationToken.None);

        var result = await sut.Handle(BuildCommand(null), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.LikeAlreadyExists, result.FirstError);
        Assert.Single(await ReadAllAsync());
    }

    [Fact]
    public async Task Handle_LikeAfterComment_StoresBothForSameUserAndIdea()
    {
        SetupIdeaCityInCityOfUser();
        var sut = CreateSut();
        await sut.Handle(BuildCommand(CommentText), CancellationToken.None);

        var result = await sut.Handle(BuildCommand(null), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(2, (await ReadAllAsync()).Count);
    }

    [Fact]
    public async Task Handle_LikesFromDifferentUsersOrOnDifferentIdeas_AreAllStored()
    {
        var otherIdeaId = Guid.NewGuid();
        SetupIdeaCityInCityOfUser();
        SetupIdeaCity(CityId, otherIdeaId);
        var sut = CreateSut();

        await sut.Handle(BuildCommand(null), CancellationToken.None);
        await sut.Handle(BuildCommand(null, userId: Guid.NewGuid()), CancellationToken.None);
        await sut.Handle(BuildCommand(null, ideaId: otherIdeaId), CancellationToken.None);

        Assert.Equal(3, (await ReadAllAsync()).Count);
    }

    [Fact]
    public async Task Handle_ManyConcurrentLikesFromSameUserOnSameIdea_StoresExactlyOne()
    {
        await Context.CreateIndexesAsync();
        SetupIdeaCityInCityOfUser();
        var sut = CreateSut();

        var results = await Task.WhenAll(
            Enumerable
                .Range(0, ConcurrentLikes)
                .Select(_ => Task.Run(() => sut.Handle(BuildCommand(null), CancellationToken.None))));

        Assert.Single(results, result => !result.IsError);
        Assert.All(
            results.Where(result => result.IsError),
            result => Assert.Equal(FeedbackErrors.LikeAlreadyExists, result.FirstError));
        Assert.Single(await ReadAllAsync());
    }
}
