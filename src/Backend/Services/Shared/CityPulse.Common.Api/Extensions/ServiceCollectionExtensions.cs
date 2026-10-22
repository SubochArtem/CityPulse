using CityPulse.Common.Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CityPulse.Common.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCityPulseAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<Auth0Settings>(configuration.GetSection(Auth0Settings.SectionName));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.ConfigureOptions<ConfigureJwtBearerOptions>();

        return services;
    }
}
