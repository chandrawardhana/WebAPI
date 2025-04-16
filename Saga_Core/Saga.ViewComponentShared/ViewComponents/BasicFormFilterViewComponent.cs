
using Microsoft.AspNetCore.Mvc;
using Saga.DomainShared.Constants;
using Saga.ViewComponentShared.Interfaces;
using Saga.ViewComponentShared.Models;

namespace Saga.ViewComponentShared.ViewComponents;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class BasicFormFilterViewComponent(IFormFilter _formFilter) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(BasicFormFilterDefault def = default )
    {
        var model = await _formFilter.GetBasicFilterReportViewModelAsync();
        model.SetDefaultValue(def);
        return await Task.FromResult(View(string.Format("../{0}", AppPart.BasicFormFilter), model));
    }
}
