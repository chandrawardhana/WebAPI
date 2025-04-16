namespace Saga.Domain.Entities.Programs;

public class EmployeeHierarchy
{
    [Column("employee_key")]
    public Guid EmployeeKey { get; set; }

    [Column("first_name")]
    public string? FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string? LastName { get; set; } = string.Empty;

    [Column("employee_code")]
    public string? EmployeeCode { get; set; } = string.Empty;

    [Column("phone_number")]
    public string? PhoneNumber { get; set; } = string.Empty;

    [Column("email")]
    public string? Email { get; set; } = string.Empty;

    [Column("position_name")]
    public string? PositionName { get; set; } = string.Empty;

    [Column("title_name")]
    public string? TitleName { get; set; } = string.Empty;

    [Column("org_key")]
    public Guid OrgKey { get; set; }

    [Column("org_name")]
    public string? OrgName { get; set; } = string.Empty;

    [Column("profile_image_key")]
    public Guid? ProfileImageKey { get; set; } = Guid.Empty;

    [Column("profile_image_name")]
    public string? ProfileImageName { get; set; } = string.Empty;

    [Column("profile_image_mime_type")]
    public string? ProfileImageMimeType { get; set; } = string.Empty;

    [Column("company_key")]
    public Guid CompanyKey { get; set; }
}
