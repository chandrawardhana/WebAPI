using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class ApprovalTransactionDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid EmployeeKey { get; set; }
    public DateTime ApprovalTransactionDate { get; set; }
    public ApprovalCategory? Category { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public string? RejectReason { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public List<ApprovalStampDto>? ApprovalStamps { get; set; } = [];

    public ApprovalTransaction ConvertToEntity()
    {
        return new ApprovalTransaction
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            ApprovalTransactionDate = this.ApprovalTransactionDate,
            Category = this.Category ?? Enums.ApprovalCategory.LeavePermit,
            ApprovalStatus = this.ApprovalStatus ?? Enums.ApprovalStatus.Waiting,
            RejectReason = this.RejectReason ?? String.Empty,
            Description = this.Description ?? String.Empty,
            ApprovalStamps = this.ApprovalStamps?.Select(x => x.ConvertToEntity())
        };
    }
}
