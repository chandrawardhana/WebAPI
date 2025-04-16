
using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance
{
    [Table("tbtattendancelogmachine", Schema = "Attendance")]
    public class AttendanceLogMachine : AuditTrail
    {
        [Required]
        public Guid EmployeeKey { get; set; }
        [Required]
        public DateTime LogTime { get; set; }
        [Required]
        public InOutMode InOutMode { get; set; }

        [NotMapped]
        public Employee Employee { get; set; } = null!;

        public AttendanceLogMachineForm ConvertToAttendanceLogMachineFormViewModel()
        {
            return new AttendanceLogMachineForm
            {
                Key = this.Key,
                EmployeeKey = this.EmployeeKey,
                LogTime = this.LogTime,
                InOutMode = this.InOutMode,
                Employee = this.Employee
            };
        }
    }
}
