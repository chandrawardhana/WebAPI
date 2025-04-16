using Saga.Domain.ViewModels.Organizations;

namespace Saga.Domain.Entities.Organizations;

[Table("tbmcountry", Schema = "Organization")]
public class Country : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;

    [NotMapped]
    public ICollection<Province>? Provinces { get; set; }

    public CountryForm ConvertToViewModelCountryForm()
    {
        return new CountryForm()
        {
            Key = this.Key,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description,
            CreatedAt = this.CreatedAt,
            CreatedBy = this.CreatedBy,
            UpdatedAt = this.UpdatedAt,
            UpdatedBy = this.UpdatedBy,
            DeletedAt = this.DeletedAt,
            DeletedBy = this.DeletedBy
        };
    }

    public CountryListItem ConvertToViewModelCountryListItem()
    {
        return new CountryListItem()
        {
            Key = this.Key,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description,
            CreatedAt = this.CreatedAt,
            CreatedBy = this.CreatedBy
        };
    }
}
