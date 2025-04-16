
using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbtattendancepointapp", Schema = "Attendance")]
public class AttendancePointApp : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public Double Latitude { get; set; }
    [Required]
    public Double Longitude { get; set; }
    [Required]
    public InOutMode InOutMode { get; set; }
    [Required]
    public DateTime AbsenceTime { get; set; }

    [NotMapped]
    public Employee Employee { get; set; } = null!;

    public AttendancePointAppListItem ConvertToAttendancePointAppViewModel()
    {
        return new AttendancePointAppListItem
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            Latitude = this.Latitude,
            Longitude = this.Longitude,
            InOutMode = this.InOutMode,
            AbsenceTime = this.AbsenceTime,
            Employee = this.Employee
        };
    }

    public AttendancePointAppForm ConvertToAttendancePointAppFormViewModel(Title? title = null, Shift? shift = null, ShiftSchedule? shiftSchedule = null)
    {
        return new AttendancePointAppForm
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            Latitude = this.Latitude,
            Longitude = this.Longitude,
            InOutMode = this.InOutMode,
            AbsenceTime = this.AbsenceTime,
            Employee = this.Employee,
            EmployeeFullName = (this.Employee.FirstName ?? String.Empty) + " " + (this.Employee.LastName ?? String.Empty),
            TitleName = title != null ? title.Name : String.Empty,
            ShiftName = shift != null ? shift.ShiftGroupName : String.Empty,
            ShiftScheduleName = shiftSchedule != null ? shiftSchedule.GroupName : String.Empty
        };
    }
}
