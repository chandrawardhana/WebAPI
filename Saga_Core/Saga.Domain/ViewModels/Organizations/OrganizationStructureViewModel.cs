using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Saga.Domain.ViewModels.Employees;
using System.Text.Json.Serialization;

namespace Saga.Domain.ViewModels.Organizations;

public class OrganizationData
{
    public CompanyHierarchyData Company { get; set; }
    public List<Organization> RootOrganizations { get; set; }
    public OrganizationBasicData Organization { get; set; }
    public List<Organization> ChildOrganizations { get; set; }
    public List<EmployeeData> Employees { get; set; }
}

public class OrganizationStructureFilter
{
    public Guid? ParentKey { get; set; }
    public IEnumerable<SelectListItem> Organizations { get; set; } = new List<SelectListItem>();
}


public class CompanyHierarchyData
{
    public Guid Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public CompanyLevel CompanyLevel { get; set; }
}

public class OrganizationBasicData
{
    public Guid Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Parent { get; set; }
    public string CompanyName { get; set; }
}

public class EmployeeData
{
    public Guid Key { get; set; }
    public string Code { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public PositionForm Position { get; set; }
    public TitleForm Title { get; set; }
    public OrganizationBasicData Organization { get; set; }
    public DirectSupervisorList Supervisor { get; set; }

    // Additional property for employee photo and Direct Supervisor Key
    public Guid AssetKey { get; set; } = Guid.Empty;
    public Guid? DirectSupervisorKey { get; set; } = Guid.Empty;
}


public class OrgChartData
{
    public string id { get; set; }
    public string name { get; set; }
    public string title { get; set; }
    public string className { get; set; }
    public string relationship { get; set; }
    public List<OrgChartData> children { get; set; }
    public OrgChartExtraData extra { get; set; }
}

public class OrgChartExtraData
{
    public string position { get; set; }
    public string organization { get; set; }
    public string photoUrl { get; set; }

    // Additional properties if needed
    public string? code { get; set; }
    public CompanyLevel? CompanyLevel { get; set; }
}

public class GoJsOrganizationData
{
    public Guid Key { get; set; }
    public string? Name { get; set; } = String.Empty;
    public string? Title { get; set; } = String.Empty;
    public string? Code { get; set; } = String.Empty;
    public Guid? ParentKey { get; set; } = Guid.Empty;
    public Guid CompanyKey { get; set; }
    public Guid? LogoKey { get; set; } = Guid.Empty;
    public string? LogoName { get; set; } = String.Empty;
    public string? LogoMimeType { get; set; } = String.Empty;
}

public class GoJsEmployeeData
{
    public Guid Key { get; set; }
    public string? FirstName { get; set; } = String.Empty;
    public string? LastName { get; set; } = String.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; } = String.Empty;
    public string? Email { get; set; } = String.Empty;
    public string? PositionName { get; set; } = String.Empty;
    public string? TitleName { get; set; } = String.Empty;
    public Guid OrgKey { get; set; }
    public string? OrgName { get; set; } = String.Empty;
    public Guid? ProfileImageKey { get; set; } = Guid.Empty;
    public string? ProfileImageName { get; set; } = String.Empty;
    public string? ProfileImageMimeType { get; set; } = String.Empty;
}

// Modified to match GoJS JSON structure
public class GoJsOrgChartNode
{
    [JsonPropertyName("key")]
    public Guid? Key { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; } = String.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; } = String.Empty;

    [JsonPropertyName("parent")]
    public string? Parent { get; set; } = String.Empty;

    [JsonPropertyName("comments")]
    public string? Comments { get; set; } = String.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; } = String.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("pic")]
    public ImageInfo? ImageInfo { get; set; }

    [JsonProperty("phone")]
    public string? PhoneNumber { get; set; } = String.Empty;

    [JsonPropertyName("dept")]
    public string Department { get; set; } = String.Empty;

    [JsonPropertyName("itemHeight")]
    public int? ItemHeight { get; set; } = 0;

    [JsonPropertyName("loc")]
    public string? Location { get; set; } = String.Empty;
}

public class ImageInfo
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; }

    [JsonPropertyName("assetKey")]
    public Guid? AssetKey { get; set; }
}

public class GoJsOrgChartModel
{
    [JsonPropertyName("class")]
    public string Class { get; set; } = "go.TreeModel";

    [JsonPropertyName("linkFromPortIdProperty")]
    public string LinkFromPortIdProperty { get; set; } = "fromPort";

    [JsonPropertyName("linkToPortIdProperty")]
    public string LinkToPortIdProperty { get; set; } = "toPort";

    [JsonPropertyName("nodeDataArray")]
    public GoJsOrgChartNode[] NodeDataArray { get; set; } = Array.Empty<GoJsOrgChartNode>();

    [JsonPropertyName("linkDataArray")]
    public object[] LinkDataArray { get; set; } = Array.Empty<object>();
}
