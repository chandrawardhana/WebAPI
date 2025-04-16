namespace Saga.Domain.Dtos.Employees;

public class EmployeeAttendanceDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public string? FingerPrintID { get; set; }
    public Guid? ShiftKey { get; set; } = Guid.Empty;
    public Guid? ShiftScheduleKey { get; set; } = Guid.Empty;
    public OvertimeMode? OvertimeMode { get; set; }
    public IEnumerable<EmployeeAttendanceDetailDto>? EmployeeAttendanceDetails { get; set; } = new List<EmployeeAttendanceDetailDto>();

    public EmployeeAttendance ConvertToEntity()
    {
        return new EmployeeAttendance
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            FingerPrintID = this.FingerPrintID ?? String.Empty,
            ShiftKey = this.ShiftKey ?? Guid.Empty,
            ShiftScheduleKey = this.ShiftScheduleKey ?? Guid.Empty,
            OvertimeMode = this.OvertimeMode ?? Enums.OvertimeMode.No,
            EmployeeAttendanceDetails = this.EmployeeAttendanceDetails?.Select(x => x.ConvertToEntity())
        };
    }
}
