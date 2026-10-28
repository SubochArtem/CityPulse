using Polls.API;
using Polls.Application;
using Polls.Infrastructure;
using Polls.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPresentation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.ApplyMigrationsIfEnabledAsync();
app.UsePresentation();
app.UseInfrastructure(); 

await app.RunAsync();

public partial class Program
{
    protected Program()
    {
    }
}
