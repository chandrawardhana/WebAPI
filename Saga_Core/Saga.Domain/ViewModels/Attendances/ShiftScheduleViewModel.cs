using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class ShiftScheduleForm
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid GroupSelected { get; set; } = Guid.Empty;
    public string GroupName { get; set; } = null!;
    public int YearPeriod { get; set; }
    public MonthName MonthPeriod { get; set; }
    public bool IsRoaster { get; set; }
    public Company? Company { get; set; }
    public IEnumerable<ShiftScheduleDetailForm>? ShiftScheduleDetails { get; set; }
    public string JsonShiftScheduleDetails { get; set; } = string.Empty;

    public List<SelectListItem> GroupExisting { get; set; } = [];
    public List<SelectListItem> Companies { get; set; } = [];
    public List<int> YearPeriodList { get; set; } = [];
    public List<SelectListItem> Months { get; set; } = [];
    public List<SelectListItem> Shifts { get; set; } = [];
    public WeekRow[][] WeekRows { get; set; } = [];


    public ShiftScheduleDto ConvertToShiftScheduleDto()
    {
        return new ShiftScheduleDto
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            GroupName = this.GroupName,
            YearPeriod = this.YearPeriod,
            MonthPeriod = this.MonthPeriod,
            IsRoaster = this.IsRoaster,
            ShiftScheduleDetails = this.ShiftScheduleDetails?.Select(sd => sd.ConvertToShiftScheduleDetailDto()).ToList()
        };
    }
}

public class ShiftScheduleItemList
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? GroupName { get; set; } = String.Empty;
    public int? YearPeriod { get; set; } = 0;
    public string? MonthPeriod { get; set; }
    public bool? IsRoaster { get; set; } = false;
    public Company? Company { get; set; }
}

public class ShiftScheduleList
{
    public IEnumerable<ShiftScheduleItemList>? ShiftSchedules { get; set; } = new List<ShiftScheduleItemList>();
}

public class WeekRow
{
    public string ShiftName { get; set; } = string.Empty;
    public DateOnly Date { get; set; } 
}
