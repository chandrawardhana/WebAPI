using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbtlatepermit", Schema = "Attendance")]
public class LatePermit : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public DateTime DateSubmission { get; set; }
    [Required]
    public TimeOnly TimeIn { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; } = String.Empty;
    [Required]
    public ApprovalStatus ApprovalStatus { get; set; }

    [Column(TypeName = "text")]
    public Guid[]? Documents { get; set; } = Array.Empty<Guid>();

    [Required]
    [MaxLength(18)]
    public string Number { get; set; } = null!;

    [Required]
    public Guid ApprovalTransactionKey { get; set; }

    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public ApprovalTransaction? ApprovalTransaction { get; set; }
    [NotMapped]
    public IEnumerable<Asset>? Assets { get; set; }

    public LatePermitForm ConvertToViewModelLatePermit()
    {
        return new LatePermitForm
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            DateSubmission = this.DateSubmission,
            TimeIn = this.TimeIn,
            Description = this.Description,
            ApprovalStatus = this.ApprovalStatus,
            Employee = this.Employee,
            ExistingDocuments = this.Documents ?? Array.Empty<Guid>(),
            DocumentFiles = null,
            Number = this.Number,
            ApprovalTransactionKey = this.ApprovalTransactionKey
        };
    }

    public LatePermitListItem ConvertToViewModelLatePermitListItem()
    {
        return new LatePermitListItem
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            DateSubmission = this.DateSubmission,
            TimeIn = this.TimeIn,
            Description = this.Description,
            ApprovalStatus = this.ApprovalStatus,
            StatusName = Enum.GetName(typeof(ApprovalStatus), this.ApprovalTransaction.ApprovalStatus),
            Number = this.Number,
            Employee = this.Employee,
            Company = this.Employee.Company
        };
    }
}
