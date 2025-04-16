namespace Saga.Domain.Entities.Employees;

[Table("tbtemployeelanguage", Schema = "Employee")]
public class EmployeeLanguage : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public Guid LanguageKey { get; set; }
    [Required]
    public Level SpeakLevel { get; set; }
    [Required]
    public Level ListenLevel { get; set; }

    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public Language? Language { get; set; } 
}
