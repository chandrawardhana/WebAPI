using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class ApproverDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid ApprovalConfigKey { get; set; }
    public Guid EmployeeKey { get; set; }
    public string Name { get; set; } = null!;
    public int Level { get; set; }
    public string Action { get; set; } = null!;

    public Approver ConvertToEntity()
    {
        return new Approver
        {
            Key = this.Key ?? Guid.Empty,
            ApprovalConfigKey = this.ApprovalConfigKey,
            EmployeeKey = this.EmployeeKey,
            Name = this.Name,
            Level = this.Level,
            Action = this.Action
        };
    }
}
