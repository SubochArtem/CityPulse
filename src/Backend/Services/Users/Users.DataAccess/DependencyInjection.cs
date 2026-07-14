using System.Reflection;
using CityPulse.Contracts.Grpc.Protos;
using Mapster;
using MapsterMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Users.DataAccess.Interceptors;
using Users.DataAccess.Interfaces;
using Users.DataAccess.Messaging;
using Users.DataAccess.Repositories;
using Users.DataAccess.Services;
using Users.DataAccess.Settings;

namespace Users.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services
            .AddScoped<SaveChangesInterceptor>()
            .AddScoped<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var saveChangesInterceptor =
                serviceProvider.GetRequiredService<SaveChangesInterceptor>();
            var auditInterceptor =
                serviceProvider.GetRequiredService<AuditInterceptor>();

            options
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            5,
                            TimeSpan.FromSeconds(30),
                            null);
                    })
                .AddInterceptors(
                    saveChangesInterceptor,
                    auditInterceptor);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddOptions<GrpcSettings>()
            .Bind(configuration.GetSection(GrpcSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddGrpcClient<CitiesService.CitiesServiceClient>((sp, options) =>
        {
            var settings = sp.GetRequiredService<IOptions<GrpcSettings>>().Value;
            options.Address = new Uri(settings.CitiesServiceUrl);
        });

        services.AddScoped<ICityService, CityGrpcService>();
        
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        
        services.AddScoped<IEventPublisher, EventPublisherAdapter>();
        
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                var settings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>() 
                               ?? throw new InvalidOperationException("RabbitMQ settings are missing"); 

                cfg.Host(settings.Host, (ushort)settings.Port, settings.VirtualHost, host =>
                {
                    host.Username(settings.Username);
                    host.Password(settings.Password);
                }); 

                cfg.ConfigureEndpoints(context); 
            });
        });

        return services;
    }
}
