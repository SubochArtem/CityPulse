using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Polls.API.Authorization;
using Polls.API.Common.Extensions.Swagger;
using Polls.API.Common.Filters;
using Polls.API.Common.Middleware;

namespace Polls.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddControllers(options =>
            options.Filters.Add<ResultFilter>());

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
}
