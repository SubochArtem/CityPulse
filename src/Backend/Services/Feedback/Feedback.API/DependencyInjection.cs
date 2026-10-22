using CityPulse.Common.Api.Auth;
using CityPulse.Common.Api.Extensions;
using CityPulse.Common.Api.Middleware;
using CityPulse.Common.Api.Swagger;
using Feedback.API.Common.Filters;
using Feedback.API.Common.Services;
using Feedback.API.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authorization;

namespace Feedback.API;

public static class DependencyInjection
{
    private const string SwaggerTitle = "CityPulse Feedback API";
    private const string SwaggerDescription = "Feedback management service with Auth0 authentication";

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers(options =>
            options.Filters.Add<ErrorOrResultFilter>());

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

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
    }

    public static async Task InitializeMongoAsync(this WebApplication app)
    {
        var context = app.Services.GetRequiredService<MongoDbContext>();
        await context.CreateIndexesAsync();
    }
}
