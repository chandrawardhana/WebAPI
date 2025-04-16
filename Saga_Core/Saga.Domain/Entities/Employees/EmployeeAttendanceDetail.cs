using Saga.Domain.ViewModels.Employees;

namespace Saga.Domain.Entities.Employees
{
    [Table("tbtemployeeattendancedetail", Schema = "Employee")]
    public class EmployeeAttendanceDetail : AuditTrail
    {
        [Required]
        public Guid EmployeeAttendanceKey { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;
        [Required]
        public int Quota { get; set; }
        public int? Used { get; set; }
        public int? Credit { get; set; }
        public DateOnly? ExpiredAt { get; set; }
        [Required]
        public LeaveCategory Category { get; set; }
        [Required]
        public int Priority { get; set; }

        public EmployeeAttendanceDetailForm ConvertToViewModelEmployeeAttendanceDetail(int index)
        {
            return new EmployeeAttendanceDetailForm
            {
                Key = this.Key,
                EmployeeAttendanceKey = this.EmployeeAttendanceKey,
                Name = this.Name,
                Quota = this.Quota,
                Used = this.Used,
                Credit = this.Credit,
                ExpiredAt = this.ExpiredAt,
                Category = this.Category,
                Priority = this.Priority,
                No = index + 1,
                CategoryName = Enum.GetName(typeof(LeaveCategory), this.Category)
            };
        }
    }
}
