using CityPulse.Contracts.Grpc.Protos;
using Feedback.API.Common.Errors;
using Feedback.API.Infrastructure.Grpc;
using Feedback.Tests.Infrastructure;
using Feedback.Tests.Unit.Grpc.TestData;
using Grpc.Core;
using Moq;
using Xunit;

namespace Feedback.Tests.Unit.Grpc.Tests;

public sealed class IdeaGrpcClientTests
{
    private readonly Mock<IdeasService.IdeasServiceClient> _client = new(MockBehavior.Strict);
    private readonly IdeaGrpcClient _sut;

    public IdeaGrpcClientTests()
    {
        _sut = new IdeaGrpcClient(_client.Object);
    }

    private void SetupClient(
        AsyncUnaryCall<GetIdeaCityResponse> call,
        Action<GetIdeaCityRequest, CancellationToken>? onCalled = null) =>
        _client
            .Setup(c => c.GetIdeaCityAsync(
                It.IsAny<GetIdeaCityRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Callback<GetIdeaCityRequest, Metadata, DateTime?, CancellationToken>(
                (request, _, _, cancellationToken) => onCalled?.Invoke(request, cancellationToken))
            .Returns(call);

    private static GetIdeaCityResponse BuildResponse(Guid ideaId, Guid cityId) => new()
    {
        IdeaId = ideaId.ToString(),
        CityId = cityId.ToString()
    };

    [Theory]
    [MemberData(nameof(IdeaGrpcClientTestData.IncludeOnlyActiveValues), MemberType = typeof(IdeaGrpcClientTestData))]
    public async Task GetIdeaCityAsync_ValidIdea_SendsRequestWithIdeaIdAndFlag(bool includeOnlyActive)
    {
        var ideaId = Guid.NewGuid();
        GetIdeaCityRequest? sentRequest = null;
        SetupClient(
            GrpcAsyncCalls.Successful(BuildResponse(ideaId, Guid.NewGuid())),
            (request, _) => sentRequest = request);

        await _sut.GetIdeaCityAsync(ideaId, includeOnlyActive);

        Assert.NotNull(sentRequest);
        Assert.Equal(ideaId.ToString(), sentRequest.IdeaId);
        Assert.Equal(includeOnlyActive, sentRequest.IncludeOnlyActive);
    }

    [Fact]
    public async Task GetIdeaCityAsync_CancellationTokenProvided_PassesItToClient()
    {
        var ideaId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        CancellationToken? passedToken = null;
        SetupClient(
            GrpcAsyncCalls.Successful(BuildResponse(ideaId, Guid.NewGuid())),
            (_, token) => passedToken = token);

        await _sut.GetIdeaCityAsync(ideaId, true, cts.Token);

        Assert.Equal(cts.Token, passedToken);
    }

    [Fact]
    public async Task GetIdeaCityAsync_IdeaExists_ReturnsCityIdFromResponse()
    {
        var ideaId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        SetupClient(GrpcAsyncCalls.Successful(BuildResponse(ideaId, cityId)));

        var result = await _sut.GetIdeaCityAsync(ideaId, true);

        Assert.False(result.IsError);
        Assert.Equal(cityId, result.Value);
    }

    [Theory]
    [MemberData(nameof(IdeaGrpcClientTestData.IdeaNotFoundStatuses), MemberType = typeof(IdeaGrpcClientTestData))]
    public async Task GetIdeaCityAsync_IdeaIsNotFound_ReturnsIdeaNotFoundError(StatusCode statusCode)
    {
        SetupClient(GrpcAsyncCalls.Failed<GetIdeaCityResponse>(statusCode));

        var result = await _sut.GetIdeaCityAsync(Guid.NewGuid(), true);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.IdeaNotFound, result.FirstError);
    }

    [Theory]
    [MemberData(nameof(IdeaGrpcClientTestData.ServiceUnavailableStatuses), MemberType = typeof(IdeaGrpcClientTestData))]
    public async Task GetIdeaCityAsync_ServiceIsUnavailable_ReturnsServiceUnavailableError(StatusCode statusCode)
    {
        SetupClient(GrpcAsyncCalls.Failed<GetIdeaCityResponse>(statusCode));

        var result = await _sut.GetIdeaCityAsync(Guid.NewGuid(), true);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.IdeasServiceUnavailable, result.FirstError);
    }

    [Theory]
    [MemberData(nameof(IdeaGrpcClientTestData.PropagatedStatuses), MemberType = typeof(IdeaGrpcClientTestData))]
    public async Task GetIdeaCityAsync_OtherRpcFailure_PropagatesRpcException(StatusCode statusCode)
    {
        SetupClient(GrpcAsyncCalls.Failed<GetIdeaCityResponse>(statusCode));

        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.GetIdeaCityAsync(Guid.NewGuid(), true));

        Assert.Equal(statusCode, exception.StatusCode);
    }
}
