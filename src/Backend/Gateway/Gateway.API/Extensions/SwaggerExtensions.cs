using Gateway.API.Configuration;
using Gateway.API.Constants;
using Microsoft.Extensions.Options;

namespace Gateway.API.Extensions;

public static class SwaggerExtensions
{
    public static void UseGatewaySwagger(this WebApplication app)
    {
        var settings = app.Services.GetRequiredService<IOptions<SwaggerSettings>>().Value;

        if (!settings.Enabled)
        {
            app.UseWhen(
                context => context.Request.Path.StartsWithSegments(SwaggerDefaults.PathBase),
                branch => branch.Run(context =>
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                }));

            return;
        }

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = SwaggerDefaults.RoutePrefix;

            foreach (var document in SwaggerDefaults.Documents)
            {
                options.SwaggerEndpoint(SwaggerDefaults.DocumentUrl(document.Name), document.Title);
            }

            options.OAuthClientId(settings.ClientId);
            options.OAuthScopes(
                SwaggerDefaults.OpenIdScope,
                SwaggerDefaults.ProfileScope,
                SwaggerDefaults.EmailScope);
            options.OAuthUsePkce();
            options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
            {
                [SwaggerDefaults.AudienceParam] = settings.Audience,
                [SwaggerDefaults.PromptParam] = SwaggerDefaults.PromptValue
            });
        });
    }
}
