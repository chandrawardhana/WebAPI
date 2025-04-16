using Saga.Domain.ViewModels.Employees;

namespace Saga.Domain.Entities.Employees;

[Table("tbmnationality", Schema = "Employee")]
public class Nationality : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;

    public NationalityForm ConvertToViewModel()
    {
        return new NationalityForm
        {
            Key = Key,
            Code = Code,
            Name = Name,
            Description = Description,
        };
    }
}
