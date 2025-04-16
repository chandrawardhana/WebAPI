
namespace Saga.ViewComponentShared.Models;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class BasicFormFilterDefault
{
    public string? Find { get; set; }
    public Guid EmployeeSelected { get; set; } = Guid.Empty;
    public Guid CompanySelected { get; set; } = Guid.Empty;
    public Guid OrganizationSelected { get; set; } = Guid.Empty;
    public Guid PositionSelected { get; set; } = Guid.Empty;
    public Guid TitleSelected { get; set; } = Guid.Empty;
}
