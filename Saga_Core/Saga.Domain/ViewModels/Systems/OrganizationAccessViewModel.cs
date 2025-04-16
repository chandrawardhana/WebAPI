
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Saga.Domain.ViewModels.Systems;

public class OrganizationAccessViewModel
{
    public List<SelectListItem> Existing { get; set; } = [];
    public Guid AccessSelected { get; set; } = Guid.Empty;
    public string AccessName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid[] AccessDetails { get; set; } = [];
    public ICollection<OrganizationAccessNode> AccessNodes { get; set; } = [];
}

public class OrganizationAccessNode
{
    public Guid Key { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public Guid? ParentKey { get; set; }
    public int Level { get; set; } = 1;
    public bool IsSelected { get; set; }
}
