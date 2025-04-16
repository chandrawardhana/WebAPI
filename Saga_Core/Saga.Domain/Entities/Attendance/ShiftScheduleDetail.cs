using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmshiftscheduledetail", Schema = "Attendance")]
public class ShiftScheduleDetail : AuditTrail
{
    [Required]
    public Guid ShiftScheduleKey { get; set; }

    [Required]
    public Guid ShiftDetailKey { get; set; }

    [Required]
    public DateOnly Date { get; set; }
    [MaxLength(100)]
    public string ShiftName { get; set; } = null!;

    [NotMapped]
    public ShiftSchedule? ShiftSchedule { get; set; }

    [NotMapped]
    public ShiftDetail? ShiftDetail { get; set; }

    public ShiftScheduleDetailForm ConvertToViewModelShiftScheduleDetailForm()
    {
        return new ShiftScheduleDetailForm
        {
            Key = this.Key,
            ShiftScheduleKey = this.ShiftScheduleKey,
            ShiftDetailKey = this.ShiftDetailKey,
            Date = this.Date,
            ShiftSchedule = this.ShiftSchedule,
            ShiftDetail = this.ShiftDetail,
            ShiftName = this.ShiftName
        };
    }
}
