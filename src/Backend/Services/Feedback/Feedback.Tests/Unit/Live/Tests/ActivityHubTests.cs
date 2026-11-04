using System.Reflection;
using Feedback.API.Features.Activity.Live;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Feedback.Tests.Unit.Live.Tests;

public sealed class ActivityHubTests : IDisposable
{
    private const string ConnectionId = "connection-1";

    private readonly Mock<IGroupManager> _groups = new();
    private readonly Dictionary<object, object?> _items = [];
    private readonly ActivityHub _sut;

    public ActivityHubTests()
    {
        var context = new Mock<HubCallerContext>();
        context.Setup(x => x.ConnectionId).Returns(ConnectionId);
        context.Setup(x => x.Items).Returns(_items);

        _sut = new ActivityHub
        {
            Context = context.Object,
            Groups = _groups.Object
        };
    }

    public void Dispose() => _sut.Dispose();

    private void VerifyAdded(string group, Times times) =>
        _groups.Verify(x => x.AddToGroupAsync(ConnectionId, group, It.IsAny<CancellationToken>()), times);

    private void VerifyRemoved(string group, Times times) =>
        _groups.Verify(x => x.RemoveFromGroupAsync(ConnectionId, group, It.IsAny<CancellationToken>()), times);

    [Fact]
    public void Hub_Always_AllowsAnonymousConnections()
    {
        Assert.NotNull(typeof(ActivityHub).GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public async Task Watch_City_JoinsCityGroup()
    {
        var cityId = Guid.NewGuid();

        await _sut.Watch(cityId);

        VerifyAdded(ActivityGroups.City(cityId), Times.Once());
    }

    [Fact]
    public async Task Watch_NoCity_JoinsAllGroup()
    {
        await _sut.Watch(null);

        VerifyAdded(ActivityGroups.All, Times.Once());
    }

    [Fact]
    public async Task Watch_AnotherCity_LeavesPreviousGroupBeforeJoiningNew()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        await _sut.Watch(first);

        await _sut.Watch(second);

        VerifyRemoved(ActivityGroups.City(first), Times.Once());
        VerifyAdded(ActivityGroups.City(second), Times.Once());
    }

    [Fact]
    public async Task Watch_SameCityTwice_DoesNotRejoin()
    {
        var cityId = Guid.NewGuid();
        await _sut.Watch(cityId);

        await _sut.Watch(cityId);

        VerifyAdded(ActivityGroups.City(cityId), Times.Once());
        VerifyRemoved(ActivityGroups.City(cityId), Times.Never());
    }

    [Fact]
    public async Task Watch_EmptyCity_ThrowsHubExceptionAndJoinsNothing()
    {
        var exception = await Assert.ThrowsAsync<HubException>(() => _sut.Watch(Guid.Empty));

        Assert.Equal(ActivityHub.EmptyCityMessage, exception.Message);
        _groups.VerifyNoOtherCalls();
    }
}
