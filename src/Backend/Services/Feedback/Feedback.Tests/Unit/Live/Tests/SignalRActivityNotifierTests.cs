using Feedback.API.Features.Activity;
using Feedback.API.Features.Activity.Live;
using Feedback.Tests.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Feedback.Tests.Unit.Live.Tests;

public sealed class SignalRActivityNotifierTests
{
    private readonly Mock<IHubClients<IActivityClient>> _clients = new();
    private readonly Mock<IActivityClient> _groupClient = new();
    private readonly Mock<IActivityClient> _allClient = new();
    private readonly SignalRActivityNotifier _sut;

    public SignalRActivityNotifierTests()
    {
        _clients
            .Setup(x => x.Groups(It.IsAny<IReadOnlyList<string>>()))
            .Returns(_groupClient.Object);
        _clients.Setup(x => x.All).Returns(_allClient.Object);

        var hubContext = new Mock<IHubContext<ActivityHub, IActivityClient>>();
        hubContext.Setup(x => x.Clients).Returns(_clients.Object);

        _sut = new SignalRActivityNotifier(hubContext.Object, NullLogger<SignalRActivityNotifier>.Instance);
    }

    private void VerifyGroups(Guid cityId) =>
        _clients.Verify(
            x => x.Groups(It.Is<IReadOnlyList<string>>(groups =>
                groups.Count == 2 &&
                groups.Contains(ActivityGroups.City(cityId)) &&
                groups.Contains(ActivityGroups.All))),
            Times.Once);

    [Fact]
    public async Task NotifyAddedAsync_Entry_SendsResponseToCityAndAllGroups()
    {
        var entry = ActivityEntries.IdeaProposed();

        await _sut.NotifyAddedAsync(entry);

        VerifyGroups(entry.CityId);
        _groupClient.Verify(x => x.ActivityAdded(ActivityResponse.FromEntity(entry)), Times.Once);
    }

    [Fact]
    public async Task NotifyAddedAsync_SendFails_DoesNotThrow()
    {
        _groupClient
            .Setup(x => x.ActivityAdded(It.IsAny<ActivityResponse>()))
            .ThrowsAsync(new InvalidOperationException("backplane down"));

        var exception = await Record.ExceptionAsync(() => _sut.NotifyAddedAsync(ActivityEntries.IdeaProposed()));

        Assert.Null(exception);
    }

    [Fact]
    public async Task NotifyInvalidatedAsync_City_SendsToCityAndAllGroups()
    {
        var cityId = Guid.NewGuid();

        await _sut.NotifyInvalidatedAsync(cityId);

        VerifyGroups(cityId);
        _groupClient.Verify(x => x.ActivityInvalidated(), Times.Once);
        _allClient.Verify(x => x.ActivityInvalidated(), Times.Never);
    }

    [Fact]
    public async Task NotifyInvalidatedAsync_NoCity_SendsToEveryone()
    {
        await _sut.NotifyInvalidatedAsync(null);

        _allClient.Verify(x => x.ActivityInvalidated(), Times.Once);
        _groupClient.Verify(x => x.ActivityInvalidated(), Times.Never);
    }

    [Fact]
    public async Task NotifyInvalidatedAsync_SendFails_DoesNotThrow()
    {
        _allClient
            .Setup(x => x.ActivityInvalidated())
            .ThrowsAsync(new InvalidOperationException("backplane down"));

        var exception = await Record.ExceptionAsync(() => _sut.NotifyInvalidatedAsync(null));

        Assert.Null(exception);
    }
}
