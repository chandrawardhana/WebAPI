using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbtapprovalstamp", Schema = "Attendance")]
public class ApprovalStamp : AuditTrail
{
    [Required]
    public Guid ApprovalTransactionKey { get; set; }
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public int Level { get; set; }
    [Required]
    public ApprovalStatus Status { get; set; }
    [MaxLength(200)]
    public string? RejectReason { get; set; } = String.Empty;
    [Required]
    public DateTime DateStamp { get; set; }
    
    [NotMapped]
    public ApprovalTransaction? ApprovalTransaction { get; set; }
    [NotMapped]
    public Employee? Approver { get; set; }

    public ApprovalStampForm ConvertToViewModelApprovalStamp()
    {
        return new ApprovalStampForm
        {
            Key = this.Key,
            ApprovalTransactionKey = this.ApprovalTransactionKey,
            EmployeeKey = this.EmployeeKey,
            Level = this.Level,
            Status = this.Status,
            RejectReason = this.RejectReason,
            DateStamp = this.DateStamp,
            ApprovalTransaction = this.ApprovalTransaction,
            Approver = this.Approver
        };
    }
}
