namespace Saga.Domain.Dtos.Employees;

public class EmployeeFamilyDto
{
    public Guid? Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public string? Name { get; set; } = String.Empty;
    public Gender Gender { get; set; }
    public DateTime? BoD { get; set; } = DateTime.Now;
    public Relationship Relationship { get; set; }
    public string? Address { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; } = String.Empty;

    public EmployeeFamily ConvertToEntity()
    {
        return new EmployeeFamily
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            Name = this.Name ?? String.Empty,
            Gender = this.Gender,
            BoD = this.BoD ?? DateTime.Now,
            Relationship = this.Relationship,
            Address = this.Address ?? String.Empty,
            PhoneNumber = this.PhoneNumber
        };
    }
}
