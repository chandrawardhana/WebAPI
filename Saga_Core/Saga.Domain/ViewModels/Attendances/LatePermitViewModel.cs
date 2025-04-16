using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Systems;

namespace Saga.Domain.ViewModels.Attendances;

public class LatePermitListItem
{
    public Guid Key { get; set; }
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public DateTime? DateSubmission { get; set; }
    public TimeOnly? TimeIn { get; set; }
    public string? Description { get; set; } = String.Empty;
    public ApprovalStatus? ApprovalStatus { get; set; }
    public string? StatusName { get; set; }
    public string Number { get; set; } = null!;
    public string? LeaveCategory { get; set; }
    public Employee? Employee { get; set; }
    public Company? Company { get; set; }
}


public class LatePermitForm : LatePermitListItem
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

    public LatePermitDto ConvertToLatePermitDto()
    {
        return new LatePermitDto
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            DateSubmission = this.DateSubmission,
            TimeIn = this.TimeIn,
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

public class  LateDetailReport : GeneralAttendanceReportFilter
{
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }

    public SelectList MonthOptions = new SelectList(new List<SelectListItem>());

    public SelectList YearOptions = new SelectList(new List<SelectListItem>());

    public IEnumerable<LateDetailReportData> LatePermitReportDetails { get; set; } = new List<LateDetailReportData>();

    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public void InitializeMonthOptions(int? selectedMonth = null)
    {
        var indonesianCulture = new System.Globalization.CultureInfo("id-ID");
        var months = indonesianCulture.DateTimeFormat.MonthNames
            .Take(12)
            .Select((monthName, index) => new SelectListItem
            {
                Value = (index + 1).ToString(),
                Text = char.ToUpper(monthName[0]) + monthName.Substring(1)
            }).ToList();

        MonthOptions = new SelectList(months, "Value", "Text", selectedMonth?.ToString());
    }

    public void InitializeYearOptionsRange(int? selectedYear = null)
    {
        // Get current year
        int currentYear = DateTime.Now.Year;

        // Create range from 2020 to current year
        var years = Enumerable.Range(2020, currentYear - 2020 + 1)
            .Select(year => new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            })
            .ToList();

        YearOptions = new SelectList(years, "Value", "Text", selectedYear?.ToString());
    }

    public LateDetailReportDto ConvertToLateDetailReportDto()
    {
        return new LateDetailReportDto
        {
            EmployeeKey = this.EmployeeKey,
            CompanyKey = this.CompanyKey,
            OrganizationKey = this.OrganizationKey,
            PositionKey = this.PositionKey,
            TitleKey = this.TitleKey,
            DocumentGeneratorFormat = this.DocumentGeneratorFormat,
            SelectedMonth = this.SelectedMonth,
            SelectedYear = this.SelectedYear
        };
    }
}

public class LateDetailReportData
{
    public string? NIK { get; set; }
    public string? EmployeeName { get; set; }
    public string? CompanyName { get; set; }
    public string? OrganizationName { get; set; }
    public string? PositionName { get; set; }
    public string? TitleName { get; set; }
    public DateOnly? LatePermitDate { get; set; }
    public TimeOnly? TimeIn { get; set; }
    public bool? IsLateDocument { get; set; } = false;


    public DateOnly Date { get; set; }
    public TimeOnly TimeOut { get; set; }
    public TimeOnly Late {  get; set; }
    public bool IsPermited { get; set; }
    public TimeOnly WorkingHour { get; set; }
    public string? Description { get; set; } 
}
