
namespace Saga.Domain.Dtos.Systems;

public class OrganizationAccessDto
{
    public Guid Key { get; set; }
    [Required]
    [MaxLength(100)]
    public string AccessName { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public List<Guid> Details { get; set; } = [];

    public OrganizationAccess ConvertToEntity()
        => new ()
        {
            Key = Key,
            AccessName = AccessName,
            Description = Description,
            AccessDetail = Details.ToArray()
        };
}
