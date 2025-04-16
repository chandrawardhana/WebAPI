namespace Saga.Domain.Dtos.Organizations;

public class GradeDto
{
    public Guid? Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid CompanyKey { get; set; }
}
