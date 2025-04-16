
using Saga.Domain.ViewModels.Payrolls;

namespace Saga.Domain.Entities.Payrolls;

/// <summary>
/// ashari.herman 2025-03-10 slipi jakarta
/// </summary>

[Table("tbmbpjsconfig", Schema = "Payroll")]
public class BpjsConfig : AuditTrail
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
    public BpjsBaseOnCalculation BaseOnCalculation { get; set; } = BpjsBaseOnCalculation.BasicSalary;
    public decimal BaseOnFixedAmount { get; set; }
    public decimal MaxAmount{ get; set; }
    public decimal MinAmount { get; set; }    
    public string Description { get; set; } = string.Empty;

    [NotMapped]
    public BpjsSubConfig[] BpjsSubs { get; set; } = [];

    public BpjsConfigViewModel ConvertToViewModel()
        => new()
        {
            Key = Key,
            Name = Name,
            BaseOnCalculation = BaseOnCalculation,
            BaseOnFixedAmount = BaseOnFixedAmount,
            MaxAmount = MaxAmount,
            MinAmount = MinAmount,
            Description = Description,
            BpjsSubs = BpjsSubs
        };
}
