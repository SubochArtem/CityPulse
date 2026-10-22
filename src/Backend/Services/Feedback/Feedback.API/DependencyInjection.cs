using Feedback.API.Authorization;
using Feedback.API.Common.Extensions.Swagger;
using Feedback.API.Common.Middleware;
using Feedback.API.Common.Services;
using Feedback.API.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Feedback.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();

        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.Configure<Auth0Settings>(configuration.GetSection(Auth0Settings.SectionName));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.ConfigureOptions<ConfigureJwtBearerOptions>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration();

        return services;
    }

    public static void UsePresentation(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseSwaggerConfiguration();
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
