using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Organizations;

namespace Saga.Domain.ViewModels.Organizations;

public class ProvinceListItem
{
    public Guid Key { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? CountryKey { get; set; }
    public Country? Country { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class ProvinceList
{
    public IEnumerable<ProvinceListItem> Provinces { get; set; } = new List<ProvinceListItem>();
}

public class ProvinceForm
{
    public Guid Key { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? CountryKey { get; set; }
    public Country? Country { get; set; }
    public List<SelectListItem> Countries { get; set; } = new List<SelectListItem>();
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    //Convert current instance to ProvinceDto
    public ProvinceDto ConvertToProvinceDto()
    {
        return new ProvinceDto()
        {
            Key = this.Key,
            Code = this.Code ?? String.Empty,
            Name = this.Name ?? String.Empty,
            Description = this.Description ?? String.Empty,
            CountryKey = this.CountryKey ?? Guid.Empty
        };
    }
}
