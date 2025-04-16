
namespace Saga.Domain.Entities.Systems;

[Table("tbmuserprofile", Schema = "System")]
public class UserProfile : AuditTrail
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid EmployeeKey { get; set; }
    public string IdentityPass { get; set; } = null!;
    public string? Token { get; set; }
    public string? TokenIdentity { get; set; }
    public string? TokenApi { get; set; }
    public string? TokenResetPassword { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
    public bool IsConfirmed { get; set; }
    public UserCategory UserCategory { get; set; }
    public ProfileLanguage Language { get; set; } = ProfileLanguage.English;

    public Guid NavigationAccessKey { get; set; }
    public Guid OrganizationAccessKey { get; set; }

    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public NavigationAccess? NavigationAccess { get; set; }
    [NotMapped]
    public OrganizationAccess? OrganizationAccess { get; set; }

    
}
