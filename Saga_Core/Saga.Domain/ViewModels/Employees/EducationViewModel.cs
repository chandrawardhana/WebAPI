using Saga.Domain.Dtos.Employees;

namespace Saga.Domain.ViewModels.Employees;

public class EducationListItem
{
    public Guid Key { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class EducationList
{
    public IEnumerable<EducationListItem> Educations { get; set; } = new List<EducationListItem>();
}

public class EducationForm
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

    //Convert current instance to EducationDto
    public EducationDto ConvertToEducationDto()
    {
        return new EducationDto
        {
            Key = this.Key,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description
        };
    }
}
