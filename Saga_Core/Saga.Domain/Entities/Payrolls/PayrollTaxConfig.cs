
using Saga.Domain.ViewModels.Payrolls;

namespace Saga.Domain.Entities.Payrolls;

[Table("tbmtaxconfiguration", Schema = "Payroll")]
public class PayrollTaxConfig : AuditTrail
{
    [Required, MaxLength(50)]
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
    public int Rounding {  get; set; }
    public decimal NonTaxNumberRate { get; set; }
    public PayrollTaxRate[] TaxRate { get; set; } = [];
    #endregion

    public PayrollTaxConfigViewModel ConvertToViewModel()
        => new()
        {
            Name = Name,
            Description = Description,
            PtkpYearly = PtkpYearly,
            PtkpMonthly = PtkpMonthly,
            MaxDependent = MaxDependent,
            DependentMaxAmountYearly = DependentMaxAmountYearly,
            DependentMaxAmountMonthly = DependentMaxAmountMonthly,
            AllowanceCost = AllowanceCost,
            MaxAllowanceCostYearly = MaxAllowanceCostYearly,
            MaxAllowanceCostMonthly = MaxAllowanceCostMonthly,
            Rounding = Rounding,
            NonTaxNumberRate = NonTaxNumberRate,
            TaxRate = TaxRate,

            Selected = Key
        };
}

public class PayrollTaxRate
{
    public int Rate { get; set; }
    public Int64 RangeStart { get; set; }
    public Int64 RangeEnd { get; set; }
    public decimal RatePercent { get; set; }
}
