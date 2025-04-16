namespace Saga.Domain.Entities.Employees;

[Table("tbmemployeepersonal", Schema = "Employee")]
public class EmployeePersonal : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [Required]
    [StringLength(16)]
    public string NationalityNumber { get; set; } = null!;
    [Required]
    public DateTime? NationalityRegistered { get; set; } = DateTime.Now;
    [Required]
    [StringLength(50)]
    public string PlaceOfBirth { get; set; } = null!;
    [Required]
    public DateTime DateOfBirth { get; set; }
    [Required]
    public Gender Gender { get; set; }
    [Required]
    public Guid ReligionKey { get; set; }
    [Required]
    public MaritalStatus MaritalStatus { get; set; }
    [Required]
    [StringLength(200)]
    public string Address { get; set; } = null!;
    [Required]
    public Guid CountryKey { get; set; }
    [Required]
    public Guid ProvinceKey { get; set; }
    [Required]
    public Guid CityKey { get; set; }
    [Required]
    [StringLength(5)]
    public string PostalCode { get; set; } = null!;

    [StringLength(200)]
    public string CurrentAddress { get; set; } = String.Empty;
    public Guid? CurrentCountryKey { get; set; }
    public Guid? CurrentProvinceKey { get; set; }
    public Guid? CurrentCityKey { get; set; }
    [StringLength(5)]
    public string? CurrentPostalCode { get; set; } = String.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = null!;
    [MaxLength(100)]
    public string? SocialMedia { get; set; } = String.Empty;

    [Required]
    public Guid EthnicKey { get; set; }
    [Required]
    public int Weight { get; set; }
    [Required]
    public int Height { get; set; }
    [Required]
    public BloodType Blood { get; set; }
    [Required]
    public bool IsColorBlindness { get; set; } = false;
    public int? NumOfChild { get; set; } = 0;
    [Required]
    [StringLength(16)]
    public string CitizenNumber { get; set; } = null!;
    [Required]
    public DateOnly CitizenRegistered { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    [Required]
    public Guid NationalityKey { get; set; }


    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public Religion? Religion { get; set; }
    [NotMapped]
    public Country? Country { get; set; }
    [NotMapped]
    public Province? Province { get; set; }
    [NotMapped]
    public City? City { get; set; }
    [NotMapped]
    public Country? CurrentCountry { get; set; }
    [NotMapped]
    public Province? CurrentProvince { get; set; }
    [NotMapped]
    public City? CurrentCity { get; set; }
    [NotMapped]
    public Ethnic? Ethnic { get; set; }
    [NotMapped]
    public Nationality? Nationality { get; set; }
}
