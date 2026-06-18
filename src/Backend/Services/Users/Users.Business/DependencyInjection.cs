using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

        services
            .AddScoped<IUserService, UserService>()
            .AddScoped<IIdentityProviderWebhookService, Auth0WebhookService>();

        services.AddOptions<Auth0Settings>()
            .Bind(configuration.GetSection(IdentityProviderConstants.Auth0ConfigurationSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddHttpClient(IdentityProviderConstants.Auth0HttpClientName)
            .AddResiliencePolicies();
        
        services.AddSingleton<IIdentityProvider, Auth0Service>();
        
        return services;
    }

    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mapperConfig = new TypeAdapterConfig();
        UserMappingConfig.Configure(mapperConfig);      
        mapperConfig.Scan(
            typeof(Business.DependencyInjection).Assembly,
            typeof(DataAccess.DependencyInjection).Assembly);

        services.AddSingleton(mapperConfig);
        services.AddScoped<IMapper, ServiceMapper>();
        
        services.AddBusiness(configuration);
        services.AddDataAccess(configuration);

        return services;
    }
}
