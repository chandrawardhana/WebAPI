using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Saga.DomainShared.Interfaces;
using Saga.Infrastructure.Helpers;
using Saga.Infrastructure.Interfaces;

namespace Saga.Infrastructure.Abstracts;

public abstract class BaseController<T> : Controller where T : BaseController<T>
{
    private ILogger<T> logger = null!;
    private IMediator mediator = null!;
    private ICurrentUser current = null!;
    private INavigationService navigation = null!;
    private IApplicationService applicationService = null!;

    protected ILogger<T> _logger => logger ??= HttpContext.RequestServices.GetRequiredService<ILogger<T>>();
    protected IMediator _mediator => mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
    protected ICurrentUser _currentUser => current ??= HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
    protected INavigationService _navigation => navigation ??= HttpContext.RequestServices.GetRequiredService<INavigationService>();
    protected IApplicationService _applicationService => applicationService ??= HttpContext.RequestServices.GetRequiredService<IApplicationService>();

    protected string ControllerKey => typeof(T).Name;
    protected string Translate(string word) => TranslatorHelper.Translate(HttpContext, word);

    protected void SessionSetString(string key, string value) => HttpContext.Session.SetString(key, value);
    protected string SessionGetString(string key) => HttpContext.Session.GetString(key);
    protected void SessionRemove(string key) => HttpContext.Session.Remove(key);
}
