using Feedback.API;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPresentation(builder.Configuration);

var app = builder.Build();
app.UsePresentation();
await app.InitializeMongoAsync();

await app.RunAsync();
