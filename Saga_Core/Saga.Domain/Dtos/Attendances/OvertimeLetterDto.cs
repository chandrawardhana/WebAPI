using Microsoft.AspNetCore.Http;
using Saga.Domain.Dtos.Systems;
using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class OvertimeLetterDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public DateOnly? DateSubmission { get; set; }
    public TimeOnly OvertimeIn { get; set; }
    public TimeOnly OvertimeOut { get; set; }
    public string? Description { get; set; } = String.Empty;
    public ApprovalStatus? ApprovalStatus { get; set; }
    public List<AssetDto>? Documents { get; set; }
    public List<IFormFile>? DocumentFiles { get; set; }
    public Guid[]? ExistingDocuments { get; set; }
    public string Number { get; set; } = null!;
    public Guid? ApprovalTransactionKey { get; set; }
    public IEnumerable<ApprovalStatusDto>? ApprovalStatuses { get; set; } = Enumerable.Empty<ApprovalStatusDto>();

    public OvertimeLetter ConvertToEntity()
    {
        return new OvertimeLetter 
        {
            Key = this.Key ?? Guid.NewGuid(),
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            DateSubmission = this.DateSubmission ?? DateOnly.FromDateTime(DateTime.Now),
            OvertimeIn = this.OvertimeIn,
            OvertimeOut = this.OvertimeOut,
            Description = this.Description,
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

public class OvertimeLetterDetailReportDto : GeneralAttendanceReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Xlsx;
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
}
