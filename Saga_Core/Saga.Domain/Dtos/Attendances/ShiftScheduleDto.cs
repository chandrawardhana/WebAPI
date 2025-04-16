using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class ShiftScheduleDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid GroupSelected { get; set; } = Guid.Empty;
    public string? GroupName { get; set; } = String.Empty;
    public int YearPeriod { get; set; } = 0;
    public MonthName MonthPeriod { get; set; }
    public bool IsRoaster { get; set; } = false;
    public List<ShiftScheduleDetailDto>? ShiftScheduleDetails { get; set; } = new List<ShiftScheduleDetailDto>();

    public ShiftSchedule ConvertToEntity()
    {
        return new ShiftSchedule
        {
            Key = this.Key ?? Guid.Empty,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            GroupName = this.GroupName ?? String.Empty,
            YearPeriod = this.YearPeriod,
            MonthPeriod = this.MonthPeriod,
            IsRoaster = this.IsRoaster,
            ShiftScheduleDetails = this.ShiftScheduleDetails?.Select(x => x.ConvertToEntity()).ToList()
        };
    }
}
