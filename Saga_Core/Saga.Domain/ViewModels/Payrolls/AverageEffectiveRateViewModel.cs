
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Entities.Payrolls;

namespace Saga.Domain.ViewModels.Payrolls;

public class AverageEffectiveRateViewModel
{
    public SelectListItem[] Existing { get; set; } = [];
    public TaxStatus[] MasterTaxStatuses { get; set; } = [];

    public Guid Selected {  get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public TaxStatus[] TaxStatuses { get; set; } = [];
    public string Description { get; set; } = string.Empty;

    public AverageEffectiveRateDetail[] Details { get; set; } = [];
}
