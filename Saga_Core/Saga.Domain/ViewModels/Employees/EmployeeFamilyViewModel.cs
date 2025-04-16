namespace Saga.Domain.ViewModels.Employees;

public class EmployeeFamilyList
{
    public IEnumerable<EmployeeFamily> Families { get; set; } = Enumerable.Empty<EmployeeFamily>();
}

public class EmployeeFamilyForm
{
    public Guid Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public string? Name { get; set; } = String.Empty;
    public Gender? Gender { get; set; } = null;
    public DateTime? BoD { get; set; } = DateTime.Now;
    public Relationship? Relationship { get; set; } = null;
    public string? Address { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; } = String.Empty;
    public Employee? Employee { get; set; }

    //Additional for show No, Gender Text and Relationship Text in list array JsonEmployeeEducations
    public int? No { get; set; } = 0;
    public string? GenderName { get; set; } = String.Empty;
    public string? RelationshipName { get; set; } = String.Empty;
}
