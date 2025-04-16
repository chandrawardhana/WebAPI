
namespace Saga.Domain.Dtos.Systems;

public class UserManagementDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public Guid EmployeeKey { get; set; }
    public bool IsActive { get; set; }
    public UserCategory Category { get; set; }
    public ProfileLanguage Language { get; set; }

    public Guid NavigationAccessKey { get; set; }
    public Guid OrganizationAccessKey { get; set; }

}
