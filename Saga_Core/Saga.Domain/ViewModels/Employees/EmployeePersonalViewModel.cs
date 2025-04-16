using Microsoft.AspNetCore.Mvc.Rendering;

namespace Saga.Domain.ViewModels.Employees;

public class EmployeePersonalList
{
    public IEnumerable<EmployeePersonal> EmployeePersonals { get; set; } = Enumerable.Empty<EmployeePersonal>();
}

public class EmployeePersonalForm
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public string? NationalityNumber { get; set; } = String.Empty;
    public DateTime? NationalityRegistered { get; set; } = DateTime.Now;
    public string? PlaceOfBirth { get; set; } = String.Empty;
    public DateTime? DateOfBirth { get; set; } = DateTime.Now;
    public Gender? Gender { get; set; } = null;
    public Guid? ReligionKey { get; set; } = Guid.Empty;
    public MaritalStatus? MaritalStatus { get; set; } = null;
    public string? Address { get; set; } = String.Empty;
    public Guid? CountryKey { get; set; } = Guid.Empty;
    public Guid? ProvinceKey { get; set; } = Guid.Empty;
    public Guid? CityKey { get; set; } = Guid.Empty;
    public string? PostalCode { get; set; } = String.Empty;
    public string? CurrentAddress { get; set; } = String.Empty;
    public Guid? CurrentCountryKey { get; set; } = Guid.Empty;
    public Guid? CurrentProvinceKey { get; set; } = Guid.Empty;
    public Guid? CurrentCityKey { get; set; } = Guid.Empty;
    public string? CurrentPostalCode { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; } = String.Empty;
    public string? Email { get; set; } = String.Empty;
    public string? SocialMedia { get; set; } = String.Empty;
    public Guid? EthnicKey { get; set; } = Guid.Empty;
    public int? Weight { get; set; } = 0;
    public int? Height { get; set; } = 0;
    public BloodType? Blood { get; set; } = null;
    public bool? IsColorBlindness { get; set; } = false;

    public int? NumOfChild { get; set; } = 0;
    public string? CitizenNumber { get; set; } = String.Empty;
    public DateOnly? CitizenRegistered { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public Guid? NationalityKey { get; set; }

    public Employee? Employee { get; set; }
    public Religion? Religion { get; set; }
    public List<SelectListItem> Religions { get; set; } = new List<SelectListItem>();
    public Country? Country { get; set; }
    public List<SelectListItem> Countries { get; set; } = new List<SelectListItem>();
    public Province? Province { get; set; }
    public List<SelectListItem> Provinces { get; set; } = new List<SelectListItem>();
    public City? City { get; set; }
    public List<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
    public Country? CurrentCountry { get; set; }
    public List<SelectListItem> CurrentCountries { get; set; } = new List<SelectListItem>();
    public Province? CurrentProvince { get; set; }
    public List<SelectListItem> CurrentProvinces { get; set; } = new List<SelectListItem>();
    public City? CurrentCity { get; set; }
    public List<SelectListItem> CurrentCities { get; set; } = new List<SelectListItem>();
    public Ethnic? Ethnic { get; set; }
    public List<SelectListItem> Ethnics { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> Nationalities { get; set; } = new List<SelectListItem>();
    public Nationality? Nationality { get; set; }
}
