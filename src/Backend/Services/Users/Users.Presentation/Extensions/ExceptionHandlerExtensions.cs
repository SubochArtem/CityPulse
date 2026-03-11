using Users.Presentation.Middleware;

namespace Users.Presentation.Extensions;

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
