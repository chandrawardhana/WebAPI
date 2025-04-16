
using Saga.Domain.ViewModels.Systems;

namespace Saga.Domain.Entities.Systems;

[Table("tbmnavigationaccessdetail", Schema = "System")]
public class NavigationAccessDetail : AuditTrail
{
    public Guid NavigationAccessKey { get; set; }
    public int Category { get; set; }
    public string Title { get; set; } = null!;
    public string AbsolutePath { get; set; } = null!;
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanExport { get; set; }

    public NavigationAccessDetailViewModel ToViewModel()
        => new ()
        {
            Category = Category,
            Title = Title,
            AbsolutePath = AbsolutePath,
            CanRead = CanRead,
            CanWrite = CanWrite,
            CanUpdate = CanUpdate,
            CanDelete = CanDelete,
            CanExport = CanExport
        };
}
