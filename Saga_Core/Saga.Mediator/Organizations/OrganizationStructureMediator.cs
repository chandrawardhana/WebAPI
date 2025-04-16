using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.ViewModels.Organizations;
using Saga.Mediator.Services;
using Saga.Mediator.Systems.AssetMediator;
using Saga.Persistence.Context;

namespace Saga.Mediator.Organizations.OrganizationStructureMediator;

#region "Get Organization Structure"
#region "Query"
    public sealed record GetOrganizationStructureQuery(Guid OrganizationKey) : IRequest<OrgChartData>;
#endregion
#region "Handler"
    public sealed class GetOrganizationStructureQueryHandler : IRequestHandler<GetOrganizationStructureQuery, OrgChartData>
    {
        private readonly IDataContext _context;
        private readonly IOrganizationRepository _repository;
        private readonly IMediator _mediator;

        public GetOrganizationStructureQueryHandler(IDataContext context, IOrganizationRepository repository, IMediator mediator)
        {
            _context = context;
            _repository = repository;
            _mediator = mediator;
        }

        public async Task<OrgChartData> Handle(GetOrganizationStructureQuery request, CancellationToken cancellationToken)
        {
            var isCompany = await IsCompanyKey(request.OrganizationKey);
            var orgData = await _repository.GetOrganizationWithChild(request.OrganizationKey, isCompany);

            if (isCompany)
            {
                return await BuildCompanyHierarchy(orgData);
            }

            return await BuildOrganizationHierarchy(orgData);
        }

        private async Task<OrgChartData> BuildCompanyHierarchy(OrganizationData data)
        {
            var company = data.Company;

            // Create company node as root
            var rootNode = new OrgChartData
            {
                id = company.Key.ToString(),
                name = company.Name,
                title = "Company",
                className = "company-node",
                extra = new OrgChartExtraData
                {
                    position = "Company",
                    organization = company.Name,
                    photoUrl = String.Empty,
                    code = company.Code
                }
            };

            // Build organization hierarchy for each root organization
            var children = new List<OrgChartData>();
            foreach (var org in data.RootOrganizations)
            {
                var childOrgData = await _repository.GetOrganizationWithChild(org.Key, false);
                var orgData = await BuildOrganizationHierarchy(childOrgData);
                if (orgData != null)
                    children.Add(orgData);
            }

            rootNode.children = children;
            rootNode.relationship = children.Any() ? "001" : "000";

            return rootNode;
        }

        private async Task<OrgChartData> BuildOrganizationHierarchy(OrganizationData data)
        {
            if (data.Organization == null)
                return null;

            // Create organization node
            var orgNode = new OrgChartData
            {
                id = data.Organization.Key.ToString(),
                name = data.Organization.Name,
                title = "Department",
                className = "department-node",
                extra = new OrgChartExtraData
                {
                    position = "Department",
                    organization = data.Organization.Parent,
                    photoUrl = String.Empty,
                    code = data.Organization.Code
                }
            };

            var children = new List<OrgChartData>();

            // Build employee hierarchy
            if (data.Employees?.Any() == true)
            {
                var employeeHierarchy = await BuildEmployeeHierarchy(data.Employees);
                if (employeeHierarchy?.Any() == true)
                {
                    children.AddRange(employeeHierarchy);
                }
            }

            // Build child organizations hierarchy
            foreach (var childOrg in data.ChildOrganizations ?? Enumerable.Empty<Organization>())
            {
                var childData = await _repository.GetOrganizationWithChild(childOrg.Key, false);
                var childNode = await BuildOrganizationHierarchy(childData);
                if (childNode != null)
                {
                    children.Add(childNode);
                }
            }

            orgNode.children = children;
            orgNode.relationship = children.Any() ? "001" : "000";

            return orgNode;
        }

        private async Task<List<OrgChartData>> BuildEmployeeHierarchy(List<EmployeeData> employees)
        {
            if (!employees.Any())
                return new List<OrgChartData>();

            // Find root employees (those without supervisors or supervisors outside current org)
            var rootEmployees = employees
                .Where(e => e.DirectSupervisorKey == null ||
                           !employees.Any(s => s.Key == e.DirectSupervisorKey))
                .ToList();

            var employeeNodes = new List<OrgChartData>();

            foreach (var rootEmployee in rootEmployees)
            {
                var empNode = await CreateEmployeeNode(rootEmployee, employees);
                if (empNode != null)
                {
                    employeeNodes.Add(empNode);
                }
            }

            return employeeNodes;
        }

        private async Task<OrgChartData> CreateEmployeeNode(EmployeeData employee, List<EmployeeData> allEmployees)
        {
            var photoUrl = employee.AssetKey != Guid.Empty
                ? await _mediator.Send(new GetFileUrlQuery(employee.AssetKey))
                : "";

            var employeeNode = new OrgChartData
            {
                id = employee.Key.ToString(),
                name = $"{employee.FirstName} {employee.LastName}",
                title = employee.Position?.Name ?? "Employee",
                className = "employee-node",
                extra = new OrgChartExtraData
                {
                    position = employee.Position?.Name ?? "No Position",
                    organization = employee.Organization?.Name ?? "",
                    photoUrl = photoUrl,
                    code = employee.Code
                }
            };

            // Find subordinates
            var subordinates = allEmployees
                .Where(e => e.DirectSupervisorKey == employee.Key)
                .ToList();

            if (subordinates.Any())
            {
                employeeNode.children = new List<OrgChartData>();
                foreach (var subordinate in subordinates)
                {
                    var subordinateNode = await CreateEmployeeNode(subordinate, allEmployees);
                    if (subordinateNode != null)
                    {
                        employeeNode.children.Add(subordinateNode);
                    }
                }
            }

            employeeNode.relationship = employeeNode.children?.Any() == true ? "001" : "000";
            return employeeNode;
        }

        private async Task<bool> IsCompanyKey(Guid key)
        {
            // Check if the key exists in Companies table
            return await _context.Companies
                .AnyAsync(c => c.Key == key && c.DeletedAt == null);
        }
    }
#endregion
#endregion

#region "Get Organization Structure using Go Js"
#region "Query"
    public sealed record GetGoJsOrganizationStructureQuery(Guid OrganizationKey) : IRequest<GoJsOrgChartModel>;
#endregion
#region "Handler"
    public sealed class GetGoJsOrganizationStructureQueryHandler : IRequestHandler<GetGoJsOrganizationStructureQuery, GoJsOrgChartModel>
    {
        private readonly IDataContext _context;
        private readonly IOrganizationRepository _repository;
        private readonly IMediator _mediator;

        public GetGoJsOrganizationStructureQueryHandler(IDataContext context, IOrganizationRepository repository, IMediator mediator)
        {
            _context = context;
            _repository = repository;
            _mediator = mediator;
        }

        public async Task<GoJsOrgChartModel> Handle(GetGoJsOrganizationStructureQuery request, CancellationToken cancellationToken)
        {
            var isCompany = await IsCompanyKey(request.OrganizationKey);

            var (organizations, employees) = await _repository.GetOrganizationStructure(request.OrganizationKey, isCompany);

            var nodes = new List<GoJsOrgChartNode>();

            // Build GoJS nodes for organizations
            foreach (var org in organizations)
            {
                var node = new GoJsOrgChartNode
                {
                    Key = org.Key,
                    Name = org.Name,
                    Title = org.Title,
                    Parent = org.ParentKey?.ToString() ?? (isCompany ? null : request.OrganizationKey.ToString()),
                    Comments = $"Code: {org.Code}"
                };

                if (org.LogoKey.HasValue)
                {
                    node.ImageInfo = new ImageInfo
                    {
                        Path = await _mediator.Send(new GetFileUrlQuery(org.LogoKey.Value)),
                        Name = org.LogoName ?? String.Empty,
                        MimeType = org.LogoMimeType ?? String.Empty,
                        AssetKey = org.LogoKey
                    };
                }

                nodes.Add(node);
            }

            // Build GoJS nodes for employees
            foreach (var emp in employees)
            {
                var node = new GoJsOrgChartNode
                {
                    Key = emp.Key,
                    Name = $"{emp.FirstName} {emp.LastName}",
                    Title = emp.TitleName,
                    Parent = emp.OrgKey.ToString(),
                    Comments = $"Code: {emp.Code}",
                    Email = emp.Email,
                    PhoneNumber = emp.PhoneNumber,
                    Department = emp.OrgName ?? String.Empty
                };

                if (emp.ProfileImageKey.HasValue)
                {
                    node.ImageInfo = new ImageInfo
                    {
                        Path = await _mediator.Send(new GetFileUrlQuery(emp.ProfileImageKey.Value)),
                        Name = emp.ProfileImageName ?? String.Empty,
                        MimeType = emp.ProfileImageMimeType ?? String.Empty,
                        AssetKey = emp.ProfileImageKey
                    };
                }

                nodes.Add(node);
            }

            return new GoJsOrgChartModel
            {
                NodeDataArray = nodes.ToArray()
            };
        }

        private async Task<bool> IsCompanyKey(Guid Key)
        {
            return await _context.OrganizationHierarchies.AnyAsync(o => o.OrgKey == Key && o.IsCompany);
        }
    }
#endregion
#endregion
