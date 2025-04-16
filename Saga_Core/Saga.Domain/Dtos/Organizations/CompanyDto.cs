using Microsoft.AspNetCore.Http;

namespace Saga.Domain.Dtos.Organizations;

public class CompanyDto
{
    public Guid? Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public CompanyLevel CompanyLevel { get; set; }
    public Guid CountryKey { get; set; }
    public Guid ProvinceKey { get; set; }
    public Guid CityKey { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public Guid? LogoKey { get; set; }
    public IFormFile? Logo { get; set; }
    public string TaxId { get; set; }
    public string TaxName { get; set; }
    public string TaxAddress { get; set; }
    public Guid BankKey { get; set; }
    public string BankAccountNumber { get; set; }
    public string BankAccountName { get; set; }
    public string BankAddress { get; set; }
    public string? CompanyProfile { get; set; }
}
