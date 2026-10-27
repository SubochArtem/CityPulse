using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace Gateway.Tests.Integration.Startup.Tests;

[Trait(TestConstants.Traits.Category, TestConstants.Traits.Integration)]
public sealed class StartupValidationTests
{
    private const string InvalidTokenLimit = "0";

    [Fact]
    public void Start_TokenLimitIsInvalid_ShouldFailWithOptionsValidationException()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(TestConstants.Environment);
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [TestConstants.ConfigurationKeys.TokenLimit] = InvalidTokenLimit
                }));
        });

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Fact]
    public void Start_DefaultConfiguration_ShouldSucceed()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment(TestConstants.Environment));

        using var client = factory.CreateClient();

        Assert.NotNull(client);
    }
}
