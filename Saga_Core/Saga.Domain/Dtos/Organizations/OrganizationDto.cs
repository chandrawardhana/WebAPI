namespace Saga.Domain.Dtos.Organizations;

public class OrganizationDto
{
    public Guid? Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public Guid? ParentKey { get; set; }
}
