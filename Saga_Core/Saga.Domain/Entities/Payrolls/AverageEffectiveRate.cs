
namespace Saga.Domain.Entities.Payrolls;

[Table("tbmaverageeffectiverate", Schema = "Payroll")]
public class AverageEffectiveRate : AuditTrail
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    public TaxStatus[] TaxStatuses { get; set; } = [];
    public string Description {  get; set; } = string.Empty;

    [NotMapped]
    public AverageEffectiveRateDetail[] Details { get; set; } = [];
}
