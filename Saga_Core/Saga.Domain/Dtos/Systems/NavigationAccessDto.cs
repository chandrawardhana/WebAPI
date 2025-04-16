
using Saga.Domain.ViewModels.Systems;

namespace Saga.Domain.Dtos.Systems;

public class NavigationAccessDto
{
    public Guid Key { get; set; }
    [Required]
    [MaxLength(100)]
    public string AccessName { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public ICollection<NavigationAccessDetailDto> Details { get; set; } = [];

    public NavigationAccess ConvertToEntity()
    {
        var access = new NavigationAccess()
        {
            Key = Key,
            AccessName = AccessName,
            Description = Description,
            AccessDetails = Details.Select(x =>
            {
                var convert = x.ConvertToEntity();
                convert.NavigationAccessKey = Key;
                return convert;
            }).ToArray()
        };

        return access;
    }
}

public class NavigationAccessDetailDto
{
    public int Category { get; set; }
    public string Title { get; set; } = null!;
    public string AbsolutePath { get; set; } = null!;
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanExport { get; set; }

    public NavigationAccessDetailViewModel ConvertToViewModel()
        => new()
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

    public NavigationAccessDetail ConvertToEntity()
        => new()
        {
            Key = Guid.NewGuid(),
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
