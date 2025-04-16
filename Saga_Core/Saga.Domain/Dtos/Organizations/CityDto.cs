namespace Saga.Domain.Dtos.Organizations;

public class CityDto
{
    public Guid? Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid CountryKey { get; set; }
    public Guid ProvinceKey { get; set; }

    public City ConvertToEntity()
    {
        return new City
        {
            Key = this.Key ?? Guid.Empty,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description ?? String.Empty,
            CountryKey = this.CountryKey,
            ProvinceKey = this.ProvinceKey
        };
    }
}
