using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Employees;

namespace Saga.Domain.ViewModels.Employees;

public class EmployeeAttendanceDetailListItem
{
    public Guid Key { get; set; }
    public Guid EmployeeAttendanceKey { get; set; }
    public string Name { get; set; } = null!;
    public int Quota { get; set; }
    public int? Used { get; set; } = 0;
    public int? Credit { get; set; } = 0;
    public DateOnly? ExpiredAt { get; set; }
    public LeaveCategory Category { get; set; }
    public int? Priority { get; set; }
}

public class EmployeeAttendanceDetailList
{
    public IEnumerable<EmployeeAttendanceDetailListItem> EmployeeAttendanceDetails { get; set; } = new List<EmployeeAttendanceDetailListItem>();
}

public class  EmployeeAttendanceDetailForm : EmployeeAttendanceDetailListItem
{
    public SelectList PriorityOptions = new SelectList(new List<SelectListItem>());
    public void InitializePriorityOptionsRange(int? selectedPriority = null)
    {
        var range = Enumerable.Range(1, 10)
                              .Select(x => new SelectListItem(x.ToString(), x.ToString()));

        PriorityOptions = new SelectList(range, "Value", "Text", selectedPriority?.ToString());
    }

    public int? No { get; set; } = 0;
    public string? CategoryName { get; set; } = string.Empty;

    public EmployeeAttendanceDetailDto ConvertToEmployeeAttendanceDetailDto()
    {
        return new EmployeeAttendanceDetailDto
        {
            Key = this.Key,
            EmployeeAttendanceKey = this.EmployeeAttendanceKey,
            Name = this.Name,
            Quota = this.Quota,
            Used = this.Used,
            Credit = this.Credit,
            ExpiredAt = this.ExpiredAt,
            Category = this.Category,
            Priority = this.Priority
        };
    }
}

public class LeaveQuota
{
    public int No { get; set; }
    public string LeaveName { get; set; } = null!;
    public int Credit { get; set; }
    public DateOnly Expired { get; set; }
}
