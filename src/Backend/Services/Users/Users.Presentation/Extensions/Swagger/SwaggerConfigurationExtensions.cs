using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Users.Business.Configurations;
using Users.Presentation.Constants;

namespace Users.Presentation.Extensions.Swagger;

public static class SwaggerConfigurationExtensions
{
    private const string SwaggerDocVersion = "v1";
    private const string SwaggerDocTitle = "CityPulse Users API";
    private const string SwaggerDocDescription = "User management service with Auth0 authentication";
    private const string SwaggerEndpointUrl = $"/swagger/{SwaggerDocVersion}/swagger.json";
    private const string SwaggerEndpointName = $"{SwaggerDocTitle} {SwaggerDocVersion}";
    private const string SwaggerRoutePrefix = "swagger";
    private const string AudienceParam = "audience";
    private const string PromptParam = "prompt";
    private const string PromptValue = "login";

    public static void AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(SwaggerDocVersion, new OpenApiInfo
            {
                Title = SwaggerDocTitle,
                Version = SwaggerDocVersion,
                Description = SwaggerDocDescription
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = SwaggerConstants.SecurityDefinitionName
                        }
                    },
                    [
                        SwaggerConstants.OpenIdScope,
                        SwaggerConstants.ProfileScope,
                        SwaggerConstants.EmailScope
                    ]
                }
            });
        });

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerSecurityConfigurer>();
    }

    public static void UseSwaggerConfiguration(this WebApplication app)
    {
        var auth0 = app.Services.GetRequiredService<IOptions<Auth0Settings>>().Value;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(SwaggerEndpointUrl, SwaggerEndpointName);
            options.RoutePrefix = SwaggerRoutePrefix;

            options.OAuthClientId(auth0.ClientId);
            options.OAuthScopes(
                SwaggerConstants.OpenIdScope,
                SwaggerConstants.ProfileScope,
                SwaggerConstants.EmailScope);
            options.OAuthUsePkce();

            options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
            {
                [AudienceParam] = auth0.Audience,
                [PromptParam] = PromptValue
            });
        });
    }
}
