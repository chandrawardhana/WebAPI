namespace Saga.Domain.Dtos.Employees;

public class EducationDto
{
    public Guid? Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    public Education ConvertToEntity()
    {
        return new Education
        {
            Key = this.Key ?? Guid.Empty,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description
        };
    }
}
