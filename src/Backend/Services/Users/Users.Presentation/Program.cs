using Users.Business;
using Users.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUsersModule(builder.Configuration,builder.Environment);
builder.Services.AddPresentation();

var app = builder.Build();

app.UsePresentation();

app.Run();
