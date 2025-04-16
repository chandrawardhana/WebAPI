namespace Saga.Domain.ViewModels.Employees;

public class EmployeeSkillList
{
    public IEnumerable<EmployeeSkill> EmployeeSkills { get; set; } = Enumerable.Empty<EmployeeSkill>();
}

public class EmployeeSkillForm
{
    public Guid Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public Guid? SkillKey { get; set; } = Guid.Empty;
    public Level? Level { get; set; } = null;
    public bool? IsCertificated { get; set; } = false;
    public Employee? Employee { get; set; }
    public Skill? Skill { get; set; }

    //Additional No, string SkillName and LevelText for array JsonEmployeeSkills
    public int? No { get; set; } = 0;
    public string? SkillName { get; set; } = String.Empty;
    public string? LevelName { get; set; } = String.Empty;
}
