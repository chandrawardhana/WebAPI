namespace Saga.Domain.Entities.Employees;

[Table("tbtemployeehobby", Schema = "Employee")]
public class EmployeeHobby : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public Guid HobbyKey { get; set; }
    [Required]
    public Level Level { get; set; }

    [NotMapped]
    public Hobby? Hobby { get; set; }
    [NotMapped]
    public Employee? Employee { get; set; } 
}
