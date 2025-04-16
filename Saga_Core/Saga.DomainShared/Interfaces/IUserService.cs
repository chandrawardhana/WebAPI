using Saga.Domain.Entities.Systems;
using System.Linq.Expressions;

namespace Saga.DomainShared.Interfaces;

public interface IUserService
{
    Task<Result> SaveUserProfile(UserProfile userProfile, CancellationToken cancellationToken);
    Task<Result> RemoveUserProfile(Guid userId, CancellationToken cancellationToken);
    Task<UserProfile?> FindUserProfile(Expression<Func<UserProfile, bool>> where);
    Task<Result> SignInAttemp(string Email, string Password);
    Task SignOut();

    Task<Result> SavePassword(UserProfile userProfile, string password, CancellationToken cancellationToken);
    Task<Result> ResetPassword(Guid userId, CancellationToken cancellationToken);
}
