using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Employees;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.ViewModels.Employees;

public class EmployeeAttendanceForm
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public string? FingerPrintID { get; set; } = String.Empty;
    public Guid? ShiftKey { get; set; } = Guid.Empty;
    public Guid? ShiftScheduleKey { get; set; } = Guid.Empty;
    public OvertimeMode? OvertimeMode { get; set; }
    public List<SelectListItem> Shifts { get; set; } = [];
    public List<SelectListItem> ShiftSchedules { get; set; } = [];
    public Employee? Employee { get; set; }
    public ShiftForm? Shift { get; set; }
    public ShiftScheduleForm? ShiftSchedule { get; set; }
    public IEnumerable<EmployeeAttendanceDetailForm>? EmployeeAttendanceDetails { get; set; }
    
    //For Deserialization or Serialization Input form array
    public string JsonEmployeeAttendanceDetails { get; set; } = String.Empty;

    public EmployeeAttendanceDto ConvertToEmployeeAttendanceDto()
    {
        return new EmployeeAttendanceDto
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            ShiftKey = this.ShiftKey ?? Guid.Empty,
            ShiftScheduleKey = this.ShiftScheduleKey ?? Guid.Empty,
            OvertimeMode = this.OvertimeMode ?? Enums.OvertimeMode.No,
            EmployeeAttendanceDetails = this.EmployeeAttendanceDetails?.Select(x => x.ConvertToEmployeeAttendanceDetailDto())
        };
    }
}
