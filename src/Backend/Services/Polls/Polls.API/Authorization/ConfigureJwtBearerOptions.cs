using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Polls.API.Authorization;

public class ConfigureJwtBearerOptions(
    IOptions<Auth0Settings> auth0Settings) : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly Auth0Settings _auth0Settings = auth0Settings.Value;

    public void Configure(string? name, JwtBearerOptions options)
    {
        options.Authority = $"https://{_auth0Settings.Domain}/";
        options.Audience = _auth0Settings.Audience;
    }

    public void Configure(JwtBearerOptions options)
        => Configure(null, options);
}
