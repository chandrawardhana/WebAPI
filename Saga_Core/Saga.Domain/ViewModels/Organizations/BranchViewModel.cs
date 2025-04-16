using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Saga.Domain.ViewModels.Organizations;

public class BranchList
{
    public IEnumerable<Branch> Branches { get; set; } = Enumerable.Empty<Branch>();
}

public class BranchForm
{
    public Guid Key { get; set; }
    public string? Code { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Company? Company { get; set; }
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    public Guid? CountryKey { get; set; } = Guid.Empty;
    public Country? Country { get; set; }
    public List<SelectListItem> Countries { get; set; } = new List<SelectListItem>();
    public Guid? ProvinceKey { get; set; } = Guid.Empty;
    public Province? Province { get; set; }
    public List<SelectListItem> Provinces { get; set; } = new List<SelectListItem>();
    public Guid? CityKey { get; set; } = Guid.Empty;
    public City? City { get; set; }
    public List<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
    public string? Address { get; set; } = string.Empty;
    public string? PostalCode { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? Website { get; set; } = string.Empty;
    public Guid? AssetKey { get; set; } = Guid.Empty;
    public Asset? Asset { get; set; }
    public IFormFile? Logo { get; set; }
    public string? TaxId { get; set; } = string.Empty;
    public string? TaxName { get; set; } = string.Empty;
    public string? TaxAddress { get; set; } = string.Empty;
    public Guid? BankKey { get; set; } = Guid.Empty;
    public Bank? Bank { get; set; }
    public List<SelectListItem> Banks { get; set; } = new List<SelectListItem>();
    public string? BankAccountNumber { get; set; } = String.Empty;
    public string? BankAccountName { get; set; } = String.Empty;
    public string? BankAddress { get; set; } = String.Empty;
    public string? BranchProfile { get; set; } = String.Empty;
}
