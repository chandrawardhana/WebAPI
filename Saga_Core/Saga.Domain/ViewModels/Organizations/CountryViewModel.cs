using Saga.Domain.Dtos.Organizations;

namespace Saga.Domain.ViewModels.Organizations;

public class CountryListItem
{
    public Guid Key { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class CountryList
{
    public IEnumerable<CountryListItem> Countries { get; set; } = new List<CountryListItem>();
}

public class CountryForm
{
    public Guid Key { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    //Convert current instance to CountryDto
    public CountryDto ConvertToCountryDto()
    {
        return new CountryDto()
        {
            Key = this.Key,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description
        };
    }
}
