using Polls.API;
using Polls.Application;
using Polls.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPresentation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UsePresentation();
app.UseInfrastructure(); 

await app.RunAsync();
