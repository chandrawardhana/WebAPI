
using Microsoft.AspNetCore.Http;

namespace Saga.Infrastructure.Middlewares;

public class UnauthorizedAccessMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            //var message = ex.Message;
            //await context.Response.WriteAsync(message);
            context.Response.Redirect("/Authorization/UnauthorizedAccess");
        }
    }
}
