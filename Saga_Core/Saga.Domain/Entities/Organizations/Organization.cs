namespace Saga.Domain.Entities.Organizations;

[Table("tbmorganization", Schema = "Organization")]
public class Organization : AuditTrail
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    public Guid? ParentKey { get; set; }

    public int Level { get; set; }
    public Guid CompanyKey { get; set; }

    [NotMapped]
    public Organization? Parent { get; set; }
    [NotMapped]
    public Company? Company { get; set; }
}
