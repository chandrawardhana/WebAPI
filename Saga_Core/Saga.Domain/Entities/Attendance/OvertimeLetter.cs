using Saga.Domain.ViewModels.Attendances;
using System;

namespace Saga.Domain.Entities.Attendance;

[Table("tbtovertimeletter", Schema = "Attendance")]
public class OvertimeLetter : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public Guid ApprovalTransactionKey { get; set; }
    [Required]
    public DateOnly DateSubmission { get; set; }
    [Required]
    public TimeOnly OvertimeIn { get; set; }
    [Required]
    public TimeOnly OvertimeOut { get; set; }
    [Required]
    public ApprovalStatus ApprovalStatus { get; set; }
    [Column(TypeName = "text")]
    public Guid[]? Documents { get; set; } = Array.Empty<Guid>();
    [MaxLength(200)]
    public string? Description { get; set; } = String.Empty;
    [Required]
    [MaxLength(18)]
    public string Number { get; set; } = null!;

    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public ApprovalTransaction? ApprovalTransaction { get; set; }
    [NotMapped]
    public IEnumerable<Asset>? Assets { get; set; }

    public OvertimeLetterForm ConvertToViewModelOvertime()
    {
        return new OvertimeLetterForm
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            ApprovalTransactionKey = this.ApprovalTransactionKey,
            DateSubmission = this.DateSubmission,
            OvertimeIn = this.OvertimeIn,
            OvertimeOut = this.OvertimeOut,
            Description = this.Description,
            Number = this.Number,
            ApprovalStatus = this.ApprovalStatus,
            ExistingDocuments = this.Documents ?? Array.Empty<Guid>(),
            Employee = this.Employee,
            DocumentFiles = null
        };
    }
}
