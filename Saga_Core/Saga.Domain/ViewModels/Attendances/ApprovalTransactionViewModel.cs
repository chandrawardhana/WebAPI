using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class ApprovalTransactionListItem
{
    public Guid Key { get; set; }
    public DateTime ApprovalTransactionDate { get; set; }
    public Employee? Submitter { get; set; }
    public ApprovalCategory? Category { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public string? RejectReason { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}

public class ApprovalRequestItemList
{
    public Guid TransactionKey { get; set; }
    public Guid SubmitterKey { get; set; }
    public DateTime ApprovalTransactionDate { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public string SubmitterEmail { get; set; } = string.Empty;
    public ApprovalCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public ApprovalStatus ApprovalStatus { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RejectReason { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public int? MaxLevel { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public Guid ApproverKey { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverEmail { get; set; } = string.Empty;
    //For pop up ApprovalStamp (ApprovalStatus)
    public IEnumerable<ApprovalStampListItem> ApprovalStamps { get; set; } = Enumerable.Empty<ApprovalStampListItem>();
}

public class ApprovalTransactionForm : ApprovalTransactionListItem
{
    public IEnumerable<ApprovalStampForm>? ApprovalStamps {  get; set; }

    //For Deserialization or Serialization Input form array
    public string JsonApprovalStamps { get; set; } = string.Empty;

    public ApprovalTransactionDto ConvertToApprovalTransactionDto()
    {
        return new ApprovalTransactionDto
        {
            Key = this.Key,
            Category = this.Category,
            ApprovalStatus = this.ApprovalStatus,
            RejectReason = this.RejectReason,
            Description = this.Description,
            ApprovalStamps = this.ApprovalStamps?.
                            Select(x => x.ConvertToApprovalStampDto()).ToList()
        };
    }
} 
