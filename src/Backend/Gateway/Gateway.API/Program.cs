using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var tokenLimit = builder.Configuration.GetValue("RateLimiting:TokenLimit", 100);
var tokensPerPeriod = builder.Configuration.GetValue("RateLimiting:TokensPerPeriod", 10);
var replenishmentSeconds = builder.Configuration.GetValue("RateLimiting:ReplenishmentPeriodSeconds", 6);

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetTokenBucketLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = tokenLimit,
                TokensPerPeriod = tokensPerPeriod,
                ReplenishmentPeriod = TimeSpan.FromSeconds(replenishmentSeconds),
                AutoReplenishment = true,
                QueueLimit = 0
            }));
});

builder.Services.AddHealthChecks();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.UseCors();
app.UseRateLimiter();
app.MapHealthChecks("/health");
app.MapReverseProxy();

await app.RunAsync();
