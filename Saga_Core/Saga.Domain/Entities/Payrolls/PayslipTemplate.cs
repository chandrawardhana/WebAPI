
using Saga.Domain.ViewModels.Payrolls;

namespace Saga.Domain.Entities.Payrolls;

/// <summary>
/// ashari.herman 2025-03-12 slipi jakarta
/// </summary>

[Table("tbmpaysliptemplate", Schema = "Payroll")]
public class PayslipTemplate : AuditTrail
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    [NotMapped]
    public PayslipTemplateDetail[] Details { get; set; } = [];

    public PayslipTemplateViewModel ConvertToViewModel()
        => new()
        {
            Key = Key,
            Name = Name,
            Description = Description,
            Details = Details
        };
}
