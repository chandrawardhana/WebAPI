namespace Saga.Domain.Entities.Programs;

public class OrganizationHierarchy
{
    [Column("org_key")]
    public Guid OrgKey { get; set; }

    [Column("org_name")]
    public string? OrgName { get; set; } = string.Empty;

    [Column("org_code")]
    public string? OrgCode { get; set; } = string.Empty;

    [Column("node_type")]
    public string? NodeType { get; set; } = string.Empty;

    [Column("parent_key")]
    public Guid? ParentKey { get; set; } = Guid.Empty;

    [Column("company_key")]
    public Guid CompanyKey { get; set; }

    [Column("logo_key")]
    public Guid? LogoKey { get; set; } = Guid.Empty;

    [Column("logo_name")]
    public string? LogoName { get; set; } = string.Empty;

    [Column("logo_mime_type")]
    public string? LogoMimeType { get; set; } = string.Empty;

    [Column("is_company")]
    public bool IsCompany { get; set; }
}
