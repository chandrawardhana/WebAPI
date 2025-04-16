namespace Saga.Domain.Dtos.Organizations;

public class CountryDto
{
    public Guid? Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    public Country ConvertToEntity()
    {
        return new Country
        {
            Key = this.Key ?? Guid.Empty,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description
        };
    }
}
