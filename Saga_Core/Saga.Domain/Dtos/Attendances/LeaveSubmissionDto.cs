using Microsoft.AspNetCore.Http;
using Saga.Domain.Dtos.Systems;
using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class LeaveSubmissionDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public Guid? LeaveKey { get; set; } = Guid.Empty;
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public int? Duration { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public List<AssetDto>? Documents { get; set; }
    public List<IFormFile>? DocumentFiles { get; set; }
    public Guid[]? ExistingDocuments { get; set; }
    public string? Description { get; set; } = string.Empty;
    public string LeaveCode { get; set; } = null!;
    public string Number { get; set; } = null!;
    public Guid? ApprovalTransactionKey { get; set; }

    public IEnumerable<ApprovalStatusDto>? ApprovalStatuses { get; set; } = Enumerable.Empty<ApprovalStatusDto>();
    public Leave? Leave { get; set; }

    public LeaveSubmission ConvertToEntity()
    {
        return new LeaveSubmission
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            LeaveKey = this.LeaveKey ?? Guid.Empty,
            DateStart = this.DateStart ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 08, 00, 00),
            DateEnd = this.DateEnd ?? DateTime.Now.AddDays(1).AddTicks(-1),
            Duration = this.Duration ?? 0,
            ApprovalStatus = this.ApprovalStatus ?? Enums.ApprovalStatus.New,
            Documents = this.Documents?.Where(d => d.Key.HasValue && d.Key.Value != Guid.Empty)
                                       .Select(d => d.Key!.Value)
                                       .ToArray() 
                                       ?? this.ExistingDocuments
                                       ?? Array.Empty<Guid>(),
            Description = this.Description ?? String.Empty,
            LeaveCode = this.LeaveCode,
            Number = this.Number,
            ApprovalTransactionKey = this.ApprovalTransactionKey ?? Guid.Empty,
            Leave = this.Leave
        };
    }
}

public class LeaveDetailReportDto : GeneralAttendanceReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Xlsx;
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
}