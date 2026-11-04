using Feedback.API.Features.Activity.Consumers;
using Feedback.API.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Feedback.Tests.Unit.Messaging.Tests;

public sealed class MessagingExtensionsTests
{
    private static IEndpointNameFormatter ResolveFormatter()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMq:Host"] = "localhost",
                ["RabbitMq:Port"] = "5672",
                ["RabbitMq:Username"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["RabbitMq:VirtualHost"] = "/"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddMessaging(configuration);

        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IEndpointNameFormatter>();
    }

    [Fact]
    public void AddMessaging_UserStatusConsumer_UsesFeedbackPrefixedQueue()
    {
        var formatter = ResolveFormatter();

        Assert.Equal("feedback-user-status-changed", formatter.Consumer<UserStatusChangedConsumer>());
    }

    [Fact]
    public void AddMessaging_ActivityConsumer_UsesFeedbackPrefixedQueue()
    {
        var formatter = ResolveFormatter();

        Assert.Equal("feedback-idea-created", formatter.Consumer<IdeaCreatedConsumer>());
    }
}
