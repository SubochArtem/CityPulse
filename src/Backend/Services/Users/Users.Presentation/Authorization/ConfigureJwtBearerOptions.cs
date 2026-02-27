using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Users.Business.Configurations;
using Users.Business.Helpers;

namespace Users.Presentation.Authorization;

public class ConfigureJwtBearerOptions(
    IOptions<Auth0Settings> auth0Settings) : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly Auth0Settings _auth0Settings = auth0Settings.Value;

    public void Configure(string? name, JwtBearerOptions options)
    {
        options.Authority = UriHelper.BuildHttpsUri(_auth0Settings.Domain).ToString();
        options.Audience = _auth0Settings.Audience;
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(null, options);
    }
}
