namespace Saga.Domain.Entities.Employees;

[Table("tbmskill", Schema = "Employee")]
public class Skill : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;
}
