namespace Saga.Domain.ViewModels.Employees;

public class EmployeeEducationList
{
    public IEnumerable<EmployeeEducation> EmployeeEducations { get; set; } = Enumerable.Empty<EmployeeEducation>();
}

public class EmployeeEducationForm
{
    public Guid Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public Guid? EducationKey { get; set; } = Guid.Empty;
    public int? GraduatedYear { get; set; } = 0;
    public int? Score { get; set; } = 0;
    public bool? IsCertificated { get; set; } = false;
    public Employee? Employee { get; set; }
    public Education? Education { get; set; }

    //Additional No, string EducationName for array JsonEmployeeEducations
    public int? No { get; set; } = 0;
    public string? EducationName { get; set; } = string.Empty;
}
