using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Users.Business.Configurations;
using Users.Business.Helpers;
using Users.Presentation.Constants;

namespace Users.Presentation.Extensions.Swagger;

public class SwaggerSecurityConfigurer(
    IOptions<Auth0Settings> auth0) : IConfigureOptions<SwaggerGenOptions>
{
    private const string AuthorizePath = "/authorize";
    private const string TokenPath = "/oauth/token";
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
                        AuthorizationUrl = UriHelper.BuildHttpsUri(_auth0.Domain, AuthorizePath),
                        TokenUrl = UriHelper.BuildHttpsUri(_auth0.Domain, TokenPath),
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
