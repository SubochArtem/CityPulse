using CityPulse.Contracts.Grpc.Protos;
using Grpc.Core;
using MapsterMapper;
using Moq;
using Users.DataAccess.DTOs;
using Users.DataAccess.Services;
using Users.Tests.Unit.Services.TestData;
using Xunit;
using GrpcCityStatus = CityPulse.Contracts.Grpc.Protos.CityStatus;
using DtoCityStatus = Users.DataAccess.DTOs.CityStatus;

namespace Users.Tests.Unit.Services.Tests;

public sealed class CityGrpcServiceTests
{
    private const string CityName = "Warsaw";
    private const string FailureMessage = "city service failure";

    private readonly Mock<CitiesService.CitiesServiceClient> _client = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
    private readonly CityGrpcService _sut;

    public CityGrpcServiceTests()
    {
        _sut = new CityGrpcService(_client.Object, _mapper.Object);
    }

    private static AsyncUnaryCall<GetCityResponse> SuccessfulCall(GetCityResponse response) =>
        new(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

    private static AsyncUnaryCall<GetCityResponse> FailedCall(StatusCode statusCode) =>
        new(
            Task.FromException<GetCityResponse>(new RpcException(new Status(statusCode, FailureMessage))),
            Task.FromResult(new Metadata()),
            () => new Status(statusCode, FailureMessage),
            () => new Metadata(),
            () => { });

    private static GetCityResponse BuildResponse(Guid cityId) => new()
    {
        Id = cityId.ToString(),
        Name = CityName,
        Status = GrpcCityStatus.Active
    };

    private void SetupClient(
        AsyncUnaryCall<GetCityResponse> call,
        Action<GetCityRequest, CancellationToken>? onCalled = null)
    {
        _client
            .Setup(c => c.GetCityAsync(
                It.IsAny<GetCityRequest>(),
                It.IsAny<Metadata>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Callback<GetCityRequest, Metadata, DateTime?, CancellationToken>(
                (request, _, _, cancellationToken) => onCalled?.Invoke(request, cancellationToken))
            .Returns(call);
    }

    [Fact]
    public async Task GetCityAsync_ValidCityId_SendsRequestWithCityIdAsString()
    {
        var cityId = Guid.NewGuid();
        var response = BuildResponse(cityId);
        GetCityRequest? sentRequest = null;
        SetupClient(SuccessfulCall(response), (request, _) => sentRequest = request);
        _mapper.Setup(m => m.Map<CityDto>(response)).Returns(new CityDto { Id = cityId, Name = CityName });

        await _sut.GetCityAsync(cityId);

        Assert.NotNull(sentRequest);
        Assert.Equal(cityId.ToString(), sentRequest.CityId);
    }

    [Fact]
    public async Task GetCityAsync_ValidCityId_ReturnsMappedDto()
    {
        var cityId = Guid.NewGuid();
        var response = BuildResponse(cityId);
        var expected = new CityDto { Id = cityId, Name = CityName, Status = DtoCityStatus.Active };
        SetupClient(SuccessfulCall(response));
        _mapper.Setup(m => m.Map<CityDto>(response)).Returns(expected);

        var result = await _sut.GetCityAsync(cityId);

        Assert.Same(expected, result);
        _mapper.Verify(m => m.Map<CityDto>(response), Times.Once);
    }

    [Fact]
    public async Task GetCityAsync_CancellationTokenProvided_PassesTokenToClient()
    {
        var cityId = Guid.NewGuid();
        var response = BuildResponse(cityId);
        using var cts = new CancellationTokenSource();
        CancellationToken? passedToken = null;
        SetupClient(SuccessfulCall(response), (_, token) => passedToken = token);
        _mapper.Setup(m => m.Map<CityDto>(response)).Returns(new CityDto { Id = cityId, Name = CityName });

        await _sut.GetCityAsync(cityId, cts.Token);

        Assert.Equal(cts.Token, passedToken);
    }

    [Theory]
    [MemberData(nameof(CityGrpcServiceTestData.FailureStatusCodes), MemberType = typeof(CityGrpcServiceTestData))]
    public async Task GetCityAsync_ClientFails_ThrowsRpcExceptionAndDoesNotMap(StatusCode statusCode)
    {
        SetupClient(FailedCall(statusCode));

        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.GetCityAsync(Guid.NewGuid()));

        Assert.Equal(statusCode, exception.StatusCode);
        _mapper.Verify(m => m.Map<CityDto>(It.IsAny<object>()), Times.Never);
    }
}
