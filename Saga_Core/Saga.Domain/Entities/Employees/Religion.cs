namespace Saga.Domain.Entities.Employees;

[Table("tbmreligion", Schema = "Employee")]
public class Religion : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;
}
