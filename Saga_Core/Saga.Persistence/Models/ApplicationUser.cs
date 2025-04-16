using Microsoft.AspNetCore.Identity;

namespace Saga.Persistence.Models;

public class ApplicationUser : IdentityUser
{
    public Guid UserId => Guid.Parse(Id);
}
