
namespace Saga.Domain.Entities.Payrolls;

/// <summary>
/// ashari.herman 2025-03-10 slipi jakarta
/// </summary>

[Table("tbmallowancesub", Schema = "Payroll")]
public class AllowanceSub : AuditTrail
{ 
    public Guid AllowanceKey { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
    public AllowanceCategory Category { get; set; }
    public decimal Amount { get; set; }
}
