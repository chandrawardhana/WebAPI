namespace Saga.Domain.Dtos.Employees;

public class EmployeeEducationDto
{
    public Guid? Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public Guid? EducationKey { get; set; } = Guid.Empty;
    public int? GraduatedYear { get; set; } = 0;
    public int? Score { get; set; } = 0;
    public bool? IsCertificated { get; set; } = false;

    public EmployeeEducation ConvertToEntity()
    {
        return new EmployeeEducation
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            EducationKey = this.EducationKey ?? Guid.Empty,
            GraduatedYear = this.GraduatedYear ?? 0,
            Score = this.Score ?? 0,
            IsCertificated = this.IsCertificated ?? false
        };
    }
}
