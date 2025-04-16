namespace Saga.Domain.Dtos.Employees;

public class EmployeePersonalDto
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
    public DateOnly CitizenRegistered { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public Guid NationalityKey { get; set; }

    public EmployeePersonal ConvertToEntity()
    {
        return new EmployeePersonal
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            NationalityNumber = this.NationalityNumber ?? String.Empty,
            NationalityRegistered = this.NationalityRegistered ?? DateTime.Now,
            PlaceOfBirth = this.PlaceOfBirth ?? String.Empty,
            DateOfBirth = this.DateOfBirth ?? DateTime.Now,
            Gender = this.Gender ?? Enums.Gender.Male,
            ReligionKey = this.ReligionKey ?? Guid.Empty,
            MaritalStatus = this.MaritalStatus ?? Enums.MaritalStatus.Single,
            Address = this.Address ?? String.Empty,
            CountryKey = this.CountryKey ?? Guid.Empty,
            ProvinceKey = this.ProvinceKey ?? Guid.Empty,
            CityKey = this.CityKey ?? Guid.Empty,
            PostalCode = this.PostalCode ?? String.Empty,
            CurrentAddress = this.CurrentAddress ?? String.Empty,
            CurrentCountryKey = this.CurrentCountryKey ?? Guid.Empty,
            CurrentProvinceKey = this.CurrentProvinceKey ?? Guid.Empty,
            CurrentCityKey = this.CurrentCityKey ?? Guid.Empty,
            CurrentPostalCode = this.CurrentPostalCode ?? String.Empty,
            PhoneNumber = this.PhoneNumber ?? String.Empty,
            Email = this.Email ?? String.Empty,
            SocialMedia = this.SocialMedia ?? String.Empty,
            EthnicKey = this.EthnicKey ?? Guid.Empty,
            Weight = this.Weight ?? 0,
            Height = this.Height ?? 0,
            Blood = this.Blood ?? Enums.BloodType.O_Positive,
            IsColorBlindness = this.IsColorBlindness ?? false,
            NumOfChild = this.NumOfChild ?? 0,
            CitizenNumber = this.CitizenNumber ?? String.Empty,
            CitizenRegistered = this.CitizenRegistered,
            NationalityKey = this.NationalityKey
        };
    }
}
