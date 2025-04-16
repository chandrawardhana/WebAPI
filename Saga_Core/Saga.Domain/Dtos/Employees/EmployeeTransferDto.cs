namespace Saga.Domain.Dtos.Employees;

public class EmployeeTransferDto
{
    public Guid? Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public TransferCategory TransferCategory { get; set; }
    public DateOnly? EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public Guid? NewCompanyKey { get; set; } = Guid.Empty;
    public Guid? NewOrganizationKey { get; set; } = Guid.Empty;
    public Guid? NewPositionKey { get; set; } = Guid.Empty;
    public Guid? NewTitleKey { get; set; } = Guid.Empty;
    public Guid? NewBranchKey { get; set; } = Guid.Empty;
    public TransferStatus? TransferStatus { get; set; } = Enums.TransferStatus.Draft;

    public EmployeeTransfer ConvertToEntity()
    {
        return new EmployeeTransfer
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            TransferCategory = this.TransferCategory,
            EffectiveDate = this.EffectiveDate,
            NewCompanyKey = this.NewCompanyKey ?? Guid.Empty,
            NewOrganizationKey = this.NewOrganizationKey ?? Guid.Empty,
            NewPositionKey = this.NewPositionKey ?? Guid.Empty,
            NewTitleKey = this.NewTitleKey ?? Guid.Empty,
            NewBranchKey = this.NewBranchKey ?? Guid.Empty,
            TransferStatus = this.TransferStatus ?? Enums.TransferStatus.Draft,
        };
    }
}

public class CancelEmployeeTransferDto
{
    public List<Guid> EmployeeTransferKeys { get; set; } = new List<Guid>();
    public String? CancelledReason { get; set; } = String.Empty;
}
