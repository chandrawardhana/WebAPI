using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class ApprovalConfigListItem
{
    public Guid Key { get; set; }
    public Guid OrganizationKey { get; set; }
    public string CompanyCode { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string OrganizationName { get; set; } = null!;
    public string? Description { get; set; } = String.Empty;
    public Organization? Organization { get; set; }
}

public class ApprovalConfigForm
{
    public Guid Key { get; set; }
    public Guid OrganizationKey { get; set; }
    public List<SelectListItem> Organizations { get; set; } = new List<SelectListItem>();
    public string? Description { get; set; } = String.Empty;
    public IEnumerable<ApproverListItem> Approvers { get; set; } = Enumerable.Empty<ApproverListItem>();
    public Organization? Organization { get; set; }
    //For Deserialization or Serialization Input form array
    public string JsonApprovers { get; set; } = string.Empty;

    public ApprovalConfigDto ConvertToApprovalConfigDto()
    {
        return new ApprovalConfigDto
        {
            Key = this.Key,
            OrganizationKey = this.OrganizationKey,
            Description = this.Description ?? String.Empty,
            Approvers = this.Approvers?
                            .Select(x => x.ConvertToApproverDto()).ToList() 
                            ?? new List<ApproverDto>()
        };
    }
}