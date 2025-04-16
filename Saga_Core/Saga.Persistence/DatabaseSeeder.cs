using Microsoft.AspNetCore.Identity;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Entities.Organizations;
using Saga.DomainShared.Enums;
using Saga.Persistence.Models;
using System.Data;
using System.Security.Claims;

namespace Saga.Persistence;

public class DatabaseSeeder
{
    public static async Task DefaultRoleAsync(RoleManager<IdentityRole> roleManager)
    {
        IdentityRole[] roles = Enum.GetNames(typeof(Role)).Select(s => new IdentityRole(s)).ToArray();
        foreach (var role in roles)
        {
            if (roleManager.Roles.All(r => r.Name != role.Name))
                await roleManager.CreateAsync(role);
        }
    }

    public static async Task DefaultUserAsync(
        UserManager<ApplicationUser> _userManager,
        RoleManager<IdentityRole> _roleManager
        //IMediator mediator
    )
    {
        var role = new IdentityRole(Role.SuperAdmin.ToString());
        var roles = _roleManager.Roles;

        string email = "administrator@sag.com";
        string password = "Zxcxz@123";

        var administrator = new ApplicationUser
        {
            UserName = email,
            Email = email
        };

        var claims = new List<Claim>(){
            //new (ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            //new (ClaimTypes.Name, Role.SuperAdmin.ToString().ToUpper()),
            //new (ClaimTypes.Email, administrator.Email),
            //new (ClaimTypes.Role, Role.SuperAdmin.ToString()),
            //new (ClaimTypes.GroupSid, Guid.NewGuid().ToString())
        };

        if (!_userManager.Users.Any(u => u.Email == administrator.Email))
        {
            var result = await _userManager.CreateAsync(administrator, password);
            if (result.Succeeded)
            {
                await _userManager.AddClaimsAsync(administrator, claims);

                if (roles.All(r => r.Name != role.Name))
                    await _roleManager.CreateAsync(role);

                await _userManager.AddToRolesAsync(administrator, new[] { role.Name });
                await _userManager.AddClaimsAsync(administrator, claims);
            }
        }

        #region
            /*
            if (userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                // branch
                Branch branch = new()
                {
                    Key = Guid.NewGuid(),
                    Code = "SYS",
                    Name = NamingSystem.BranchSystem.ToTitle()
                };
                var branchResult = await mediator.Send(new SaveBranchCommand(Guid.Empty) { Form = branch });

                // employee
                Employee employee = new()
                {
                    Key = Guid.NewGuid(),
                    FirstName = NamingSystem.SuperAdmin.ToTitle(),
                    LastName = string.Empty,
                    Email = administrator.Email,
                    Phone = string.Empty,
                    JoinDate = DateTime.UtcNow,
                    Address = string.Empty,
                    BranchKey = branch.Key,
                    RecordingPosition = RecordingPosition.HeadOffice

                };
                var employeeResult = await mediator.Send(new SaveEmployeeCommand(Guid.Empty) { Form = employee });

                // branch access
                BranchAccess branchAccess = new()
                {
                    Key = Guid.NewGuid(),
                    Name = NamingSystem.SuperAdminAccess.ToTitle(),
                    Description = string.Empty,
                    Branchs = new List<string>()
                };
                var branchAccessResult = await mediator.Send(new SaveBranchAccessCommand(Guid.Empty) { Form = branchAccess });

                // menu access
                MenuAccessForm menuAccess = new()
                {
                    Key = Guid.NewGuid(),
                    Name = NamingSystem.SuperAdminAccess.ToTitle(),
                    Description = string.Empty,
                    AccessDetails = new List<MenuAccessDetail>()
                };
                var menuAccessResult = await mediator.Send(new SaveMenuAccessCommand(Guid.Empty) { Form = menuAccess });

                string password = "Zxcxz@123";

                // profile
                UserForm userProfile = new()
                {
                    EmployeeKey = employee.Key.ToString(),
                    BranchAccessKey = branchAccess.Key.ToString(),
                    MenuAccessKey = menuAccess.Key.ToString(),
                    Email = administrator.Email,
                    IsActive = true,
                    Password = password,
                };

                var saveUser = await mediator.Send(new SaveUserManagementCommand(Guid.Empty) { Form = userProfile });
                if (saveUser.Succeeded)
                {
                    if (roles.All(r => r.Name != role.Name))
                        await roleManager.CreateAsync(role);

                    //await userManager.CreateAsync(administrator, password);
                    await userManager.AddToRolesAsync(administrator, new[] { role.Name });
                    await userManager.AddClaimsAsync(administrator, claims);
                }
            }
            */
            #endregion
    }
}
