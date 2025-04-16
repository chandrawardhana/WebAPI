
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Saga.Domain.ViewModels.Systems;

public class NavigationAccessViewModel
{
    public List<SelectListItem> Existing { get; set; } = [];
    public Guid AccessSelected {  get; set; } = Guid.Empty;
    public string AccessName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<NavigationAccessDetailViewModel> AccessDetails { get; set; } = [];
}

public class NavigationAccessDetailViewModel
{
    public int Category { get; set; }
    public string Title { get; set; } = null!;
    public string AbsolutePath { get; set; } = null!;
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanExport { get; set; }
    public ICollection<NavigationAccessDetailViewModel> Childs { get; set; } = [];

}
