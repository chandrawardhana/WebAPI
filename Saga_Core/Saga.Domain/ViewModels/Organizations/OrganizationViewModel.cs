using Microsoft.AspNetCore.Mvc.Rendering;

namespace Saga.Domain.ViewModels.Organizations;

public class OrganizationList
{
    public IEnumerable<Organization> Organizations { get; set; } = Enumerable.Empty<Organization>();
}

public class ParentList
{
    public Guid Key { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public Guid? ParentKey { get; set; }
    public int Level { get; set; } = 1;
    public Organization? Parent { get; set; }
    public Company? Company { get; set; }
}

public class OrganizationForm
{
    public Guid Key { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public Guid? ParentKey { get; set; } = Guid.Empty;
    public Organization? Parent { get; set; }
    public List<SelectListItem> Parents { get; set; } = new List<SelectListItem>();
    public int Level { get; set; } = 1;
    
    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
    public Guid CompanyKey { get; set; }
    //for get selected Company in form detail
    public Company? Company { get; set; }
}
