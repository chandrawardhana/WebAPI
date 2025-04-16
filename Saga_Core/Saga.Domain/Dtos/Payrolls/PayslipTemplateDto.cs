
using Saga.Domain.Entities.Payrolls;

namespace Saga.Domain.Dtos.Payrolls;

/// <summary>
/// ashari.herman 2025-03-12 slipi jakarta
/// </summary>

public class PayslipTemplateDto
{
    public Guid Key { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PayslipTemplateDetail[] Details { get; set; } = [];

    public PayslipTemplate ConvertToEntity()
    => new()
    {
        Key = Key,
        Name = Name,
        Description = Description,
        Details = Details
    };
}
