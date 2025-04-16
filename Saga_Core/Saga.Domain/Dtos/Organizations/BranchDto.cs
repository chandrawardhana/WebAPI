using Microsoft.AspNetCore.Http;

namespace Saga.Domain.Dtos.Organizations;

public class BranchDto
{
    public Guid? Key { get; set; }
    public string? Code { get; set; } = String.Empty;
    public string? Name { get; set; } = String.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid? CountryKey { get; set; } = Guid.Empty;
    public Guid? ProvinceKey { get; set; } = Guid.Empty;
    public Guid? CityKey { get; set; } = Guid.Empty;
    public string? Address { get; set; } = String.Empty;
    public string? PostalCode { get; set; } = String.Empty;
    public string? Email { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; } = String.Empty;
    public string? Website { get; set; } = String.Empty;
    public Guid? LogoKey { get; set; } = Guid.Empty;
    public IFormFile? Logo { get; set; }
    public string? TaxId { get; set; } = String.Empty;
    public string? TaxName { get; set; } = String.Empty;
    public string? TaxAddress { get; set; } = String.Empty;
    public Guid? BankKey { get; set; } = Guid.Empty;
    public string? BankAccountNumber { get; set; } = String.Empty;
    public string? BankAccountName { get; set; } = String.Empty;
    public string? BankAddress { get; set; } = String.Empty;
    public string? BranchProfile { get; set; } = String.Empty;
}
