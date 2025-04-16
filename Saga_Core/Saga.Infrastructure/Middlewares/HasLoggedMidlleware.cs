
using Microsoft.AspNetCore.Http;
using Saga.DomainShared.Interfaces;
using Saga.Infrastructure.Enums;
using Saga.Infrastructure.Extensions;

namespace Saga.Infrastructure.Middlewares;

public class HasLoggedMidlleware(RequestDelegate _next, ICurrentUser _currentUser)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path;

        var areas = (ApplicationAreas._.ToEnumerableOf<ApplicationAreas>()).ToList();

        areas.Remove(ApplicationAreas._);
        areas.Remove(ApplicationAreas.Authorization);

        path = path.TrimStart('/');
        
        var uriSegments = path.Split('/').Select(x => x.ToLower()).ToArray();

        var areaSegments = areas.Select(x => x.ToString().ToLower()).ToArray();

        if (areaSegments.Contains(uriSegments[0]) && string.IsNullOrEmpty(_currentUser.UserId))
        {
            context.Response.Redirect("/Authorization/Expired", true);
        }

        await _next(context);
    }
}
