namespace Saga.Domain.Dtos.Organizations;

public class CompanyPolicyDto
{
    public Guid? Key { get; set; }
    public Guid CompanyKey { get; set; }
    public Guid OrganizationKey { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiredDate { get; set; }
    public string? Policy { get; set; }
}

public class CompanyPolicyReportDto
{
    public Guid? OrganizationKey { get; set; } = Guid.Empty;
    public DateTime? EffectiveDate { get; set; } = DateTime.Now;
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Pdf;
}
