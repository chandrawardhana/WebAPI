using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Employees;

namespace Saga.Domain.ViewModels.Employees;

public class EmployeeTransferList
{
    public IEnumerable<EmployeeTransferItemList> EmployeeTransfers { get; set; } = Enumerable.Empty<EmployeeTransferItemList>();
}

public class EmployeeTransferItemList
{
    public Guid EmployeeTransferKey { get; set; }
    public Guid EmployeeKey { get; set; }
    public string? EmployeeName { get; set; } = String.Empty;
    public string? EmployeeCode { get; set; } = String.Empty;
    public string? TransferCategory { get; set; } = String.Empty;
    public string? TransferStatus { get; set; } = String.Empty;
    public DateOnly? EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}

public class EmployeeTransferItemPaginationList
{
    public Guid EmployeeTransferKey { get; set; }
    public Guid EmployeeKey { get; set; }
    public string? EmployeeName { get; set; } = String.Empty;
    public string? EmployeeCode { get; set; } = String.Empty;
    public string? TransferCategory { get; set; } = String.Empty;
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public DateOnly? EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? TransferStatus { get; set; } = String.Empty;
    // For multiple selection checkbox
    public bool IsSelected { get; set; } = false;
    // Cancellation reason
    public string? CancelledReason { get; set; } = String.Empty;

    // Additional properties to support multi-selection functionality
    public bool CanBeCancelled {  get; set; }
    public bool IsCheckboxVisible { get; set; }

    // Helper method to determine if the row should be enabled/disabled
    public bool IsRowEnabled { get; set; }
}

// Add a wrapper class to handle the list and selection state
public class EmployeeTransferListViewModel
{
    public List<EmployeeTransferItemPaginationList> Transfers { get; set; } = new();

    public string? CancelledReason { get; set; } = String.Empty;
    public List<Guid> EmployeeTransferKeys { get; set; } = new List<Guid>();

    // Track number of selected items
    public int SelectedCount => Transfers.Count(x => x.IsSelected);

    // Check if any items are selected
    public bool HasSelectedItems => SelectedCount > 0;

    // Helper method to get selected transfer keys
    public IEnumerable<Guid> GetSelectedTransferKeys()
    {
        return Transfers.Where(x => x.IsSelected).Select(x => x.EmployeeTransferKey);
    }

    // Helper method to clear all selections
    public void ClearSelections()
    {
        foreach (var transfer in Transfers)
        {
            transfer.IsSelected = false;
        }
    }

    // Filter options
    public TransferStatus? TransferStatusFilter { get; set; }
    public TransferCategory? TransferCategoryFilter { get; set; }
    public DateTime? EffectiveDateFilter { get; set; }
    public string? SearchQuery { get; set; }
}

public class EmployeeTransferForm
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public TransferCategory? TransferCategory { get; set; } = null;
    public DateOnly? EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public Guid? OldCompanyKey { get; set; } = Guid.Empty;
    public Guid? OldOrganizationKey { get; set; } = Guid.Empty;
    public Guid? OldPositionKey { get; set; } = Guid.Empty;
    public Guid? OldTitleKey { get; set; } = Guid.Empty;
    public Guid? OldBranchKey { get; set; } = Guid.Empty;
    public Guid? NewCompanyKey { get; set; } = Guid.Empty;
    public Guid? NewOrganizationKey { get; set; } = Guid.Empty;
    public Guid? NewPositionKey { get; set; } = Guid.Empty;
    public Guid? NewTitleKey { get; set; } = Guid.Empty;
    public Guid? NewBranchKey { get; set; } = Guid.Empty;
    public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> NewCompanies { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> NewOrganizations { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> NewPositions { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> NewTitles { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> NewBranches { get; set; } = new List<SelectListItem>();
    public TransferStatus? TransferStatus { get; set; }
    public Employee? Employee { get; set; }
    public Company? OldCompany { get; set; }
    public Organization? OldOrganization { get; set; }
    public Position? OldPosition { get; set; }
    public Title? OldTitle { get; set; }
    public Branch? OldBranch { get; set; }
    public Company? NewCompany { get; set; }
    public Organization? NewOrganization { get; set; }
    public Position? NewPosition { get; set; }
    public Title? NewTitle { get; set; }
    public Branch? NewBranch { get; set; }

    public EmployeeTransferDto ConvertToEmployeeTransferDto()
    {
        return new EmployeeTransferDto
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            TransferCategory = this.TransferCategory ?? Domain.Enums.TransferCategory.Mutation,
            EffectiveDate = this.EffectiveDate,
            NewCompanyKey = this.NewCompanyKey,
            NewOrganizationKey = this.NewOrganizationKey,
            NewPositionKey = this.NewPositionKey,
            NewTitleKey = this.NewTitleKey,
            NewBranchKey = this.NewBranchKey,
            TransferStatus = this.TransferStatus ?? Domain.Enums.TransferStatus.Draft,
        };
    }
}
