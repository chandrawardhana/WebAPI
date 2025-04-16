using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class ShiftScheduleDetailDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? ShiftScheduleKey { get; set; } = Guid.Empty;
    public Guid? ShiftDetailKey { get; set; } = Guid.Empty;
    public DateOnly? Date { get; set; }
    public string ShiftName { get; set; } = string.Empty;

    public ShiftScheduleDetail ConvertToEntity()
    {
        return new ShiftScheduleDetail
        {
            Key = this.Key ?? Guid.Empty,
            ShiftScheduleKey = this.ShiftScheduleKey ?? Guid.Empty,
            ShiftDetailKey = this.ShiftDetailKey ?? Guid.Empty,
            Date = this.Date ?? DateOnly.FromDateTime(DateTime.Now),
            ShiftName = this.ShiftName
        };
    }
}
