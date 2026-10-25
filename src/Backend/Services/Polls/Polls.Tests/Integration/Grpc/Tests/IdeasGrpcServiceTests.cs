using CityPulse.Contracts.Grpc.Protos;
using Grpc.Core;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Grpc.TestData;
using Polls.Tests.Integration.Handlers.Ideas;
using Polls.Tests.Integration.Handlers.Ideas.TestData;
using Xunit;

namespace Polls.Tests.Integration.Grpc.Tests;

public sealed class IdeasGrpcServiceTests(PollsWebAppFactory factory) : PollsGrpcTestBase(factory)
{
    private const string InvalidIdeaIdMessage = "Invalid idea id format";

    private static string NotFoundMessage(Guid id) => $"Idea with id '{id}' was not found";

    private static Metadata AnonymousMetadata() =>
        new() { { AuthConstants.Headers.Anonymous.ToLowerInvariant(), bool.TrueString } };

    private static GetIdeaCityRequest Request(Guid ideaId, bool includeOnlyActive = false) =>
        new() { IdeaId = ideaId.ToString(), IncludeOnlyActive = includeOnlyActive };

    [Fact]
    public async Task GetIdeaCity_IdeaExists_ReturnsIdeaIdAndCityOfItsPoll()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        var other = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);
        await SeedAsync(other.Entities);

        var response = await CreateIdeasClient().GetIdeaCityAsync(Request(graph.Idea.Id));

        Assert.Equal(graph.Idea.Id.ToString(), response.IdeaId);
        Assert.Equal(graph.City.Id.ToString(), response.CityId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetIdeaCity_IdeaIsActive_IsReturnedWithAndWithoutActiveOnlyFlag(bool includeOnlyActive)
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await CreateIdeasClient().GetIdeaCityAsync(Request(graph.Idea.Id, includeOnlyActive));

        Assert.Equal(graph.City.Id.ToString(), response.CityId);
    }

    [Theory]
    [MemberData(nameof(GetIdeaIntegrationTestData.RestrictedAccessStatuses), MemberType = typeof(GetIdeaIntegrationTestData))]
    public async Task GetIdeaCity_IdeaIsRestricted_IsReturnedUnlessOnlyActiveIsRequested(IdeaAccessStatus status)
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(accessStatus: status);
        await SeedAsync(graph.Entities);
        var client = CreateIdeasClient();

        var response = await client.GetIdeaCityAsync(Request(graph.Idea.Id));
        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await client.GetIdeaCityAsync(Request(graph.Idea.Id, includeOnlyActive: true)));

        Assert.Equal(graph.City.Id.ToString(), response.CityId);
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        Assert.Equal(NotFoundMessage(graph.Idea.Id), exception.Status.Detail);
    }

    [Fact]
    public async Task GetIdeaCity_PollIsFinished_StillReturnsTheCity()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(pollStatus: PollStatus.Finished);
        await SeedAsync(graph.Entities);

        var response = await CreateIdeasClient().GetIdeaCityAsync(Request(graph.Idea.Id, includeOnlyActive: true));

        Assert.Equal(graph.City.Id.ToString(), response.CityId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetIdeaCity_IdeaDoesNotExist_ReturnsNotFoundNamingTheId(bool emptyId)
    {
        var id = emptyId ? Guid.Empty : Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await CreateIdeasClient().GetIdeaCityAsync(Request(id)));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        Assert.Equal(NotFoundMessage(id), exception.Status.Detail);
    }

    [Theory]
    [MemberData(nameof(GrpcIntegrationTestData.MalformedIds), MemberType = typeof(GrpcIntegrationTestData))]
    public async Task GetIdeaCity_IdIsMalformed_ReturnsInvalidArgument(MalformedIdCase malformed)
    {
        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await CreateIdeasClient().GetIdeaCityAsync(new GetIdeaCityRequest { IdeaId = malformed.Value }));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        Assert.Equal(InvalidIdeaIdMessage, exception.Status.Detail);
    }

    [Fact]
    public async Task GetIdeaCity_CallerIsAnonymous_StillReturnsTheCity()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await CreateIdeasClient().GetIdeaCityAsync(
            Request(graph.Idea.Id),
            AnonymousMetadata());

        Assert.Equal(graph.City.Id.ToString(), response.CityId);
    }
}
