
namespace Saga.Domain.Entities.Payrolls;

/// <summary>
/// ashari.herman 2025-03-10 slipi jakarta
/// </summary>

[Table("tbmbpjssubconfig", Schema = "Payroll")]
public class BpjsSubConfig : AuditTrail
{
    public Guid BpjsConfigKey { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
    public decimal Percentage { get; set; }
}
