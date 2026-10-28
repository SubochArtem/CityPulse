using CityPulse.Common.Api.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CityPulse.Common.Api.Swagger;

public static class SwaggerConfigurationExtensions
{
    private const string SwaggerDocVersion = "v1";
    private const string SwaggerEndpointUrl = $"/swagger/{SwaggerDocVersion}/swagger.json";
    private const string SwaggerRoutePrefix = "swagger";
    private const string AudienceParam = "audience";
    private const string PromptParam = "prompt";
    private const string PromptValue = "login";

    public static void AddSwaggerConfiguration(
        this IServiceCollection services,
        string title,
        string description,
        Action<SwaggerResponseOptions>? configureResponses = null)
    {
        var responseOptions = new SwaggerResponseOptions();
        configureResponses?.Invoke(responseOptions);

        services.AddSwaggerGen(options =>
        {
            options.AddOperationFilterInstance(new ResponseSchemaOperationFilter(responseOptions));
            options.DocumentFilter<UnusedSchemaDocumentFilter>();

            options.SwaggerDoc(SwaggerDocVersion, new OpenApiInfo
            {
                Title = title,
                Version = SwaggerDocVersion,
                Description = description
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

    public static void UseSwaggerConfiguration(this WebApplication app, string title)
    {
        var auth0 = app.Services.GetRequiredService<IOptions<Auth0Settings>>().Value;

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(SwaggerEndpointUrl, $"{title} {SwaggerDocVersion}");
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
