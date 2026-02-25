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
        var config = new TypeAdapterConfig();
        UserMappingConfig.Configure(config);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

        services.AddScoped<IUserService, UserService>();

        services.Configure<Auth0Settings>(
            configuration.GetSection(IdentityProviderConstants.Auth0ConfigurationSection));

        services.AddHttpClient(IdentityProviderConstants.Auth0HttpClientName)
            .AddPolicyHandler(HttpPolicies.RetryPolicy())
            .AddPolicyHandler(HttpPolicies.CircuitBreakerPolicy());

        services.AddSingleton<IIdentityProvider, Auth0Service>();

        return services;
    }

    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddBusiness(configuration);
        services.AddDataAccess(configuration);

        return services;
    }
}
