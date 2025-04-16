using Saga.Domain.ViewModels.Employees;

namespace Saga.Domain.Entities.Employees;

[Table("tbmemployee", Schema = "Employee")]
public class Employee : AuditTrail
{
    [Required]
    [StringLength(30)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;
    [StringLength(50)]
    public string LastName { get; set; } = String.Empty;
    [Column("PhotoKey")]
    public Guid? AssetKey { get; set; }
    [Required]
    public Guid CompanyKey { get; set; }
    [Required]
    public Guid OrganizationKey { get; set; }
    [Required]
    public Guid PositionKey { get; set; }
    [Required]
    public Guid TitleKey { get; set; }
    [Required]
    public Guid BranchKey { get; set; }
    [Required]
    public Guid GradeKey { get; set; }
    [Required]
    public DateTime HireDate { get; set; }
    [Required]
    public EmployeeStatus Status { get; set; }
    public Guid? DirectSupervisorKey { get; set; }
    public DateTime? ResignDate { get; set; } = null;
    [MaxLength(100)]
    public string? CorporateEmail { get; set; }
    [MaxLength(20)]
    public string? PhoneExtension { get; set; }

    [NotMapped]
    public Asset? Asset { get; set; }
    [NotMapped]
    public Company? Company { get; set; }
    [NotMapped]
    public Organization? Organization { get; set; }
    [NotMapped]
    public Position? Position { get; set; }
    [NotMapped]
    public Title? Title { get; set; }
    [NotMapped]
    public Branch? Branch { get; set; }
    [NotMapped]
    public Grade? Grade { get; set; }
    [NotMapped]
    public Employee? DirectSupervisor { get; set; }
    [NotMapped]
    public EmployeePersonal? EmployeePersonal { get; set; }
    [NotMapped]
    public IEnumerable<EmployeeEducation>? EmployeeEducations { get; set; }
    [NotMapped]
    public IEnumerable<EmployeeFamily>? EmployeeFamilies { get; set; }
    [NotMapped]
    public IEnumerable<EmployeeSkill>? EmployeeSkills { get; set; }
    [NotMapped]
    public IEnumerable<EmployeeHobby>? EmployeeHobbies { get; set; }
    [NotMapped]
    public IEnumerable<EmployeeLanguage>? EmployeeLanguages { get; set; }
    [NotMapped]
    public IEnumerable<EmployeeExperience>? EmployeeExperiences { get; set; }
    [NotMapped]
    public EmployeeAttendance? EmployeeAttendance { get; set; }
    [NotMapped]
    public EmployeePayroll? EmployeePayroll { get; set; }

    public DetailPopulateEmployee ConvertToViewModelDetailPopulateEmployee()
    {
        return new DetailPopulateEmployee
        {
            Key = this.Key,
            FirstName = this.FirstName,
            LastName = this.LastName,
            Code = this.Code,
            Company = this.Company,
            Organization = this.Organization,
            Position = this.Position,
            Title = this.Title,
            Branch = this.Branch,
            Grade = this.Grade,
            DirectSupervisor = this.DirectSupervisor,
            EmployeePersonal = this.EmployeePersonal
        };
    }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
