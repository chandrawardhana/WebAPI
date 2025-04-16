
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Entities.Payrolls;

namespace Saga.Domain.ViewModels.Payrolls;

public class PayrollTaxConfigViewModel
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;

    #region DEPENDENT
    public int PtkpYearly { get; set; }
    public int PtkpMonthly { get; set; }
    public int MaxDependent { get; set; }
    public int DependentMaxAmountYearly { get; set; }
    public int DependentMaxAmountMonthly { get; set; }
    #endregion

    #region Allowance Cost
    public decimal AllowanceCost { get; set; }
    public int MaxAllowanceCostYearly { get; set; }
    public int MaxAllowanceCostMonthly { get; set; }
    #endregion

    #region Main Tax
    public int Rounding { get; set; }
    public decimal NonTaxNumberRate { get; set; }
    public PayrollTaxRate[] TaxRate { get; set; } = [];
    #endregion

    public SelectListItem[] Existing { get; set; } = [];
    public Guid Selected {  get; set; }
}
