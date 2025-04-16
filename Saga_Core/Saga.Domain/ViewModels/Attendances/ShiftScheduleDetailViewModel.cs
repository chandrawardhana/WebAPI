using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.ViewModels.Attendances;

public class ShiftScheduleDetailForm
{
    public Guid Key { get; set; }
    public Guid? ShiftScheduleKey { get; set; }
    public Guid? ShiftDetailKey { get; set; }
    public DateOnly? Date { get; set; }
    public string ShiftName { get; set; }
    public ShiftSchedule? ShiftSchedule { get; set; }
    public ShiftDetail? ShiftDetail { get; set; }

    //for dropdown shift name
    public IEnumerable<SelectListItem>? ShiftNameList { get; set; }

    public void InitializeShiftNameList(IEnumerable<ShiftDetail> shiftDetails)
    {
        ShiftNameList = shiftDetails.Select(sd => new SelectListItem
        {
            Value = sd.Key.ToString(),
            Text = sd.WorkName
        });
    }

    public ShiftScheduleDetailDto ConvertToShiftScheduleDetailDto()
    {
        return new ShiftScheduleDetailDto
        {
            Key = this.Key,
            ShiftScheduleKey = this.ShiftScheduleKey,
            ShiftDetailKey = this.ShiftDetailKey,
            Date = this.Date,
            ShiftName = this.ShiftName
        };
    }
}
