using CityPulse.Common.Api.Auth;
using CityPulse.Common.Api.Extensions;
using CityPulse.Common.Api.Health;
using CityPulse.Common.Api.Middleware;
using CityPulse.Common.Api.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polls.API.Common.Filters;
using Polls.API.Grpc;
using Polls.API.Mapping;
using Polls.Domain.Common;
using Polls.Infrastructure.Persistence;
using Polls.Infrastructure.Storage;

namespace Polls.API;

public static class DependencyInjection
{
    private const string SwaggerTitle = "CityPulse Polls API";
    private const string SwaggerDescription = "Polls management service with Auth0 authentication";
    private const string PostgresCheckName = "postgres";
    private const string MinioCheckName = "minio";

    private static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(5);

    private static readonly int[] ProblemStatusCodes =
    [
        StatusCodes.Status400BadRequest,
        StatusCodes.Status401Unauthorized,
        StatusCodes.Status403Forbidden,
        StatusCodes.Status404NotFound,
        StatusCodes.Status409Conflict,
        StatusCodes.Status500InternalServerError
    ];

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddAutoMapper(cfg =>
            cfg.AddMaps(typeof(CityGrpcProfile).Assembly));

        services.AddControllers(options =>
            options.Filters.Add<ResultFilter>());

        services.AddUnifiedApiErrors();

        services.AddGrpc();

        services.AddCityPulseAuthentication(configuration);

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration(
            SwaggerTitle,
            SwaggerDescription,
            responses => responses
                .UnwrapResponse(typeof(Result<>))
                .TreatAsEmpty(typeof(Unit))
                .DocumentProblems(ProblemStatusCodes));

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(PostgresCheckName, tags: [HealthCheckConstants.ReadyTag])
            .AddCheck<MinioHealthCheck>(MinioCheckName, tags: [HealthCheckConstants.ReadyTag]);

        services.PostConfigure<HealthCheckServiceOptions>(options =>
        {
            foreach (var registration in options.Registrations)
            {
                registration.Timeout = HealthCheckTimeout;
            }
        });

        return services;
    }

    public static void UsePresentation(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseUnifiedStatusCodePages();
        app.UseSwaggerConfiguration(SwaggerTitle);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapCityPulseHealthChecks();
        app.MapControllers();
        app.MapGrpcService<CitiesGrpcService>();
        app.MapGrpcService<IdeasGrpcService>();
    }
}
