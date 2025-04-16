using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbtleavesubmission", Schema = "Attendance")]
public class LeaveSubmission : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public Guid LeaveKey { get; set; }  
    [Required]
    public DateTime DateStart { get; set; }
    [Required]
    public DateTime DateEnd { get; set; }
    [Required]
    public int Duration { get; set; }
    [Required]
    public ApprovalStatus ApprovalStatus { get; set; }

    [Column(TypeName = "text")]
    public Guid[]? Documents { get; set; } = Array.Empty<Guid>();

    [MaxLength(200)]
    public string? Description { get; set; } = String.Empty;

    [Required]
    [MaxLength(10)]
    public string LeaveCode { get; set; } = null!;

    [Required]
    [MaxLength(18)]
    public string Number { get; set; } = null!;

    [Required]
    public Guid ApprovalTransactionKey { get; set; }

    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public Leave? Leave { get; set; }
    [NotMapped]
    public ApprovalTransaction? ApprovalTransaction { get; set; }
    [NotMapped]
    public IEnumerable<Asset>? Assets { get; set; }

    public LeaveSubmissionForm ConvertToLeaveSubmissionFormViewModel()
    {
        return new LeaveSubmissionForm
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            LeaveKey = this.LeaveKey,
            DateStart = this.DateStart,
            DateEnd = this.DateEnd,
            Duration = this.Duration,
            ApprovalStatus = this.ApprovalStatus,
            Employee = this.Employee,
            Leave = this.Leave,
            ExistingDocuments = this.Documents ?? Array.Empty<Guid>(),
            DocumentFiles = null,
            Description = this.Description ?? String.Empty,
            LeaveCode = this.LeaveCode,
            Number = this.Number,
            ApprovalTransactionKey = this.ApprovalTransactionKey
        };
    }

    public LeaveSubmissionListItem ConvertToLeaveSubmissionListItem()
    {
        return new LeaveSubmissionListItem
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            LeaveKey = this.LeaveKey,
            DateStart = this.DateStart,
            DateEnd = this.DateEnd,
            Duration = this.Duration,
            ApprovalStatus = this.ApprovalStatus,
            StatusName = Enum.GetName(typeof(ApprovalStatus), this.ApprovalTransaction.ApprovalStatus),
            Number = this.Number,
            LeaveCategory = this.LeaveCode,
            Employee = this.Employee,
            Leave = this.Leave,
            Company = this.Employee.Company
        };
    }
}
