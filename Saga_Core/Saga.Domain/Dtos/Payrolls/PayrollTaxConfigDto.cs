
using Saga.Domain.Entities.Payrolls;
using Saga.Domain.ViewModels.Payrolls;

namespace Saga.Domain.Dtos.Payrolls;

public class PayrollTaxConfigDto
{
    public Guid Key { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;

    public int PtkpYearly { get; set; }
    public int PtkpMonthly { get; set; }
    public int MaxDependent { get; set; }
    public int DependentMaxAmountYearly { get; set; }
    public int DependentMaxAmountMonthly { get; set; }

    public decimal AllowanceCost { get; set; }
    public int MaxAllowanceCostYearly { get; set; }
    public int MaxAllowanceCostMonthly { get; set; }

    public int Rounding { get; set; }
    public decimal NonTaxNumberRate { get; set; }
    public PayrollTaxRate[] TaxRate { get; set; } = [];

    public PayrollTaxConfig ConvertToEntity()
        => new()
        {
            Key = Key,
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
            TaxRate = TaxRate
        };

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
