using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Infrastructure.Helpers;

namespace Saga.Infrastructure.Extensions;
public static class HtmlHelperExtension
{
    public static string Translate(this IHtmlHelper htmlHelper, string word)
        => TranslatorHelper.Translate(htmlHelper.ViewContext.HttpContext, word);
}