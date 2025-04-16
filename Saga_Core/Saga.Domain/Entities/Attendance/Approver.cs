
using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance
{
    [Table("tbmapprover", Schema = "Attendance")]
    public class Approver : AuditTrail
    {
        [Required]
        public Guid ApprovalConfigKey { get; set; }
        [Required]
        public Guid EmployeeKey { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        public int Level { get; set; }
        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = null!;

        [NotMapped]
        public Employee? User { get; set; }
        [NotMapped]
        public ApprovalConfig? ApprovalConfig { get; set; }

        public ApproverListItem ConvertToApproverListItem()
        {
            return new ApproverListItem
            {
                ApprovalConfigKey = this.ApprovalConfigKey,
                EmployeeKey = this.EmployeeKey,
                Name = this.Name,
                Level = this.Level,
                Action = this.Action,
                User = this.User,
            };
        }
    }
}
