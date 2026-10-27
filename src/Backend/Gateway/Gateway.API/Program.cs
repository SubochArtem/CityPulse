using Gateway.API;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGateway(builder.Configuration);

var app = builder.Build();
app.UseGateway();

await app.RunAsync();

public partial class Program
{
    protected Program()
    {
    }
}
