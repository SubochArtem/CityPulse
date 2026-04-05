using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polls.API.Authorization;
using Polls.API.Constants;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Polls.API.Common.Extensions.Swagger;

public class SwaggerSecurityConfigurer(
    IOptions<Auth0Settings> auth0) : IConfigureOptions<SwaggerGenOptions>
{
    private const string OpenIdScopeDescription = "OpenID";
    private const string ProfileScopeDescription = "Profile";
    private const string EmailScopeDescription = "Email";

    private readonly Auth0Settings _auth0 = auth0.Value;

    public void Configure(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition(
            SwaggerConstants.SecurityDefinitionName,
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"https://{_auth0.Domain}/authorize"),
                        TokenUrl = new Uri($"https://{_auth0.Domain}/oauth/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { SwaggerConstants.OpenIdScope, OpenIdScopeDescription },
                            { SwaggerConstants.ProfileScope, ProfileScopeDescription },
                            { SwaggerConstants.EmailScope, EmailScopeDescription }
                        }
                    }
                }
            });
    }
}
