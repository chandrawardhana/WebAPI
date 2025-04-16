
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Saga.Domain.ViewModels.Systems;

public class UserManagementViewModel
{
    public Guid ProfileKey { get; set; }
    public string Email { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
    public UserCategory UserCategory { get; set; }
    public ProfileLanguage Language { get; set; } = ProfileLanguage.English;
    public string NavigationAccessName { get; set; } = string.Empty;
    public string OrganizationAccessName {  get; set; } = string.Empty;

    public UserManagementViewModel() { }
    public UserManagementViewModel(UserProfile profile) 
    {
        ProfileKey = profile.Key;
        Email = profile.Email;
        EmployeeName = profile.Employee?.FullName ?? string.Empty;
        LastLogin = profile.LastLogin;
        IsActive = profile.IsActive;
        UserCategory = profile.UserCategory;
        Language = profile.Language;
        NavigationAccessName = profile.NavigationAccess?.AccessName ?? string.Empty;
        OrganizationAccessName = profile.OrganizationAccess?.AccessName ?? string.Empty;
    }
}

public class UserManagementFormViewModel : UserManagementViewModel
{
    public UserManagementFormViewModel() { }
    public UserManagementFormViewModel(UserProfile profile) : base(profile)
    {
        UserId = profile.UserId;
        EmployeeKey = profile.EmployeeKey;
        NavigationAccessKey = profile.NavigationAccessKey;
        OrganizationAccessKey = profile.OrganizationAccessKey;
    }

    public Guid UserId { get; set; }
    public Guid EmployeeKey { get; set; }
    public Guid NavigationAccessKey { get; set; }
    public Guid OrganizationAccessKey { get; set; }

    public List<SelectListItem> EmployeeList { get; set; } = [];
    public List<SelectListItem> OrganizationAccessList { get; set; } = [];
    public List<SelectListItem> NavigationAccessList { get; set; } = [];
    public List<UserCategory> UserCategories { get; set; } = [];
    public List<ProfileLanguage> ProfileLanguages { get; set; } = [];

}
