namespace Saga.Domain.Dtos.Employees;

public class NationalityDto
{
    public Guid? Key { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    public Nationality ConvertToEntity()
    {
        return new Nationality
        {
            Key = Key ?? Guid.Empty,
            Code = Code ?? string.Empty,
            Name = Name ?? string.Empty,
            Description = Description
        };
    }
}
