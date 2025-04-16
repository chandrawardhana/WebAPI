
namespace Saga.Domain.Entities.Systems;

[Table("tbmorganizationaccess", Schema = "System")]
public class OrganizationAccess : AuditTrail
{
    [Required]
    [StringLength(100)]
    public string AccessName { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    [Column(TypeName = "text")]
    public Guid[] AccessDetail { get; set; } = [];

}
