namespace Saga.Domain.Entities.Employees
{
    [Table("tbtemployeefamily", Schema = "Employee")]
    public class EmployeeFamily : AuditTrail
    {
        [Required]
        public Guid EmployeeKey { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public DateTime BoD { get; set; }
        [Required]
        public Relationship Relationship { get; set; }
        [Required]
        [StringLength(200)]
        public string Address { get; set; } = null!;
        [StringLength(20)]
        public string? PhoneNumber { get; set; } = string.Empty;

        [NotMapped]
        public Employee? Employee { get; set; }
    }
}
