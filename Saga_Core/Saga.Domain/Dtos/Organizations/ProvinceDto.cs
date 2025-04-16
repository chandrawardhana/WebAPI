namespace Saga.Domain.Dtos.Organizations;

public class ProvinceDto
{
    public Guid? Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid CountryKey { get; set; }

    public Province ConvertToEntity()
    {
        return new Province 
        {
            Key = this.Key ?? Guid.Empty,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description,
            CountryKey = this.CountryKey
        };
    }
}
