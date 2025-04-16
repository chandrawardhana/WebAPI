using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using System.Collections;
using System.Collections.Generic;

namespace Saga.Domain.ViewModels.Attendances;

public class AttendanceForm
{
    public Guid Key { get; set; }
    public Guid? EmployeeKey { get; set; }
    public DateOnly AttendanceDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public TimeOnly? In { get; set; }
    public TimeOnly? Out { get; set; }
    public string ShiftName { get; set; } = null!;
    public AttendanceStatus Status { get; set; }
    public string? Description { get; set; } = String.Empty;
    public bool? IsMobileApp { get; set; } = false;
    public bool? IsFingerPrintMachine { get; set; } = false;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Employee? Employee { get; set; }

    public AttendanceDto ConvertToAttendanceDto()
    {
        return new AttendanceDto
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            AttendanceDate = this.AttendanceDate,
            In = this.In,
            Out = this.Out,
            ShiftName = this.ShiftName,
            Status = this.Status,
            Description = this.Description,
            IsMobileApp = this.IsMobileApp,
            IsFingerPrintMachine = this.IsFingerPrintMachine,
            Latitude = this.Latitude,
            Longitude = this.Longitude
        };
    }
}

public class AttendanceListItem : AttendanceForm
{
    public Company? Company { get; set; }
    public Organization? Organization { get; set; }
    public Position? Position { get; set; }
    public Title? Title { get; set; }
}

public class GeneralAttendanceReportFilter
{
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid? OrganizationKey { get; set; } = Guid.Empty;
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public Guid? TitleKey { get; set; } = Guid.Empty;
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = null;
    public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Organizations { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Positions { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Titles { get; set; } = new List<SelectListItem>();
    public Employee? Employee { get; set; }
    public Company? Company { get; set; }
    public Organization? Organization { get; set; }
    public Position? Position { get; set; }
    public Title? Title { get; set; }
}

public class AttendanceDailyReport : GeneralAttendanceReportFilter
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [Display(Name = "Date Range")]
    public string? DateRangeString
    {
        get => StartDate.HasValue && EndDate.HasValue
            ? $"{StartDate:MM/dd/yyyy} - {EndDate:MM/dd/yyyy}"
            : null;
        set
        {
            if (string.IsNullOrEmpty(value)) return;

            var dates = value.Split('-');
            if (dates.Length == 2)
            {
                if (DateOnly.TryParse(dates[0].Trim(), out DateOnly start))
                    StartDate = start;
                if (DateOnly.TryParse(dates[1].Trim(), out DateOnly end))
                    EndDate = end;
            }
        }
    }

    public IEnumerable<AttendanceReportData> AttendancesData { get; set; } = new List<AttendanceReportData>();

    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public AttendanceDailyReportDto ConvertToDailyReportDto()
    {
        return new AttendanceDailyReportDto
        {
            EmployeeKey = this.EmployeeKey,
            CompanyKey = this.CompanyKey,
            OrganizationKey = this.OrganizationKey,
            PositionKey = this.PositionKey,
            TitleKey = this.TitleKey,
            DocumentGeneratorFormat = this.DocumentGeneratorFormat,
            StartDate = this.StartDate,
            EndDate = this.EndDate
        };
    }
}

public class AttendanceWeeklyReport : GeneralAttendanceReportFilter
{
    public DateOnly? StartDate { get; set; }

    public IEnumerable<WeeklyAttendanceReportData> AttendancesWeeklyData { get; set; } = new List<WeeklyAttendanceReportData>();

    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public AttendanceWeeklyReportDto ConvertToWeeklyReportDto()
    {
        return new AttendanceWeeklyReportDto
        {
            EmployeeKey = this.EmployeeKey,
            CompanyKey = this.CompanyKey,
            OrganizationKey = this.OrganizationKey,
            PositionKey = this.PositionKey,
            TitleKey = this.TitleKey,
            DocumentGeneratorFormat = this.DocumentGeneratorFormat,
            StartDate = this.StartDate
        };
    }
}

public class AttendanceMonthlyReport : GeneralAttendanceReportFilter
{
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }

    public SelectList MonthOptions = new(new List<SelectListItem>());

    public SelectList YearOptions = new(new List<SelectListItem>());

    public IEnumerable<WeeklyAttendanceReportData> AttendancesMonthlyData { get; set; } = [];

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

    public AttendanceMonthlyReportDto ConvertToMonthlyReportDto()
    {
        return new AttendanceMonthlyReportDto
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

    public IDictionary<DateOnly, string> GetDays()
    {
        var days = this.AttendancesMonthlyData
                .SelectMany(x => x.DailyAttendances)
                .Select(x => x.AttendanceDate)
                .Distinct()
                .Order()
                .ToDictionary(x => x, x => x.ToString());
        return days;
    }
}

public class AttendanceRecapitulationReport : GeneralAttendanceReportFilter 
{
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }

    public SelectList MonthOptions = new SelectList(new List<SelectListItem>());

    public SelectList YearOptions = new SelectList(new List<SelectListItem>());
    public IEnumerable<RecapitulationAttendanceReportData> AttendancesRecapitulationData { get; set; } = new List<RecapitulationAttendanceReportData>();
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

    public AttendanceRecapitulationReportDto ConvertToAttendanceRecapitulationReportDto()
    {
        return new AttendanceRecapitulationReportDto
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

    public IDictionary<DateOnly, string> GetDays()
    {
        var days = this.AttendancesRecapitulationData
                .SelectMany(x => x.DailyRecaps)
                .Select(x => x.Date)
                .Distinct()
                .Order()
                .ToDictionary(x => x, x => x.ToString());
        return days;
    }
}

public class AttendanceReportData
{
    public string? EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public string? CompanyName { get; set; }
    public string? OrganizationName { get; set; }
    public string? PositionName { get; set; }
    public string? TitleName { get; set; }
    public DateOnly? AttendanceDate { get; set; }
    public string? AttendanceDay { get; set; }
    public TimeOnly? In { get; set; }
    public TimeOnly? Out { get; set; }
    public string? ShiftName { get; set; }
    public TimeOnly? WorkingHour { get; set; }
    public string? Description { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
    public bool? IsLateDocument { get; set; } = false;
}

public class WeeklyAttendanceReportData
{
    public string? NIK { get; set; }
    public string? EmployeeName { get; set; }
    public string? CompanyName { get; set; }
    public string? OrganizationName { get; set; }
    public string? PositionName { get; set; }
    public string? TitleName { get; set; }
    public IEnumerable<DailyAttendance>? DailyAttendances { get; set; } = Enumerable.Empty<DailyAttendance>();
}

public class RecapitulationAttendanceReportData
{
    public string? NIK { get; set; }
    public string? EmployeeName { get; set; }
    public string? CompanyName { get; set; }
    public string? OrganizationName { get; set; }
    public string? PositionName { get; set; }
    public string? TitleName { get; set; }
    public List<DailyRecap> DailyRecaps { get; set; } = new();
    public AttendanceTotals Totals { get; set; } = new();
}

public class DailyRecap
{
    public DateOnly Date { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class AttendanceTotals
{
    public double WorkingHours { get; set; }
    public int WorkEntries { get; set; }
    public int Alphas { get; set; }
    public Dictionary<string, int> LeaveTotals { get; set; } = new();
}

public class DailyAttendance
{
    public DateOnly AttendanceDate { get; set; }
    public TimeOnly In { get; set; }
    public TimeOnly Out { get; set; }
    public AttendanceStatus Status { get; set; }
    public bool? IsLateDocument { get; set; } = false;
}

public class PermitDetailReport : GeneralAttendanceReportFilter
{
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
    public PermitReportCategory Category { get; set; } = PermitReportCategory.EarlyOutPermit;

    public SelectList MonthOptions = new SelectList(new List<SelectListItem>());
    public SelectList YearOptions = new SelectList(new List<SelectListItem>());

    public IEnumerable<EarlyOutDetailReportData>? EarlyOutDetailReports { get; set; } = new List<EarlyOutDetailReportData>();
    public IEnumerable<OutPermitDetailReportData>? OutPermitDetailReports { get; set; } = new List<OutPermitDetailReportData>();

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

    public PermitDetailReportDto ConvertToPermitDetailReportDto()
    {
        return new PermitDetailReportDto
        {
            EmployeeKey = this.EmployeeKey,
            CompanyKey = this.CompanyKey,
            OrganizationKey = this.OrganizationKey,
            PositionKey = this.PositionKey,
            TitleKey = this.TitleKey,
            DocumentGeneratorFormat = this.DocumentGeneratorFormat,
            Category = this.Category,
            SelectedMonth = this.SelectedMonth,
            SelectedYear = this.SelectedYear
        };
    }
}

public class EarlyOutDetailReportData
{
    public string? NIK { get; set; }
    public string? EmployeeName { get; set; }
    public string? CompanyName { get; set; }
    public string? OrganizationName { get; set; }
    public string? PositionName { get; set; }
    public string? TitleName { get; set; }
    public DateOnly DateSubmission { get; set; }
    public TimeOnly TimeOut { get; set; }
    public string? Reason { get; set; }
}

public class OutPermitDetailReportData
{
    public string? NIK { get; set; }
    public string? EmployeeName { get; set; }
    public string? CompanyName { get; set; }
    public string? OrganizationName { get; set; }
    public string? PositionName { get; set; }
    public string? TitleName { get; set; }
    public DateOnly DateSubmission { get; set; }
    public TimeOnly TimeOut { get; set; }
    public TimeOnly BackToOffice { get; set; }
    public string? Reason { get; set; } = String.Empty;
}

public class ShiftScheduleDetailReport : GeneralAttendanceReportFilter
{
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
    public SelectList MonthOptions = new SelectList(new List<SelectListItem>());
    public SelectList YearOptions = new SelectList(new List<SelectListItem>());
    public IEnumerable<ShiftScheduleDetailReportData> ShiftScheduleDetailReports { get; set; } = new List<ShiftScheduleDetailReportData>();
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

    public ShiftScheduleDetailReportDto ConvertToShiftScheduleDetailReportDto()
    {
        return new ShiftScheduleDetailReportDto
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

    public IDictionary<DateOnly, string> GetDays()
    {
        var days = this.ShiftScheduleDetailReports
                .SelectMany(x => x.DailyShiftSchedules)
                .Select(x => x.Date)
                .Distinct() 
                .Order()
                .ToDictionary(x => x, x => x.ToString());
        return days;
    }
}

public class ShiftScheduleDetailReportData
{
    public string? NIK { get; set; }
    public string? EmployeeName { get; set; }
    public string? CompanyName { get; set; }
    public string? OrganizationName { get; set; }
    public string? PositionName { get; set; }
    public string? TitleName { get; set; }
    public List<DailyShiftSchedule> DailyShiftSchedules { get; set; } = new();
}

public class DailyShiftSchedule
{
    public DateOnly Date { get; set; }
    public string ShiftName { get; set; } = String.Empty;
}

