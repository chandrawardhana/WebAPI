
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Saga.DomainShared.Interfaces;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing;

namespace Saga.Infrastructure.Services;

public class RazorRendererHelper(
    IRazorViewEngine _viewEngine,
    ITempDataProvider _tempDataProvider,
    IServiceProvider _serviceProvider) : IRazorRendererHelper
{
    public async Task<string> RenderViewToString<TModel>(string partialName, TModel model)
    {
        var actionContext = GetActionContext();
        var partial = FindView(actionContext, partialName);

        using (var output = new StringWriter())
        {
            var viewContext = new ViewContext(
                actionContext,
                partial,
                new ViewDataDictionary<TModel>(
                    metadataProvider: new EmptyModelMetadataProvider(),
                    modelState: new ModelStateDictionary())
                {
                    Model = model
                },
                new TempDataDictionary(
                    actionContext.HttpContext,
                    _tempDataProvider),
                output,
                new HtmlHelperOptions()
            );

            await partial.RenderAsync(viewContext).ConfigureAwait(false);
            //return output.ToString();
            return await Task.FromResult<string>(output.ToString());
        }
    }

    private IView FindView(ActionContext actionContext, string partialName)
    {
        var getPartialResult = _viewEngine.GetView(null, partialName, false);
        if (getPartialResult.Success)
        {
            return getPartialResult.View;
        }

        var findPartialResult = _viewEngine.FindView(actionContext, partialName, false);
        if (findPartialResult.Success)
        {
            return findPartialResult.View;
        }

        var searchedLocations = getPartialResult.SearchedLocations.Concat(findPartialResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find partial '{partialName}'. The following locations were searched:" }.Concat(searchedLocations)); ;
        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider
        };
        return new ActionContext(httpContext,
                                    new RouteData(),
                                    new ActionDescriptor());
    }
}
