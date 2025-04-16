using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class ApproverListItem
{
    public Guid ApprovalConfigKey { get; set; }
    public Guid EmployeeKey { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int Level { get; set; }
    public string Action { get; set; } = null!;
    public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
    public Employee? User { get; set; }

    public ApproverDto ConvertToApproverDto()
    {
        return new ApproverDto
        {
            ApprovalConfigKey = this.ApprovalConfigKey,
            EmployeeKey = this.EmployeeKey,
            Name = this.Name,
            Level = this.Level,
            Action = this.Action
        };
    }
}
