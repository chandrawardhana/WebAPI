

namespace Saga.Domain.Entities.Systems;

[Table("tbmnavigationaccess", Schema = "System")]
public class NavigationAccess : AuditTrail
{
    [Required]
    [StringLength(100)]
    public string AccessName { get; set; } = null!;
    public string Description { get; set; } = string.Empty;

    [NotMapped]
    public ICollection<NavigationAccessDetail> AccessDetails { get; set; } = [];
}
