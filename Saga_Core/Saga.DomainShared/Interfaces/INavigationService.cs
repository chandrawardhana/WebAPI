
using Saga.DomainShared.Models;

namespace Saga.DomainShared.Interfaces;

public interface INavigationService
{
    Task<IEnumerable<NavigationMenu>> GetDefaultNavigation();
    Task<IEnumerable<AccessNavigationMenu>> GetAccessNavigation();
    Task<AccessNavigationMenu> CheckAccessNavigation(Uri uri);
}
