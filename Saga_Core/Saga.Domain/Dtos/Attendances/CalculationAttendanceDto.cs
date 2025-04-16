namespace Saga.Domain.Dtos.Attendances;

public class CalculationAttendanceDto
{
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid? OrganizationKey { get; set; } = Guid.Empty;
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public Guid? TitleKey { get; set; } = Guid.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
