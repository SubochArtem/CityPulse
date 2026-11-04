using Microsoft.Extensions.DependencyInjection;
using Users.DataAccess.Interfaces;
using Users.Tests.Infrastructure;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class UserRepositoryGetByIdsTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    [Fact]
    public async Task GetByIdsAsync_SomeIdsExist_ReturnsOnlyExistingUsers()
    {
        var first = await SeedUserAsync();
        var second = await SeedUserAsync();
        var notRequested = await SeedUserAsync();
        Guid[] ids = [first.Id, second.Id, Guid.NewGuid()];

        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var result = await repository.GetByIdsAsync(ids);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == first.Id && x.Nickname == first.Nickname);
        Assert.Contains(result, x => x.Id == second.Id && x.Nickname == second.Nickname);
        Assert.DoesNotContain(result, x => x.Id == notRequested.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_NoIdsMatch_ReturnsEmptyList()
    {
        await SeedUserAsync();

        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var result = await repository.GetByIdsAsync([Guid.NewGuid()]);

        Assert.Empty(result);
    }
}
