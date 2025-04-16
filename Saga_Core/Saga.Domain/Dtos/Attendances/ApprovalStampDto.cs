using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class ApprovalStampDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? ApprovalTransactionKey { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public int Level { get; set; }
    public ApprovalStatus? Status { get; set; }
    public string? RejectReason { get; set; } = String.Empty;
    public DateTime? DateStamp { get; set; }

    public ApprovalStamp ConvertToEntity()
    {
        return new ApprovalStamp
        {
            Key = this.Key ?? Guid.Empty,
            ApprovalTransactionKey = this.ApprovalTransactionKey ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            DateStamp = this.DateStamp ?? DateTime.Now,
            Status = this.Status ?? Enums.ApprovalStatus.Waiting,
            RejectReason = this.RejectReason ?? String.Empty
        };
    }
}
