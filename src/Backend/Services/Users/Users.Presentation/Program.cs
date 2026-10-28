using Users.Business;
using Users.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration);

var app = builder.Build();

await app.Services.ApplyUsersModuleMigrationsAsync();

app.UsePresentation();

await app.RunAsync();

public partial class Program
{
    protected Program()
    {
    }
}
