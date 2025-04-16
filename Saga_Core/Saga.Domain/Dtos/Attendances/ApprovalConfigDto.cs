using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class ApprovalConfigDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid OrganizationKey { get; set; }
    public string? Description { get; set; }
    public List<ApproverDto> Approvers { get; set; } = [];

    public ApprovalConfig ConvertToEntity()
    {
        return new ApprovalConfig
        {
            Key = this.Key ?? Guid.Empty,
            OrganizationKey = this.OrganizationKey,
            Description = this.Description ?? String.Empty,
            Approvers = this.Approvers.Select(a => a.ConvertToEntity()).ToList()
        };
    }
}
