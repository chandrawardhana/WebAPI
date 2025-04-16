using Saga.Domain.Dtos.Employees;

namespace Saga.Domain.ViewModels.Employees;

public class NationalityForm
{
    public Guid Key { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public NationalityDto ConvertToNationalityDto()
    {
        return new NationalityDto
        {
            Key = Key,
            Code = Code,
            Name = Name,
            Description = Description
        };
    }
}
