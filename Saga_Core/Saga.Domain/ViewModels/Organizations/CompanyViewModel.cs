using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Saga.Domain.ViewModels.Organizations;

public class CompanyList
{
    public IEnumerable<Company> Companies { get; set; }
}

public class CompanyForm
{
    public Guid Key { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public CompanyLevel? CompanyLevel { get; set; }
    public Guid? CountryKey { get; set; }
    public Country? Country { get; set; }
    public List<SelectListItem> Countries { get; set; } = new List<SelectListItem>();
    public Guid? ProvinceKey { get; set; }
    public Province? Province { get; set; }
    public List<SelectListItem> Provinces { get; set; } = new List<SelectListItem>();
    public Guid? CityKey { get; set; }
    public City? City { get; set; }
    public List<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public Guid? AssetKey { get; set; }
    public Asset? Asset { get; set; }
    public IFormFile? Logo { get; set; }
    public string? TaxId { get; set; }
    public string? TaxName { get; set; }
    public string? TaxAddress { get; set; }
    public Guid? BankKey { get; set; }
    public Bank? Bank { get; set; }
    public List<SelectListItem> Banks { get; set; } = new List<SelectListItem>();
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankAddress { get; set; }
    public string? CompanyProfile { get; set; }
}
