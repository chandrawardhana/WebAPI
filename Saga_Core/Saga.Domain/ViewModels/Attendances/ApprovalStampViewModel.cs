using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.ViewModels.Attendances;

public class ApprovalStampListItem
{
    public Guid Key { get; set; }
    public Guid? ApprovalTransactionKey { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public int Level { get; set; }
    public ApprovalStatus? Status { get; set; }
    public string? RejectReason { get; set; } = String.Empty;
    public DateTime? DateStamp { get; set; }
    public ApprovalTransaction? ApprovalTransaction { get; set; }
    public Employee? Approver { get; set; }
}

public class ApprovalStampForm : ApprovalStampListItem
{
    public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();

    public ApprovalStampDto ConvertToApprovalStampDto()
    {
        return new ApprovalStampDto
        {
            Key = this.Key,
            ApprovalTransactionKey = this.ApprovalTransactionKey,
            EmployeeKey = this.EmployeeKey,
            Level = this.Level,
            Status = this.Status,
            RejectReason = this.RejectReason,
            DateStamp = this.DateStamp
        };
    }
}

public class ApprovalStatusItemList
{
    public int No { get; set; }
    public string Approver { get; set; } = null!;
    public string Action { get; set; } = null!;
    public ApprovalStatus? Status { get; set; } = ApprovalStatus.Waiting;
    public DateOnly? ApprovalDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public Guid? ApproverKey { get; set; }
    public int Level { get; set; }
}
