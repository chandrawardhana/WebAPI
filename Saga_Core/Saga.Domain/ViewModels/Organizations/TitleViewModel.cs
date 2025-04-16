using Microsoft.AspNetCore.Mvc.Rendering;

namespace Saga.Domain.ViewModels.Organizations;

public class TitleList
{
    public IEnumerable<Title> Titles { get; set; } = Enumerable.Empty<Title>();
}

public class TitleForm
{
    public Guid Key { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? CompanyKey { get; set; }
    public Company? Company { get; set; }
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
