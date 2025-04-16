namespace Saga.Domain.ViewModels.Organizations;

public class CompanyPolicyList
{
    public IEnumerable<CompanyPolicy> CompanyPolicies { get; set; } = Enumerable.Empty<CompanyPolicy>();
}

public class CompanyPolicyReport
{
    public IEnumerable<CompanyPolicyItemReport> CompanyPolicies { get; set; }
    public Guid? OrganizationKey { get; set; } = Guid.Empty;
    public DateTime? EffectiveDate { get; set; } = DateTime.Now;
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Pdf;
    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}

public class CompanyPolicyItemReport
{
    public string? CompanyCode { get; set; } = String.Empty;
    public string? CompanyName { get; set; } = String.Empty;
    public string? CompanyAddress { get; set; } = String.Empty;
    public string? OrganizationName { get; set; } = String.Empty;
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiredDate { get; set; }
    public string? Policy { get; set; }
}


public class CompanyPolicyForm
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; }
    public Company? Company { get; set; }
    public Guid? OrganizationKey { get; set; }
    public Organization? Organization { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpiredDate { get; set; }
    public string? Policy { get; set; }
    public ICollection<Company> Companies { get; set; } = new List<Company>();
    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
