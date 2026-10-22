using Feedback.API.Authorization;
using Feedback.API.Constants;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Feedback.API.Common.Extensions.Swagger;

public class SwaggerSecurityConfigurer(
    IOptions<Auth0Settings> auth0) : IConfigureOptions<SwaggerGenOptions>
{
    private const string OpenIdScopeDescription = "OpenID";
    private const string ProfileScopeDescription = "Profile";
    private const string EmailScopeDescription = "Email";
    private const string AuthorizationEndpoint = "authorize";
    private const string TokenEndpoint = "oauth/token";

    private readonly Auth0Settings _auth0 = auth0.Value;

    public void Configure(SwaggerGenOptions options)
    {
        var auth0AuthorityUri = new Uri(_auth0.Authority);

        options.AddSecurityDefinition(
            SwaggerConstants.SecurityDefinitionName,
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(auth0AuthorityUri, AuthorizationEndpoint),
                        TokenUrl = new Uri(auth0AuthorityUri, TokenEndpoint),

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
