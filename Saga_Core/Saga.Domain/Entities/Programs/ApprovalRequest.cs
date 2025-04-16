namespace Saga.Domain.Entities.Programs;

public class ApprovalRequest
{
    [Column("transaction_key")]
    public Guid TransactionKey { get; set; }
    [Column("submitter_key")]
    public Guid SubmitterKey { get; set; }
    [Column("transaction_date")]
    public DateTime ApprovalTransactionDate { get; set; }
    [Column("submitter_first_name")]
    public string SubmitterFirstName { get; set; } = string.Empty;
    [Column("submitter_last_name")]
    public string SubmitterLastName { get; set; } = string.Empty;
    [Column("submitter_email")]
    public string SubmitterEmail { get; set; } = string.Empty;
    [Column("category")]
    public ApprovalCategory Category { get; set; }
    [Column("category_name")]
    public string CategoryName { get; set; } = string.Empty;
    [Column("approval_status")]
    public ApprovalStatus ApprovalStatus { get; set; }
    [Column("status_name")]
    public string StatusName { get; set; } = string.Empty;
    [Column("description")]
    public string Description { get; set; } = string.Empty;
    [Column("reject_reason")]
    public string RejectReason { get; set; } = string.Empty;
    [Column("current_level")]
    public int CurrentLevel { get; set; }
    [Column("max_level")]
    public int? MaxLevel { get; set; }
    [Column("can_approve")]
    public bool CanApprove { get; set; }
    [Column("can_reject")]
    public bool CanReject { get; set; }
    [Column("approver_key")]
    public Guid ApproverKey { get; set; }
    [Column("approver_first_name")]
    public string ApproverFirstName { get; set; } = string.Empty;
    [Column("approver_last_name")]
    public string ApproverLastName { get; set; } = string.Empty;
    [Column("approver_email")]
    public string ApproverEmail { get; set; } = string.Empty;
}
