using Microsoft.AspNetCore.Http;
using Saga.Domain.Dtos.Systems;
using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class OutPermitDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public DateOnly? DateSubmission { get; set; }
    public TimeOnly? OutPermitSubmission { get; set; }
    public TimeOnly? BackToWork { get; set; }
    public string? Description { get; set; } = String.Empty;
    public ApprovalStatus? ApprovalStatus { get; set; }
    public List<AssetDto>? Documents { get; set; }
    public List<IFormFile>? DocumentFiles { get; set; }
    public Guid[]? ExistingDocuments { get; set; }
    public string Number { get; set; } = null!;
    public Guid? ApprovalTransactionKey { get; set; }
    public IEnumerable<ApprovalStatusDto>? ApprovalStatuses { get; set; } = Enumerable.Empty<ApprovalStatusDto>();

    public OutPermit ConvertToEntity()
    {
        return new OutPermit
        {
            Key = this.Key ?? Guid.NewGuid(),
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            DateSubmission = this.DateSubmission ?? DateOnly.FromDateTime(DateTime.Now),
            OutPermitSubmission = this.OutPermitSubmission ?? TimeOnly.FromDateTime(DateTime.Now),
            BackToWork = this.BackToWork ?? TimeOnly.FromDateTime(DateTime.Now),
            Description = this.Description ?? String.Empty,
            ApprovalStatus = this.ApprovalStatus ?? Enums.ApprovalStatus.New,
            Documents = this.Documents?.Where(d => d.Key.HasValue && d.Key.Value != Guid.Empty)
                                       .Select(d => d.Key!.Value)
                                       .ToArray()
                                       ?? this.ExistingDocuments,
            Number = this.Number,
            ApprovalTransactionKey = this.ApprovalTransactionKey ?? Guid.Empty
        };
    }
}
