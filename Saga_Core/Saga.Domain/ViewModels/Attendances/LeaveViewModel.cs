using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class LeaveListItem
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? Name { get; set; } = String.Empty;
    public int? MaxDays { get; set; } = 0;
    public int? MinSubmission { get; set; } = 0;
    public int? MaxSubmission { get; set; } = 0;
    public bool? IsByWeekDay { get; set; } = false;
    public bool? IsResidue { get; set; } = false;
    public Company? Company { get; set; }
}

public class LeaveList
{
    public IEnumerable<LeaveListItem> Leaves { get; set; } = new List<LeaveListItem>();
}

public class LeaveForm : LeaveListItem
{
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    public string? Description { get; set; } = String.Empty;

    public LeaveDto ConvertToLeaveDto()
    {
        return new LeaveDto
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            Code = this.Code,
            Name = this.Name,
            MaxDays = this.MaxDays,
            MinSubmission = this.MinSubmission,
            MaxSubmission = this.MaxSubmission,
            IsByWeekDay = this.IsByWeekDay,
            IsResidue = this.IsResidue,
            Description = this.Description
        };
    }
}
