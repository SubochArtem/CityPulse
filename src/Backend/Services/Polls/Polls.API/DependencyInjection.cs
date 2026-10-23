using CityPulse.Common.Api.Auth;
using CityPulse.Common.Api.Extensions;
using CityPulse.Common.Api.Middleware;
using CityPulse.Common.Api.Swagger;
using Microsoft.AspNetCore.Authorization;
using Polls.API.Common.Filters;
using Polls.API.Grpc;
using Polls.API.Mapping;

namespace Polls.API;

public static class DependencyInjection
{
    private const string SwaggerTitle = "CityPulse Polls API";
    private const string SwaggerDescription = "Polls management service with Auth0 authentication";

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddAutoMapper(cfg =>
            cfg.AddMaps(typeof(CityGrpcProfile).Assembly));

        services.AddControllers(options =>
            options.Filters.Add<ResultFilter>());

        services.AddGrpc();

        services.AddCityPulseAuthentication(configuration);

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration(SwaggerTitle, SwaggerDescription);

        return services;
    }

    public static void UsePresentation(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseSwaggerConfiguration(SwaggerTitle);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGrpcService<CitiesGrpcService>();
        app.MapGrpcService<IdeasGrpcService>();
    }
}
