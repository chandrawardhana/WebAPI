namespace Saga.Domain.Entities.Employees
{
    [Table("tbtemployeeeducation", Schema = "Employee")]
    public class EmployeeEducation : AuditTrail
    {
        [Required]
        public Guid EmployeeKey { get; set; }
        [Required]
        public Guid EducationKey { get; set; }
        public int? GraduatedYear { get; set; }
        public int? Score { get; set; }
        [Required]
        public bool IsCertificated { get; set; } = false;

        [NotMapped]
        public Employee? Employee { get; set; }
        [NotMapped]
        public Education? Education { get; set; }
    }
}
