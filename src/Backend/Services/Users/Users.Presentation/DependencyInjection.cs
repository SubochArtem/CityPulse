using CityPulse.Common.Api.Extensions;
using CityPulse.Common.Api.Swagger;
using Users.Presentation.Authorization;
using Users.Presentation.Extensions;

namespace Users.Presentation;

public static class DependencyInjection
{
    private const string SwaggerTitle = "CityPulse Users API";
    private const string SwaggerDescription = "User management service with Auth0 authentication";

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers();
        services.AddUnifiedApiErrors();
        services.AddSwaggerConfiguration(SwaggerTitle, SwaggerDescription);
        services.AddPermissionAuthorization();

        services.AddCityPulseAuthentication(configuration);

        return services;
    }

    public static void UsePresentation(this WebApplication app)
    {
        app.UseGlobalExceptionHandler();
        app.UseUnifiedStatusCodePages();
        app.UseSwaggerConfiguration(SwaggerTitle);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
