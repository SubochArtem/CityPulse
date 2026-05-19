using CityPulse.Contracts.Grpc.Protos;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Users.Business.Configurations;
using Users.Business.Interfaces;
using Users.Business.Mapping;
using Users.Business.Policies;
using Users.Business.Services;
using Users.Business.Validators;
using Users.DataAccess;

namespace Users.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusiness(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var config = new TypeAdapterConfig();
        UserMappingConfig.Configure(config);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

        services
            .AddScoped<IUserService, UserService>()
            .AddScoped<IIdentityProviderWebhookService, Auth0WebhookService>();

        services.Configure<Auth0Settings>(
            configuration.GetSection(IdentityProviderConstants.Auth0ConfigurationSection));
        services.AddHttpClient(IdentityProviderConstants.Auth0HttpClientName)
            .AddResiliencePolicies();
        services.AddSingleton<IIdentityProvider, Auth0Service>();

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

        return services;
    }

    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddBusiness(configuration, environment);
        services.AddDataAccess(configuration);

        return services;
    }
}
