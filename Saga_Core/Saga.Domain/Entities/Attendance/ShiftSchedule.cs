using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmshiftschedule", Schema = "Attendance")]
public class ShiftSchedule : AuditTrail
{
    [Required]
    public Guid CompanyKey { get; set; }
    [Required]
    [StringLength(200)]
    public string GroupName { get; set; } = null!;
    [Required]
    public int YearPeriod { get; set; }
    [Required]
    public MonthName MonthPeriod { get; set; }
    public bool? IsRoaster { get; set; } = false;

    [NotMapped]
    public Company? Company { get; set; }
    [NotMapped]
    public IEnumerable<ShiftScheduleDetail>? ShiftScheduleDetails { get; set; }

    public ShiftScheduleItemList ConvertToViewModelShiftScheduleItemList()
    {
        return new ShiftScheduleItemList
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            GroupName = this.GroupName,
            YearPeriod = this.YearPeriod,
            MonthPeriod = this.MonthPeriod.ToString(),
            IsRoaster = this.IsRoaster,
            Company = this.Company
        };
    }

    public ShiftScheduleForm ConvertToViewModelShiftScheduleForm()
    {
        return new ShiftScheduleForm
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            GroupName = this.GroupName,
            YearPeriod = this.YearPeriod,
            MonthPeriod = this.MonthPeriod,
            IsRoaster = this.IsRoaster ?? false,
            Company = this.Company,
            ShiftScheduleDetails = this.ShiftScheduleDetails?.Select(x => x.ConvertToViewModelShiftScheduleDetailForm()).ToList(),
            JsonShiftScheduleDetails = String.Empty
        };
    }
}
