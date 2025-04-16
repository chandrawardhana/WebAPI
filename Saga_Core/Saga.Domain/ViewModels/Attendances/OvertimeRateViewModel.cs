using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class OvertimeRateItemList
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? GroupName { get; set; } = String.Empty;
    public Day BaseOnDay { get; set; }
    public int? MaxHour { get; set; } = 0;
    public Company? Company { get; set; }
}

public class OvertimeRateList
{
    public IEnumerable<OvertimeRateItemList> OvertimeRates { get; set; } = new List<OvertimeRateItemList>();
}

public class OvertimeRateForm
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public List<SelectListItem> GroupExisting { get; set; } = [];
    public List<SelectListItem> Companies { get; set; } = [];
    public List<SelectListItem> Days { get; set; } = [];
    public string? GroupName { get; set; } = String.Empty;
    public Day BaseOnDay { get; set; }
    public int? MaxHour { get; set; } = 0;
    public Guid CompanySelected { get; set; } = Guid.Empty;
    public Guid GroupSelected { get; set; } = Guid.Empty;
    public IEnumerable<OvertimeRateDetailForm>? OvertimeRateDetails { get; set; }

    //For Deserialization or Serialization Input form array
    public string JsonOvertimeRateDetails { get; set; } = string.Empty;

    public OvertimeRateDto ConvertToOvertimeRateDto()
    {
        return new OvertimeRateDto
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            GroupName = this.GroupName ?? String.Empty,
            BaseOnDay = this.BaseOnDay,
            MaxHour = this.MaxHour ?? 0,
            OvertimeRateDetails = this.OvertimeRateDetails?.Select(x => x.ConvertToOvertimeRateDetailDto())?.ToList() ?? new List<OvertimeRateDetailDto>()
        };
    }
}
