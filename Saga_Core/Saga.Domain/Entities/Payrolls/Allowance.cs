
using Saga.Domain.ViewModels.Payrolls;

namespace Saga.Domain.Entities.Payrolls;

/// <summary>
/// ashari.herman 2025-03-10 slipi jakarta
/// </summary>

[Table("tbmallowance", Schema = "Payroll")]
public class Allowance : AuditTrail
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
    public bool IsFixedAllowance { get; set; }
    public TaxVariable TaxVariable { get; set; }
    public string Description { get; set; } = string.Empty;

    [NotMapped]
    public AllowanceSub[] AllowanceSubs { get; set; } = [];

    public AllowanceViewModel ConvertToViewModel()
        => new()
        {
            Key = Key,
            Name = Name,
            IsFixedAllowance = IsFixedAllowance,
            TaxVariable = TaxVariable,
            Description = Description,
            AllowanceSubs = AllowanceSubs
        };
}
