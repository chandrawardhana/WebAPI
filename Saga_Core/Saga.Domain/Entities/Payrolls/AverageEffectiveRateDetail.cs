
namespace Saga.Domain.Entities.Payrolls;

[Table("tbmaverageeffectiveratedetail", Schema = "Payroll")]
public class AverageEffectiveRateDetail : AuditTrail
{
    public Guid ParentKey { get; set; }
    public int Order {  get; set; }
    public Int64 RangeStart { get; set; }
    public Int64 RangeEnd { get; set; }
    public decimal RatePercentage { get; set; }
}
