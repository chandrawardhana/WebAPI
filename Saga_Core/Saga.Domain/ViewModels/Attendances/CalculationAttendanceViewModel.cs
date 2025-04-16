using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;
using System.Globalization;

namespace Saga.Domain.ViewModels.Attendances;

public class CalculationAttendanceForm
{
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid? OrganizationKey { get; set; } = Guid.Empty;
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public Guid? TitleKey { get; set; } = Guid.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [Display(Name = "Date Range")]
    public string? DateRangeString
    {
        get => StartDate.HasValue && EndDate.HasValue
            ? $"{StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd}"
            : null;
        set
        {
            if (string.IsNullOrEmpty(value)) return;

            // Split by " - " (with spaces)
            var dates = value.Split(new[] { " - " }, StringSplitOptions.None);
            if (dates.Length == 2)
            {
                if (DateOnly.TryParseExact(dates[0].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly start))
                    StartDate = start;
                if (DateOnly.TryParseExact(dates[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly end))
                    EndDate = end;
            }
        }
    }

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

    public CalculationAttendanceDto ConvertToCalculationDto()
    {
        return new CalculationAttendanceDto
        {
            EmployeeKey = this.EmployeeKey,
            CompanyKey = this.CompanyKey,
            OrganizationKey = this.OrganizationKey,
            PositionKey = this.PositionKey,
            TitleKey = this.TitleKey,
            StartDate = this.StartDate,
            EndDate = this.EndDate
        };
    }
}
