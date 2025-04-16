
using Saga.Domain.ViewModels.Systems;
using Saga.DomainShared.Enums;

namespace Saga.DomainShared.Models;

public class NavigationMenu
{
    public NavigationCategory Category { get; set; }
    public string Title { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public string AbsolutePath { get; set; } = null!;
    public ICollection<NavigationMenu> SubNavigations { get; set; } = [];

    public NavigationAccessDetailViewModel ToViewModel()
    {
        NavigationAccessDetailViewModel nav = new()
        {
            Category = (int)Category,
            Title = Title,
            AbsolutePath = AbsolutePath,
            Childs = SubNavigations.Select(x => x.ToViewModel()).ToArray()
        };
        return nav;
    }
}

public class AccessNavigationMenu : NavigationMenu
{
    public AccessNavigationMenu() { }
    public AccessNavigationMenu(NavigationMenu parent)
    {
        Category = parent.Category;
        Title = parent.Title;
        Icon = parent.Icon;
        AbsolutePath = parent.AbsolutePath;
        SubNavigations = parent.SubNavigations;
    }
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanExport { get; set; }
}
