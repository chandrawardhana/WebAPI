using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Systems;

namespace Saga.Domain.ViewModels.Attendances;

public class EarlyOutPermitListItem
{
    public Guid Key { get; set; }
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public DateOnly DateSubmission { get; set; }
    public TimeOnly TimeOut { get; set; }
    public string? Description { get; set; } = String.Empty;
    public ApprovalStatus? ApprovalStatus { get; set; }
    public string? StatusName { get; set; }
    public string Number { get; set; } = null!;
    public Employee? Employee { get; set; }
    public Company? Company { get; set; }
}

public class EarlyOutPermitForm : EarlyOutPermitListItem
{
    public Guid? ApprovalTransactionKey { get; set; } = Guid.Empty;
    public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
    //Multiple File Upload Support
    public List<IFormFile>? DocumentFiles { get; set; }
    //Existing document keys if any
    public Guid[]? ExistingDocuments { get; set; }
    public ApprovalTransaction? ApprovalTransaction { get; set; }
    public IEnumerable<ApprovalStatusItemList> ApprovalStatuses { get; set; } = Enumerable.Empty<ApprovalStatusItemList>();
    public IEnumerable<AssetForm>? Assets { get; set; } = Enumerable.Empty<AssetForm>();

    public EarlyOutPermitDto ConvertToEarlyOutPermitDto()
    {
        return new EarlyOutPermitDto
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            DateSubmission = this.DateSubmission,
            TimeOut = this.TimeOut,
            Description = this.Description ?? String.Empty,
            ApprovalStatus = this.ApprovalStatus,
            DocumentFiles = this.DocumentFiles,
            ExistingDocuments = this.ExistingDocuments,
            Number = this.Number,
            ApprovalTransactionKey = this.ApprovalTransactionKey
        };
    }

    public IEnumerable<ApprovalStatusDto> ConvertToApprovalStatusDto()
    {
        if (ApprovalStatuses == null || !ApprovalStatuses.Any())
            return Enumerable.Empty<ApprovalStatusDto>();

        return ApprovalStatuses.Select(x => new ApprovalStatusDto
        {
            Action = x.Action,
            Status = x.Status,
            ApproverKey = x.ApproverKey,
            Level = x.Level
        });
    }
}
