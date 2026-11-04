using MassTransit;
using Microsoft.Extensions.Options;

namespace Feedback.API.Infrastructure.Messaging;

public static class MessagingExtensions
{
    private const string EndpointPrefix = "feedback";

    private static readonly TimeSpan[] RetryIntervals =
    [
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5)
    ];

    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(EndpointPrefix, false));
            x.AddConsumers(typeof(MessagingExtensions).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                cfg.Host(options.Host, options.Port, options.VirtualHost, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });

                cfg.UseMessageRetry(r => r.Intervals(RetryIntervals));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
