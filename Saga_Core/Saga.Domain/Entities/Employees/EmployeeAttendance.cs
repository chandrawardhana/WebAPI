using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Employees;

namespace Saga.Domain.Entities.Employees
{
    [Table("tbtemployeeattendance", Schema = "Employee")]
    public class EmployeeAttendance : AuditTrail
    {
        [Required]
        public Guid EmployeeKey { get; set; }
        [StringLength(20)]
        public string? FingerPrintID { get; set; } = String.Empty;
        [Required]
        public Guid ShiftKey { get; set; }
        [Required]
        public Guid ShiftScheduleKey { get; set; }
        [Required]
        public OvertimeMode OvertimeMode { get; set; }

        [NotMapped]
        public Employee? Employee { get; set; }
        [NotMapped]
        public Shift? Shift { get; set; }
        [NotMapped]
        public ShiftSchedule? ShiftSchedule { get; set; }
        [NotMapped]
        public IEnumerable<EmployeeAttendanceDetail>? EmployeeAttendanceDetails { get; set; }

        public EmployeeAttendanceForm ConvertToEmployeeAttendanceForm()
        {
            return new EmployeeAttendanceForm
            {
                Key = this.Key,
                EmployeeKey = this.EmployeeKey,
                FingerPrintID = this.FingerPrintID,
                ShiftKey = this.ShiftKey,
                ShiftScheduleKey = this.ShiftScheduleKey,
                Employee = this.Employee,
                Shift = this.Shift.ConvertToViewModelShiftForm(),
                ShiftSchedule = this.ShiftSchedule.ConvertToViewModelShiftScheduleForm(),
                OvertimeMode = this.OvertimeMode,
                EmployeeAttendanceDetails = this.EmployeeAttendanceDetails?.Select((detail, index) => detail.ConvertToViewModelEmployeeAttendanceDetail(index)),
                JsonEmployeeAttendanceDetails = String.Empty
            };
        }
    }
}
