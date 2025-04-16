namespace Saga.Domain.ViewModels.Employees;

public class EmployeeHobbyList
{
    public IEnumerable<EmployeeHobby> EmployeeHobbies { get; set; } = Enumerable.Empty<EmployeeHobby>();
}

public class EmployeeHobbyForm
{
    public Guid Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public Guid? HobbyKey { get; set; } = Guid.Empty;
    public Level? Level { get; set; } = null;
    public Hobby? Hobby { get; set; }
    public Employee? Employee { get; set; }

    //Additional No, string HobbyName and LevelText for array JsonEmployeeHobbies
    public int? No { get; set; } = 0;
    public string? HobbyName { get; set; } = String.Empty;
    public string? LevelName { get; set; } = String.Empty;
}
