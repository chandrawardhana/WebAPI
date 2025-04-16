using Saga.Domain.Entities.Systems;
using Saga.Domain.Enums;

namespace Saga.DomainShared.Interfaces;

public interface ICurrentUser
{
    string UserId { get; }
    string Email { get; }
    ProfileLanguage Language { get; }
}
