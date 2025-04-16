
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.ViewModels.Attendances;

public class AttendancePointAppListItem
{
    public Guid Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public Double Latitude { get; set; }
    public Double Longitude { get; set; }
    public InOutMode InOutMode { get; set; }
    public DateTime AbsenceTime { get; set; }
    public Employee? Employee { get; set; }
}

public class AttendancePointAppForm : AttendancePointAppListItem
{
    public string EmployeeFullName { get; set; } = String.Empty; 
    public string TitleName { get; set; } = String.Empty;
    public string ShiftName { get; set; } = String.Empty;
    public string ShiftScheduleName { get; set; } = String.Empty;
}

public class AttendancePointItemPaginationList
{
    public Guid Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public string EmployeeId { get; set; } = null!;
    public string EmployeeName {  get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string OrganizationName { get; set; } = null!;
    public DateOnly AbsenceDate { get; set; }
    public TimeOnly AbsenceTime { get; set; }
    public string Mode { get; set; } = null!;
    public Double Latitude { get; set; }
    public Double Longitude { get; set; }
}

public class AttendancePointAppList
{
    public Guid CompanySelected { get; set; } = Guid.Empty;
    public Guid EmployeeSelected { get; set; } = Guid.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public IEnumerable<SelectListItem> Companies { get; set; } = [];
    public IEnumerable<SelectListItem> Employees { get; set; } = [];
    public IEnumerable<AttendancePointAppListItem> Attendances { get; set; } = [];
}
