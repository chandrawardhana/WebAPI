namespace Saga.Domain.Entities.Organizations;

[Table("tbmbranch", Schema = "Organization")]
public class Branch : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
    [Required]
    public Guid CompanyKey { get; set; } 
    [Required]
    public Guid CountryKey { get; set; }
    [Required]
    public Guid ProvinceKey { get; set; }
    [Required]
    public Guid CityKey { get; set; }
    [Required]
    [StringLength(200)]
    public string Address { get; set; } = null!;
    [Required]
    [StringLength(5)]
    public string PostalCode { get; set; } = null!;
    [Required]
    [StringLength(50)]
    public string Email { get; set; } = null!;
    [StringLength(20)]
    public string? PhoneNumber { get; set; } = string.Empty;
    [StringLength(100)]
    public string? Website { get; set; } = string.Empty;
    [Column("LogoKey")]
    public Guid? AssetKey { get; set; }
    //Branch Tax
    [StringLength(16)]
    public string TaxId { get; set; } = null!;
    [StringLength(100)]
    public string TaxName { get; set; } = null!;
    [StringLength(200)]
    public string TaxAddress { get; set; } = null!;
    //Branch Bank Account
    [Required]
    public Guid BankKey { get; set; }
    [Required]
    [StringLength(50)]
    public string BankAccountNumber { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string BankAccountName { get; set; } = null!;
    [Required]
    [StringLength(200)]
    public string BankAddress { get; set; } = null!;
    [Column(TypeName = "text")]
    public string? BranchProfile { get; set; } = string.Empty;

    [NotMapped]
    public Company? Company { get; set; }
    [NotMapped]
    public Country? Country { get; set; }
    [NotMapped]
    public Province? Province { get; set; }
    [NotMapped]
    public City? City { get; set; }
    [NotMapped]
    public Asset? Asset { get; set; }
    [NotMapped]
    public Bank? Bank { get; set; }
}
