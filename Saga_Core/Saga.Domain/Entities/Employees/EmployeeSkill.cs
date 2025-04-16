namespace Saga.Domain.Entities.Employees
{
    [Table("tbtemployeeskill", Schema = "Employee")]
    public class EmployeeSkill : AuditTrail
    {
        [Required]
        public Guid EmployeeKey { get; set; }
        [Required]
        public Guid SkillKey { get; set; }  
        [Required]
        public Level Level { get; set; }
        [Required]
        public bool IsCertificated { get; set; } = false;

        [NotMapped]
        public Employee? Employee { get; set; }
        [NotMapped]
        public Skill? Skill { get; set; }
    }
}
