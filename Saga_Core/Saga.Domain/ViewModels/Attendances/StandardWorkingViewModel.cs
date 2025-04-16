using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class StandardWorkingListItem
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public int? YearPeriod { get; set; } = 0;
    public int? January { get; set; } = 0;
    public int? February { get; set; } = 0;
    public int? March { get; set; } = 0;
    public int? April { get; set; } = 0;
    public int? May { get; set; } = 0;
    public int? June { get; set; } = 0;
    public int? July { get; set; } = 0;
    public int? August { get; set; } = 0;
    public int? September { get; set; } = 0;
    public int? October { get; set; } = 0;
    public int? November { get; set; } = 0;
    public int? December { get; set; } = 0;
    public Company? Company { get; set; }
}

public class StandardWorkingList
{
    public IEnumerable<StandardWorkingListItem> StandardWorkings { get; set; } = new List<StandardWorkingListItem>();
}

public class StandardWorkingForm : StandardWorkingListItem
{
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    public string? Description { get; set; } = string.Empty;

    public StandardWorkingDto ConvertToStandardWorkingDto()
    {
        return new StandardWorkingDto
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            YearPeriod = this.YearPeriod,
            January = this.January,
            February = this.February,
            March = this.March,
            April = this.April,
            May = this.May,
            June = this.June,
            July = this.July,
            August = this.August,
            September = this.September,
            October = this.October,
            November = this.November,
            December = this.December,
            Description = this.Description
        };
    }
}
