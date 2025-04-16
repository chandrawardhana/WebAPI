namespace Saga.Domain.Dtos.Employees;

public class EmployeeHobbyDto
{
    public Guid? Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public Guid? HobbyKey { get; set; } = Guid.Empty;
    public Level Level { get; set; }

    public EmployeeHobby ConvertToEntity()
    {
        return new EmployeeHobby
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            HobbyKey = this.HobbyKey ?? Guid.Empty,
            Level = this.Level
        };
    }
}
