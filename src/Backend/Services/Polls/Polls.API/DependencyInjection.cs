using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Polls.API.Authorization;
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

        services.Configure<Auth0Settings>(configuration.GetSection("Auth0"));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.ConfigureOptions<ConfigureJwtBearerOptions>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    public static void UsePresentation(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
