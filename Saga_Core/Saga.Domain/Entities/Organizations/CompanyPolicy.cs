namespace Saga.Domain.Entities.Organizations;

[Table("tbmcompanypolicy", Schema = "Organization")]
public class CompanyPolicy : AuditTrail
{
    [Required]
    public Guid CompanyKey { get; set; }
    [Required]
    public Guid OrganizationKey { get; set; }
    [Required]
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiredDate { get; set; }
    [Column(TypeName = "text")]
    public string? Policy { get; set; } = string.Empty;

    [NotMapped]
    public Company? Company { get; set; }
    [NotMapped]
    public Organization? Organization { get; set; }
}
