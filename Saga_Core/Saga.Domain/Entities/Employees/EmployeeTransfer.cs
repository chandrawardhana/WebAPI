using Saga.Domain.ViewModels.Employees;

namespace Saga.Domain.Entities.Employees;

[Table("tbtemployeetransfer", Schema = "Employee")]
public class EmployeeTransfer : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    public TransferCategory TransferCategory { get; set; }
    [Required]
    public DateOnly? EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public bool? IsProcessed { get; set; } = false;
    [Required]
    public TransferStatus TransferStatus { get; set; }
    [MaxLength(300)]
    public string? CancelledReason { get; set; } = String.Empty;

    //Old Employee Data
    [Required]
    public Guid OldCompanyKey { get; set; }
    [Required]
    public Guid OldOrganizationKey { get; set; }
    [Required]
    public Guid OldPositionKey { get; set; }
    [Required]
    public Guid OldTitleKey { get; set; }
    [Required]
    public Guid OldBranchKey { get; set; }

    //New Employee Data
    [Required]
    public Guid NewCompanyKey { get; set; }
    [Required]
    public Guid NewOrganizationKey { get; set; }
    [Required]
    public Guid NewPositionKey { get; set; }
    [Required]
    public Guid NewTitleKey { get; set; }   
    [Required]
    public Guid NewBranchKey { get; set; }

    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public Company? OldCompany { get; set; }
    [NotMapped]
    public Organization? OldOrganization { get; set; }
    [NotMapped]
    public Position? OldPosition { get; set; }
    [NotMapped]
    public Title? OldTitle { get; set; }
    [NotMapped]
    public Branch? OldBranch { get; set; }
    [NotMapped]
    public Company? NewCompany { get; set; }
    [NotMapped]
    public Organization? NewOrganization { get; set; }
    [NotMapped]
    public Position? NewPosition { get; set; }
    [NotMapped]
    public Title? NewTitle { get; set; }
    [NotMapped]
    public Branch? NewBranch { get; set; }

    public EmployeeTransferItemList ConvertToViewModelEmployeeTransferItemList()
    {
        return new EmployeeTransferItemList
        {
            EmployeeTransferKey = this.Key,
            EmployeeKey = this.EmployeeKey,
            EmployeeName = (this.Employee?.FirstName ?? String.Empty) + " " + (this.Employee?.LastName ?? String.Empty),
            EmployeeCode = this.Employee?.Code,
            TransferCategory = this.TransferCategory.ToString(),
            TransferStatus = this.TransferStatus.ToString(),
            EffectiveDate = this.EffectiveDate
        };
    }

    public EmployeeTransferForm ConvertToViewModelEmployeeTransferForm()
    {
        return new EmployeeTransferForm
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            TransferCategory = this.TransferCategory,
            EffectiveDate = this.EffectiveDate,
            OldCompanyKey = this.OldCompanyKey,
            OldOrganizationKey = this.OldOrganizationKey,
            OldPositionKey = this.OldPositionKey,
            OldTitleKey = this.OldTitleKey,
            OldBranchKey = this.OldBranchKey,
            NewCompanyKey = this.NewCompanyKey,
            NewOrganizationKey = this.NewOrganizationKey,
            NewPositionKey = this.NewPositionKey,
            NewTitleKey = this.NewTitleKey,
            NewBranchKey = this.NewBranchKey,
            TransferStatus = this.TransferStatus,
            Employee = this.Employee,
            OldCompany = this.OldCompany,
            OldOrganization = this.OldOrganization,
            OldPosition = this.OldPosition,
            OldTitle = this.OldTitle,
            OldBranch = this.OldBranch,
            NewCompany = this.NewCompany,
            NewOrganization = this.NewOrganization,
            NewPosition = this.NewPosition,
            NewTitle = this.NewTitle,
            NewBranch = this.NewBranch
        };
    }
}
