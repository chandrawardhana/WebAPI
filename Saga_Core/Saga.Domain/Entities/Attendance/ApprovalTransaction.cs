using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbtapprovaltransaction", Schema = "Attendance")]
public class ApprovalTransaction : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public DateTime ApprovalTransactionDate { get; set; }
    [Required]
    public ApprovalCategory Category { get; set; }
    [Required]
    public ApprovalStatus ApprovalStatus { get; set; }
    [MaxLength(200)]
    public string? RejectReason { get; set; } = String.Empty;
    [MaxLength(200)]
    public string? Description { get; set; } = String.Empty;

    [NotMapped]
    public Employee? Submitter { get; set; }
    [NotMapped]
    public IEnumerable<ApprovalStamp>? ApprovalStamps { get; set; }

    public ApprovalTransactionListItem ConvertToViewModelApprovalTransactionListItem()
    {
        return new ApprovalTransactionListItem
        {
            Key = this.Key,
            ApprovalTransactionDate = this.ApprovalTransactionDate,
            Submitter = this.Submitter,
            Category = this.Category,
            ApprovalStatus = this.ApprovalStatus,
            RejectReason = this.RejectReason,
            Description = this.Description
        };
    }

    public ApprovalTransactionForm ConvertToViewModelApprovalTransactionForm()
    {
        return new ApprovalTransactionForm
        {
            Key = this.Key,
            ApprovalTransactionDate = this.ApprovalTransactionDate,
            Category = this.Category,
            ApprovalStatus = this.ApprovalStatus,
            RejectReason = this.RejectReason,
            Description = this.Description,
            ApprovalStamps = this.ApprovalStamps?.Select(d => d.ConvertToViewModelApprovalStamp()),
            //JsonApprovalStamps would typically be handled at the presentation layer
            JsonApprovalStamps = string.Empty
        };
    }
}
