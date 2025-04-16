
using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbtoutpermit", Schema = "Attendance")]
public class OutPermit : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public Guid ApprovalTransactionKey { get; set; }
    [Required]
    [MaxLength(18)]
    public string Number { get; set; } = null!;
    [Required]
    public DateOnly DateSubmission { get; set; }
    [Required]
    public TimeOnly OutPermitSubmission { get; set; }
    [Required]
    public TimeOnly BackToWork { get; set; }
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = null!;
    [Required]
    public ApprovalStatus ApprovalStatus { get; set; }
    [Column(TypeName = "text")]
    public Guid[]? Documents { get; set; } = Array.Empty<Guid>();

    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public ApprovalTransaction? ApprovalTransaction { get; set; }
    [NotMapped]
    public IEnumerable<Asset>? Assets { get; set; }

    public OutPermitForm ConvertToViewModelOutPermit()
    {
        return new OutPermitForm
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            ApprovalTransactionKey = this.ApprovalTransactionKey,
            Number = this.Number,
            DateSubmission = this.DateSubmission,
            OutPermitSubmission = this.OutPermitSubmission,
            BackToWork = this.BackToWork,
            Description = this.Description,
            ApprovalStatus = this.ApprovalStatus,
            Employee = this.Employee,
            ExistingDocuments = this.Documents ?? Array.Empty<Guid>(),
            DocumentFiles = null
        };
    }

    public OutPermitListItem ConvertToViewModelOutPermitListItem()
    {
        return new OutPermitListItem
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            DateSubmission = this.DateSubmission,
            OutPermitSubmission = this.OutPermitSubmission,
            BackToWork = this.BackToWork,
            Description = this.Description,
            ApprovalStatus = this.ApprovalStatus,
            StatusName = Enum.GetName(typeof(ApprovalStatus), this.ApprovalTransaction.ApprovalStatus),
            Number = this.Number,
            Employee = this.Employee,
            Company = this.Employee.Company
        };
    }
}
