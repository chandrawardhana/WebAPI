namespace Saga.Domain.Dtos.Employees;

public class EmployeeAttendanceDetailDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeAttendanceKey { get; set; } = Guid.Empty;
    public string? Name { get; set; }
    public int? Quota { get; set; }
    public int? Used { get; set; }
    public int? Credit { get; set; }
    public DateOnly? ExpiredAt { get; set; }
    public LeaveCategory Category { get; set; }
    public int? Priority { get; set; }

    public EmployeeAttendanceDetail ConvertToEntity()
    {
        return new EmployeeAttendanceDetail
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeAttendanceKey = this.EmployeeAttendanceKey ?? Guid.Empty,
            Name = this.Name ?? String.Empty,
            Quota = this.Quota ?? 0,
            Used = this.Used ?? 0,
            Credit = this.Credit ?? 0,
            ExpiredAt = ExpiredAt ?? DateOnly.FromDateTime(DateTime.Now),
            Category = this.Category,
            Priority = this.Priority ?? 0
        };
    }
}
