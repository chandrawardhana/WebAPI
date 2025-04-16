using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.ViewModels.Organizations;

namespace Saga.Domain.Entities.Organizations;

[Table("tbmcity", Schema = "Organization")]
public class City : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;
    public Guid CountryKey { get; set; }
    public Guid ProvinceKey { get; set; }


    [NotMapped]
    public Country? Country { get; set; }
    [NotMapped]
    public Province? Province { get; set; }

    public CityForm ConvertToViewModelCityForm()
    {
        return new CityForm()
        {
            Key = this.Key,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description,
            CountryKey = this.CountryKey,
            Country = this.Country,
            Countries = new List<SelectListItem>(),
            ProvinceKey = this.ProvinceKey,
            Province = this.Province,
            Provinces = new List<SelectListItem>(),
            CreatedAt = this.CreatedAt,
            CreatedBy = this.CreatedBy,
            UpdatedAt = this.UpdatedAt,
            UpdatedBy = this.UpdatedBy,
            DeletedAt = this.DeletedAt,
            DeletedBy = this.DeletedBy
        };
    }

    public CityListItem ConvertToViewModelCityListItem()
    {
        return new CityListItem()
        {
            Key = this.Key,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description,
            CountryKey = this.CountryKey,
            Country = this.Country,
            ProvinceKey = this.ProvinceKey,
            Province = this.Province,
            CreatedAt = this.CreatedAt,
            CreatedBy = this.CreatedBy
        };
    }
}
