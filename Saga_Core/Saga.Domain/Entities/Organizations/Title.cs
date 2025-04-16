namespace Saga.Domain.Entities.Organizations;

[Table("tbmtitle", Schema = "Organization")]
public class Title : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;
    [Required]
    public Guid CompanyKey { get; set; }

    [NotMapped]
    public Company? Company { get; set; }
}
