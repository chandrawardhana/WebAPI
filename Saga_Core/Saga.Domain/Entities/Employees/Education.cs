using Saga.Domain.ViewModels.Employees;

namespace Saga.Domain.Entities.Employees;

[Table("tbmeducation", Schema = "Employee")]
public class Education : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;

    public EducationForm ConvertToViewModelEducationForm()
    {
        return new EducationForm
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

    public EducationListItem ConvertToViewModelEducationListItem()
    {
        return new EducationListItem
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
