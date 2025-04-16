using Saga.Domain.ViewModels.Organizations;

namespace Saga.Domain.Entities.Organizations;

[Table("tbmbank", Schema = "Organization")]
public class Bank : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;

    public BankForm ConvertToViewModelBankForm()
    {
        return new BankForm()
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

    public BankListItem ConvertToViewModelBankListItem()
    {
        return new BankListItem
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
