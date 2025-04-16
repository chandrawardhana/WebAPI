
using Saga.Domain.ViewModels;
using Saga.DomainShared.Interfaces;

namespace Saga.Infrastructure.Interfaces;

public interface IApplicationService : 
    IAuditLogger, 
    IUserService
{
}
