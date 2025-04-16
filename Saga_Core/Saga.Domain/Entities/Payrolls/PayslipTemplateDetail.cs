
namespace Saga.Domain.Entities.Payrolls;

/// <summary>
/// ashari.herman 2025-03-12 slipi jakarta
/// </summary>

[Table("tbmpaysliptemplatedetail", Schema = "Payroll")]
public class PayslipTemplateDetail : AuditTrail
{
    public Guid ParentKey { get; set; }
    public int Order {  get; set; }
    public Guid? ComponentKey { get; set; }
    [MaxLength(50)]
    public string DisplayName { get; set; } = string.Empty;
    public bool IsProccess { get; set; }
    public PayrollBalance Balance { get; set; }
    public bool Tax { get; set; }
    public bool TaxDeduction { get; set; }
    public bool Hide { get; set; }

}
