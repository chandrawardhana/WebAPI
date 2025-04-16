namespace Saga.Domain.Dtos.Employees;

public class EmployeeExperienceDto
{
    public Guid? Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public string? CompanyName { get; set; } = String.Empty;
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public int? YearStart { get; set; } = 0;
    public int? YearEnd { get; set; } = 0;

    public EmployeeExperience ConvertToEntity()
    {
        return new EmployeeExperience
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            CompanyName = this.CompanyName ?? String.Empty,
            PositionKey = this.PositionKey ?? Guid.Empty,
            YearStart = this.YearStart ?? 0,
            YearEnd = this.YearEnd ?? 0
        };
    }
}
