using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Polls.API.Authorization;

public class ConfigureJwtBearerOptions(
    IOptions<Auth0Settings> auth0Settings) : IConfigureOptions<JwtBearerOptions>
{
    private readonly Auth0Settings _auth0Settings = auth0Settings.Value;

    public void Configure(JwtBearerOptions options)
    {
        options.Authority = _auth0Settings.Authority;
        options.Audience = _auth0Settings.Audience;
    }
}
