using Microsoft.AspNetCore.Mvc.Rendering;

namespace Saga.Domain.ViewModels.Employees;

public class EmployeeExperienceList
{
    public IEnumerable<EmployeeExperience> EmployeeExperiences { get; set; } = Enumerable.Empty<EmployeeExperience>();
}

public class EmployeeExperienceForm
{
    public Guid? Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public string? CompanyName { get; set; } = String.Empty;
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public List<SelectListItem> Positions { get; set; } = new List<SelectListItem>();
    public int? YearStart { get; set; } = 0;
    public int? YearEnd { get; set; } = 0;
    public Employee? Employee { get; set; }
    public Position? Position { get; set; }

    //Additional No, string PositionName for array JsonEmployeeExperiences
    public int? No { get; set; } = 0;
    public string? PositionName { get; set; } = String.Empty;
}
