
using Microsoft.AspNetCore.Builder;
using Saga.Infrastructure.Middlewares;

namespace Saga.Infrastructure.Extensions;

public static class MiddlewareExtension
{
    public static IApplicationBuilder UseHasLoggedMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<HasLoggedMidlleware>();
}
