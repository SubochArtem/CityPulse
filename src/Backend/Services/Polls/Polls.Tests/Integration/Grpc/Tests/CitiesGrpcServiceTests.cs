using CityPulse.Contracts.Grpc.Protos;
using Grpc.Core;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Grpc.TestData;
using Polls.Tests.Integration.Handlers.Cities;
using Xunit;
using DomainCityStatus = Polls.Domain.Cities.Enums.CityStatus;
using GrpcCityStatus = CityPulse.Contracts.Grpc.Protos.CityStatus;

namespace Polls.Tests.Integration.Grpc.Tests;

public sealed class CitiesGrpcServiceTests(PollsWebAppFactory factory) : PollsGrpcTestBase(factory)
{
    private const string CityTitle = "Springfield";
    private const string InvalidCityIdMessage = "Invalid city id format";

    private static string NotFoundMessage(Guid id) => $"City with id '{id}' was not found";

    private static Metadata AnonymousMetadata() =>
        new() { { AuthConstants.Headers.Anonymous.ToLowerInvariant(), bool.TrueString } };

    [Fact]
    public async Task GetCity_ActiveCity_ReturnsIdNameAndActiveStatus()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle);
        await SeedAsync(city);

        var response = await CreateCitiesClient().GetCityAsync(new GetCityRequest { CityId = city.Id.ToString() });

        Assert.Equal(city.Id.ToString(), response.Id);
        Assert.Equal(CityTitle, response.Name);
        Assert.Equal(GrpcCityStatus.Active, response.Status);
    }

    [Fact]
    public async Task GetCity_InactiveCity_ReturnsItWithInactiveStatus()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle, DomainCityStatus.Inactive);
        await SeedAsync(city);

        var response = await CreateCitiesClient().GetCityAsync(new GetCityRequest { CityId = city.Id.ToString() });

        Assert.Equal(GrpcCityStatus.Inactive, response.Status);
    }

    [Fact]
    public async Task GetCity_CityWithUndefinedStatus_MapsToUndefined()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle, DomainCityStatus.Undefined);
        await SeedAsync(city);

        var response = await CreateCitiesClient().GetCityAsync(new GetCityRequest { CityId = city.Id.ToString() });

        Assert.Equal(GrpcCityStatus.Undefined, response.Status);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetCity_CityDoesNotExist_ReturnsNotFoundNamingTheId(bool emptyId)
    {
        var id = emptyId ? Guid.Empty : Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await CreateCitiesClient().GetCityAsync(new GetCityRequest { CityId = id.ToString() }));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        Assert.Equal(NotFoundMessage(id), exception.Status.Detail);
    }

    [Theory]
    [MemberData(nameof(GrpcIntegrationTestData.MalformedIds), MemberType = typeof(GrpcIntegrationTestData))]
    public async Task GetCity_IdIsMalformed_ReturnsInvalidArgument(MalformedIdCase malformed)
    {
        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await CreateCitiesClient().GetCityAsync(new GetCityRequest { CityId = malformed.Value }));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        Assert.Equal(InvalidCityIdMessage, exception.Status.Detail);
    }

    [Fact]
    public async Task GetCity_CallerIsAnonymous_StillReturnsTheCity()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle);
        await SeedAsync(city);

        var response = await CreateCitiesClient().GetCityAsync(
            new GetCityRequest { CityId = city.Id.ToString() },
            AnonymousMetadata());

        Assert.Equal(city.Id.ToString(), response.Id);
    }
}
