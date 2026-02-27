using Microsoft.AspNetCore.Authentication;
using Users.Presentation.Authorization;
using Users.Presentation.Extensions;
using Users.Presentation.Extensions.Swagger;

namespace Users.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddControllers();
        services.AddSwaggerConfiguration();
        services.AddPermissionAuthorization();
        services.AddTransient<IClaimsTransformation, UserClaimsTransformation>();

        return services;
    }

    public static void UsePresentation(this WebApplication app)
    {
        app.UseGlobalExceptionHandler();
        app.UseSwaggerConfiguration();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
