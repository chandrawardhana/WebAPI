namespace Saga.Domain.Entities.Employees;

[Table("tbtemployeeexperience", Schema = "Employee")]
public class EmployeeExperience : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    [StringLength(200)]
    public string CompanyName { get; set; } = null!;
    [Required]
    public Guid PositionKey { get; set; }
    [Required]
    public int YearStart { get; set; }
    [Required]
    public int YearEnd { get; set; }

    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public Position? Position { get; set; }
}
