namespace Saga.Domain.Dtos.Employees;

public class EmployeeSkillDto
{
    public Guid? Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public Guid? SkillKey { get; set; } = Guid.Empty;
    public Level Level { get; set; }
    public bool? IsCertificated { get; set; } = false;

    public EmployeeSkill ConvertToEntity()
    {
        return new EmployeeSkill
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            SkillKey = this.SkillKey ?? Guid.Empty,
            Level = this.Level,
            IsCertificated = this.IsCertificated ?? false
        };
    }
}
