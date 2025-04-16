using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.ViewModels.Systems;

namespace Saga.Domain.ViewModels.Employees;

public class EmployeeList
{
    public IEnumerable<Employee> Employees { get; set; } = Enumerable.Empty<Employee>();
}

public class EmployeeItemPagination
{
    public Employee Employee { get; set; } = null!;
    public DateTimeDuration Age { get; set; } = null!;
    public DateTimeDuration LongOfJoin { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public Position Position { get; set; } = null!;
    public Title Title { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Grade Grade { get; set; } = null!;
    public Employee? DirectSupervisor { get; set; } = null;
}

public class DirectSupervisorList
{
    public Guid Key { get; set; }
    public string? Code { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
}

public class EmployeeForm
{
    public Guid Key { get; set; }
    public string? Code { get; set; } = string.Empty;
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public Guid? PhotoKey { get; set; } = Guid.Empty;
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
    public AssetForm? Asset { get; set; }
    public IFormFile? Photo { get; set; }
    public Company? Company { get; set; }
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    public Organization? Organization { get; set; }
    public List<SelectListItem> Organizations { get; set; } = new List<SelectListItem>();
    public Position? Position { get; set; }
    public List<SelectListItem> Positions { get; set; } = new List<SelectListItem>();
    public Title? Title { get; set; }
    public List<SelectListItem> Titles { get; set; } = new List<SelectListItem>();
    public Branch? Branch { get; set; }
    public List<SelectListItem> Branches { get; set; } = new List<SelectListItem>();
    public Grade? Grade { get; set; }
    public List<SelectListItem> Grades { get; set; } = new List<SelectListItem>();
    public Employee? DirectSupervisor { get; set; }
    public List<SelectListItem> DirectSupervisors { get; set; } = new List<SelectListItem>();
    public EmployeePersonalForm? EmployeePersonal { get; set; }

    //For Displaying Age and LongOfJoin at Report Employee Excel
    public DateTimeDuration Age { get; set; } = null!;
    public DateTimeDuration LongOfJoin { get; set; } = null!;

    public IEnumerable<EmployeeEducationForm>? EmployeeEducations { get; set; }
    public IEnumerable<EmployeeExperienceForm>? EmployeeExperiences { get; set; }
    public IEnumerable<EmployeeFamilyForm>? EmployeeFamilies { get; set; }
    public IEnumerable<EmployeeSkillForm>? EmployeeSkills { get; set; }
    public IEnumerable<EmployeeHobbyForm>? EmployeeHobbies { get; set; }
    public IEnumerable<EmployeeLanguageForm>? EmployeeLanguages { get; set; }
    public EmployeeAttendanceForm? EmployeeAttendance { get; set; }
    public EmployeePayrollForm? EmployeePayroll { get; set; }

    //For Deserialization or Serialization Input form array
    public string JsonEmployeeEducations { get; set; } = string.Empty;
    public string JsonEmployeeExperiences { get; set; } = string.Empty;
    public string JsonEmployeeFamilies { get; set; } = string.Empty;
    public string JsonEmployeeSkills { get; set; } = string.Empty;
    public string JsonEmployeeHobbies { get; set; } = string.Empty;
    public string JsonEmployeeLanguages { get; set; } = string.Empty;
}

public class GeneralReportFilter
{
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid? OrganizationKey { get; set; } = Guid.Empty;
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public Guid? TitleKey { get; set; } = Guid.Empty;
    public Guid? GradeKey { get; set; } = Guid.Empty;
    public EmployeeStatus? Status { get; set; } = null;
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = null;
    public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Organizations { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Positions { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Titles { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Grades { get; set; } = new List<SelectListItem>();
    public Employee? Employee { get; set; }
    public Company? Company { get; set; }
    public Organization? Organization { get; set; }
    public Position? Position { get; set; }
    public Title? Title { get; set; }
    public Grade? Grade { get; set; }
}

public class EmployeeReport : GeneralReportFilter
{
    public IEnumerable<EmployeeForm> EmployeesData { get; set; } = new List<EmployeeForm>();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}

public class GraphManPowerFilter : GeneralReportFilter
{
    [Column(TypeName = "text")]
    public string? TagsGender { get; set; } = String.Empty;

    [Column(TypeName = "text")]
    public string? TagsEducation { get; set; } = String.Empty;

    [Column(TypeName = "text")]
    public string? TagsReligion { get; set; } = String.Empty;

    [Column(TypeName = "text")]
    public string? TagsAge { get; set; } = String.Empty;

    public List<int> GenderValues {
        get => string.IsNullOrEmpty(TagsGender)
            ? new List<int>()
             : TagsGender.Split(',').Select(int.Parse).ToList();
        set => TagsGender = string.Join(",", value);
    }

    public List<Guid> EducationKeys {
        get => string.IsNullOrEmpty(TagsEducation)
            ? new List<Guid>()
             : TagsEducation.Split(',').Select(Guid.Parse).ToList();
        set => TagsEducation = string.Join(",", value);
    }

    public List<Guid> ReligionKeys {
        get => string.IsNullOrEmpty(TagsReligion)
            ? new List<Guid>()
             : TagsReligion.Split(',').Select(Guid.Parse).ToList();
        set => TagsReligion = string.Join(",", value);
    }

    public List<string> AgeRanges
    {
        get => string.IsNullOrEmpty(TagsAge)
            ? new List<string>()
            : TagsAge.Split(',').ToList();
        set => TagsAge = string.Join(",", value);
    }

    public MultiSelectList GenderTags { get; set; } = new MultiSelectList(new List<SelectListItem>());
    public MultiSelectList EducationTags { get; set; } = new MultiSelectList(new List<SelectListItem>());
    public MultiSelectList ReligionTags { get; set; } = new MultiSelectList(new List<SelectListItem>());
    public MultiSelectList AgeTags { get; set; } = new MultiSelectList(new List<SelectListItem>());

    // Helper method to initialize age ranges
    public void InitializeAgeTags(IEnumerable<string> selectedRanges = null)
    {
        var ages = Enumerable.Range(16, 55); // 55 is the count to reach 70 (16 + 54 = 70)

        var ageRanges = ages
            .Select((age, index) => new { Age = age, Index = index })
            .GroupBy(x => x.Index / 5)
            .Select(group =>
            {
                var firstAge = group.First().Age;
                var lastAge = Math.Min(firstAge + 4, 70);
                return new SelectListItem
                {
                    Text = $"{firstAge}-{lastAge}",
                    Value = $"{firstAge}-{lastAge}"
                };
            })
            .ToList();

        AgeTags = new MultiSelectList(ageRanges, "Value", "Text", selectedRanges);
    }
}

public class GraphTurnOverFilter : GeneralReportFilter
{
    public int? SelectedYear { get; set; }
    public SelectList YearOptions = new SelectList(new List<SelectListItem>());

    public void InitializeYearOptionsRange(int? selectedYear = null)
    {
        // Get current year
        int currentYear = DateTime.Now.Year;

        // Create range from 2020 to current year
        var years = Enumerable.Range(2020, currentYear - 2020 + 1)
            .Select(year => new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            })
            .ToList();

        // Add default option at the beginning
        years.Insert(0, new SelectListItem { Value = "", Text = "-- Select Year --" });

        YearOptions = new SelectList(years, "Value", "Text", selectedYear?.ToString());
    }
}

public class CurriculumVitaeReport : GeneralReportFilter
{
    public IEnumerable<EmployeeForm> CurriculumVitae { get; set; } = new List<EmployeeForm>();
}

//For GraphTurnOverData
public class GraphTurnOverData
{
    public int Year { get; set; }
    public List<MonthlyTurnOverData> MonthlyData { get; set; } = new List<MonthlyTurnOverData>();
}

public class MonthlyTurnOverData
{
    public int Month { get; set; }
    public string? MonthName { get; set; } = String.Empty;
    public int? EmployeesIn { get; set; } = 0;
    public int? EmployeesOut { get; set; } = 0;
    public int? EmployeesExisting { get; set; } = 0;
}

public class GraphManPowerReport : GraphManPowerFilter
{
    public Dictionary<string, int> GenderData { get; set; }
    public Dictionary<string, int> EducationData { get; set; }
    public Dictionary<string, int> ReligionData { get; set; }
    public Dictionary<string, int> AgeData { get; set; }
}

public class DetailPopulateEmployee
{
    public Guid Key { get; set; }
    public string? FirstName { get; set; } = String.Empty;
    public string? LastName { get; set; } = String.Empty;
    public string? Code { get; set; } = String.Empty;
    public Company? Company { get; set; }
    public Organization? Organization { get; set; }
    public Position? Position { get; set; }
    public Title? Title { get; set; }
    public Branch? Branch { get; set; }
    public Grade? Grade { get; set; }
    public Employee? DirectSupervisor { get; set; }
    public EmployeePersonal? EmployeePersonal { get; set; }
}