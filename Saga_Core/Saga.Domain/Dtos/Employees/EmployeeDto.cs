using Microsoft.AspNetCore.Http;

namespace Saga.Domain.Dtos.Employees;

public class EmployeeDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? FirstName { get; set; } = String.Empty;
    public string? LastName { get; set; } = String.Empty;
    public Guid? AssetKey { get; set; } = Guid.Empty;
    public IFormFile? Photo { get; set; } = null;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid? OrganizationKey { get; set; } = Guid.Empty;
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public Guid? TitleKey { get; set; } = Guid.Empty;
    public Guid? BranchKey { get; set; } = Guid.Empty;
    public Guid? GradeKey { get; set; } = Guid.Empty;
    public DateTime? HireDate { get; set; } = DateTime.Now;
    public EmployeeStatus? Status { get; set; } = null;
    public Guid? DirectSupervisorKey { get; set; } = Guid.Empty;
    public DateTime? ResignDate { get; set; } = null;
    public string? CorporateEmail { get; set; } = String.Empty;
    public string? PhoneExtension { get; set; } = String.Empty;

    //Add EmployeePersonalDto for personal details
    public EmployeePersonalDto? EmployeePersonal { get; set; } = null;

    //Add EmployeeEducationDto for education details
    public List<EmployeeEducationDto>? EmployeeEducations { get; set; } = new List<EmployeeEducationDto>();

    //Add EmployeeExperienceDto for experience details
    public List<EmployeeExperienceDto>? EmployeeExperiences { get; set; } = new List<EmployeeExperienceDto>();

    //Add EmployeeFamilyDto for family details
    public List<EmployeeFamilyDto>? EmployeeFamilies { get; set; } = new List<EmployeeFamilyDto>();

    //Add EmployeeHobbyDto for hobby details
    public List<EmployeeHobbyDto>? EmployeeHobbies { get; set; } = new List<EmployeeHobbyDto>();

    //Add EmployeeLanguageDto for language details
    public List<EmployeeLanguageDto>? EmployeeLanguages { get; set; } = new List<EmployeeLanguageDto>();

    //Add EmployeeSkillDto for skill details
    public List<EmployeeSkillDto>? EmployeeSkills { get; set; } = new List<EmployeeSkillDto>();

    //Add EmployeeAttendanceDto for attendance details
    public EmployeeAttendanceDto? EmployeeAttendance { get; set; } = null;

    //Add EmployeePayrolDto for payroll details
    public EmployeePayrollDto? EmployeePayroll { get; set; } = null;

    public Employee ConvertToEntity()
    {
        return new Employee
        {
            Key = this.Key ?? Guid.Empty,
            Code = this.Code ?? String.Empty,
            FirstName = this.FirstName ?? String.Empty,
            LastName = this.LastName ?? String.Empty,
            AssetKey = this.AssetKey ?? Guid.Empty,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            OrganizationKey = this.OrganizationKey ?? Guid.Empty,
            PositionKey = this.PositionKey ?? Guid.Empty,
            TitleKey = this.TitleKey ?? Guid.Empty,
            BranchKey = this.BranchKey ?? Guid.Empty,
            GradeKey = this.GradeKey ?? Guid.Empty,
            HireDate = this.HireDate ?? DateTime.Now,
            Status = this.Status ?? EmployeeStatus.Contract,
            DirectSupervisorKey = this.DirectSupervisorKey ?? Guid.Empty,
            ResignDate = this.ResignDate ?? null,
            CorporateEmail = this.CorporateEmail ?? String.Empty,
            PhoneExtension = this.PhoneExtension ?? String.Empty,
            EmployeePersonal = this.EmployeePersonal?.ConvertToEntity(),
            EmployeeEducations = this.EmployeeEducations?.Select(e => e.ConvertToEntity()).ToList(),
            EmployeeExperiences = this.EmployeeExperiences?.Select(e => e.ConvertToEntity()).ToList(),
            EmployeeFamilies = this.EmployeeFamilies?.Select(e => e.ConvertToEntity()).ToList(),
            EmployeeHobbies = this.EmployeeHobbies?.Select(e => e.ConvertToEntity()).ToList(),
            EmployeeLanguages = this.EmployeeLanguages?.Select(e => e.ConvertToEntity()).ToList(),
            EmployeeSkills = this.EmployeeSkills?.Select(e => e.ConvertToEntity()).ToList(),
            EmployeeAttendance = this.EmployeeAttendance?.ConvertToEntity(),
            EmployeePayroll = this.EmployeePayroll?.ConvertToEntity()
        };
    }
}

public class GeneralEmployeeReportDto
{
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid? OrganizationKey { get; set; } = Guid.Empty;
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public Guid? TitleKey { get; set; } = Guid.Empty;
    public Guid? GradeKey { get; set; } = Guid.Empty;
    public EmployeeStatus? Status { get; set; } = null;
}

public class EmployeeReportDto : GeneralEmployeeReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Xlsx;
}

public class CurriculumVitaeReportDto : GeneralEmployeeReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Pdf;
}

public class GraphTurnOverDto : GeneralEmployeeReportDto
{
    public int? SelectedYear { get; set; }
}

public class GraphManPowerDto : GeneralEmployeeReportDto
{
    public List<int> GenderValues { get; set; } = new List<int>();
    public List<Guid> EducationKeys { get; set; } = new List<Guid>();
    public List<Guid> ReligionKeys { get; set; } = new List<Guid>();
    public List<String> AgeRanges { get; set; } = new List<String>();
}

