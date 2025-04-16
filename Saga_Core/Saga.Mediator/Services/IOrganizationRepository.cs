using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Organizations;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.ViewModels.Organizations;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Services;

public interface IOrganizationRepository
{
    Task<OrganizationList> GetAllOrganizations(Expression<Func<Organization, bool>>[] wheres);
    Task<PaginatedList<Organization>> GetAllOrganizationsWithPagination(Expression<Func<Organization, bool>>[] wheres, PaginationConfig pagination);
    Task<OrganizationForm> GetOrganization(Guid Key, Guid? ParentKey);
    Task<OrganizationData> GetOrganizationWithChild(Guid Key, bool isCompany);
    Task SaveOrganizationAsync(Organization organization, CancellationToken cancellationToken);
    Task<IEnumerable<CompanyPolicyItemReport>> GetCompanyPolicyReportQuery(CompanyPolicyReportDto report);
    Task<(IEnumerable<GoJsOrganizationData> Organizations, IEnumerable<GoJsEmployeeData> Employees)> GetOrganizationStructure(Guid Key, bool isCompany);
}

public class OrganizationRepository(IDataContext _context) : IOrganizationRepository
{
    public async Task<OrganizationList> GetAllOrganizations(Expression<Func<Organization, bool>>[] wheres)
    {
        var queries = _context.Organizations.AsQueryable().Where(b => b.DeletedAt == null);

        foreach (var where in wheres)
        {
            queries = queries.Where(where);
        }
        var organizations = await queries.ToListAsync();

        organizations.ForEach(x =>
        {
            var parentOrganization = _context.Organizations.FirstOrDefault(f => f.Key == x.ParentKey);
            if (parentOrganization != null)
            {
                x.Parent = new Organization
                {
                    Key = parentOrganization.Key,
                    Code = parentOrganization.Code,
                    Name = parentOrganization.Name,
                    ParentKey = parentOrganization.ParentKey
                };
            }
            else
            {
                // If the parent is not an organization, check if it's a company
                var parentCompany = _context.Companies.FirstOrDefault(c => c.Key == x.ParentKey);

                if (parentCompany != null)
                {
                    x.Parent = new Organization
                    {
                        Key = parentCompany.Key,
                        Code = parentCompany.Code,
                        Name = parentCompany.Name,
                        ParentKey = null // Companies do not have a ParentKey in this context
                    };
                }
            }

            x.Company = _context.Companies.FirstOrDefault(c => c.Key == x.CompanyKey);
        });

        var viewModel = new OrganizationList
        {
            Organizations = organizations.Select(org => new Organization
            {
                Key = org.Key,
                Code = org.Code,
                Name = org.Name,
                ParentKey = org.ParentKey,
                Parent = org.Parent,
                Level = org.Level,
                CompanyKey = org.CompanyKey,
                Company = org.Company
            })
        };

        return await Task.FromResult(viewModel);
    }

    public async Task<PaginatedList<Organization>> GetAllOrganizationsWithPagination(Expression<Func<Organization, bool>>[] wheres, PaginationConfig pagination)
    {
        var queries = _context.Organizations.AsQueryable().Where(b => b.DeletedAt == null);
        string search = pagination.Find;

        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(p => EF.Functions.ILike(p.Code, $"%{search}%") || EF.Functions.ILike(p.Name, $"%{search}%"));
        }

        // Applying additional filters from 'wheres'
        foreach (var filter in wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x));
        }
        var organizations = await queries.PaginatedListAsync(pagination.PageNumber, pagination.PageSize);

        organizations.Items.ForEach(x =>
        {
            var parentOrganization = _context.Organizations.FirstOrDefault(f => f.Key == x.ParentKey);
            if (parentOrganization != null)
            {
                x.Parent = new Organization
                {
                    Key = parentOrganization.Key,
                    Code = parentOrganization.Code,
                    Name = parentOrganization.Name,
                    ParentKey = parentOrganization.ParentKey
                };
            }
            else
            {
                // If the parent is not an organization, check if it's a company
                var parentCompany = _context.Companies.FirstOrDefault(c => c.Key == x.ParentKey);

                if (parentCompany != null)
                {
                    x.Parent = new Organization
                    {
                        Key = parentCompany.Key,
                        Code = parentCompany.Code,
                        Name = parentCompany.Name,
                        ParentKey = null // Companies do not have a ParentKey in this context
                    };
                }
            }

            x.Company = _context.Companies.FirstOrDefault(c => c.Key == x.CompanyKey);
        });

        return await Task.FromResult(organizations);
    }

    public async Task<OrganizationForm> GetOrganization(Guid Key, Guid? ParentKey)
    {
        List<Tuple<Guid, int, Guid>> parents = [];
        var companies = _context.Companies.Select(x => new Tuple<Guid, int, Guid>(x.Key, 0, x.Key)).ToList();
        parents.AddRange(companies);

        var organizations = _context.Organizations.Select(x => new Tuple<Guid, int, Guid>(x.Key, x.Level, x.CompanyKey)).ToList();
        parents.AddRange(organizations);

        var parent = parents.FirstOrDefault(x => x.Item1 == ParentKey);
        var parentKey = parent.Item1;
        var level = parent.Item2;
        var companyKey = parent.Item3;

        var organization = await(from org in _context.Organizations
                                     // Left join on the parent organization
                                 from parentOrg in _context.Organizations.Where(p => p.Key == parentKey && p.Level == level).DefaultIfEmpty()
                                     // Left join to find the parent company if the parent is an organization
                                 from parentCompany in _context.Companies.Where(c => c.Key == (parentOrg == null ? org.ParentKey : parentOrg.ParentKey)).DefaultIfEmpty()
                                 from company in _context.Companies.Where(c => c.Key == companyKey).DefaultIfEmpty()
                                 where org.Key == Key
                                 select new OrganizationForm
                                 {
                                     Key = org.Key,
                                     Code = org.Code,
                                     Name = org.Name,
                                     ParentKey = org.ParentKey,
                                     Parent = (parentOrg == null
                                                   ? new Organization
                                                   {
                                                       Key = parentCompany.Key,
                                                       Code = parentCompany.Code,
                                                       Name = parentCompany.Name,
                                                       ParentKey = null // Companies do not have a ParentKey in this context
                                                   } : parentOrg),
                                     Level = org.Level,
                                     CompanyKey = org.CompanyKey,
                                     Company = company
                                 }).FirstOrDefaultAsync();

        if (organization == null)
        {
            throw new InvalidOperationException("Organization not found or has been deleted.");
        }

        return await Task.FromResult(organization);
    }

    public async Task<OrganizationData> GetOrganizationWithChild(Guid key, bool isCompany)
    {
        if (isCompany)
        {
            var company = await GetCompanyData(key);
            if (company == null)
                throw new KeyNotFoundException($"Company not found with Key: {key}");

            var rootOrganizations = await GetRootOrganizations(company.Key);

            return new OrganizationData
            {
                Company = company,
                RootOrganizations = rootOrganizations
            };
        }

        return new OrganizationData
        {
            Organization = await GetOrganizationData(key),
            ChildOrganizations = await GetChildOrganizations(key),
            Employees = await GetOrganizationEmployees(key)
        };
    }

    private async Task<CompanyHierarchyData> GetCompanyData(Guid key)
    {
        return await _context.Companies
            .Where(c => c.Key == key && c.DeletedAt == null)
            .Select(c => new CompanyHierarchyData
            {
                Key = c.Key,
                Code = c.Code,
                Name = c.Name,
                CompanyLevel = c.CompanyLevel
            })
            .FirstOrDefaultAsync();
    }

    private async Task<List<Organization>> GetRootOrganizations(Guid companyKey)
    {
        return await _context.Organizations
            .Where(o => o.CompanyKey == companyKey && o.ParentKey == companyKey && o.DeletedAt == null)
            .ToListAsync();
    }

    private async Task<OrganizationBasicData> GetOrganizationData(Guid key)
    {
        return await (from org in _context.Organizations
                      join parent in _context.Organizations
                          on org.ParentKey equals parent.Key into parentJoin
                      from parent in parentJoin.DefaultIfEmpty()
                      join company in _context.Companies
                          on org.CompanyKey equals company.Key
                      where org.Key == key && org.DeletedAt == null
                      select new OrganizationBasicData
                      {
                          Key = org.Key,
                          Code = org.Code,
                          Name = org.Name,
                          Parent = parent != null ? parent.Name : company.Name,
                          CompanyName = company.Name
                      }).FirstOrDefaultAsync();
    }

    private async Task<List<Organization>> GetChildOrganizations(Guid parentKey)
    {
        return await _context.Organizations
            .Where(o => o.ParentKey == parentKey && o.DeletedAt == null)
            .ToListAsync();
    }

    private async Task<List<EmployeeData>> GetOrganizationEmployees(Guid orgKey)
    {
        return await (from emp in _context.Employees
                      join pos in _context.Positions on emp.PositionKey equals pos.Key
                      join ti in _context.Titles on emp.TitleKey equals ti.Key
                      join org in _context.Organizations on emp.OrganizationKey equals org.Key
                      where emp.OrganizationKey == orgKey && emp.DeletedAt == null
                      select new EmployeeData
                      {
                          Key = emp.Key,
                          Code = emp.Code,
                          FirstName = emp.FirstName,
                          LastName = emp.LastName,
                          DirectSupervisorKey = emp.DirectSupervisorKey,
                          AssetKey = emp.AssetKey ?? Guid.Empty,
                          Position = new PositionForm
                          {
                              Key = pos.Key,
                              Code = pos.Code,
                              Name = pos.Name
                          },
                          Title = new TitleForm
                          {
                              Key = ti.Key,
                              Code = ti.Code,
                              Name = ti.Name
                          },
                          Organization = new OrganizationBasicData
                          {
                              Key = org.Key,
                              Code = org.Code,
                              Name = org.Name
                          }
                      }).ToListAsync();
    }

    public async Task SaveOrganizationAsync(Organization organization, CancellationToken cancellationToken)
    {
        List<Tuple<Guid, int, Guid>> parents = [];
        var companies = _context.Companies.Select(x => new Tuple<Guid, int, Guid>(x.Key, 0, x.Key)).ToList();
        parents.AddRange(companies);

        var organizations = _context.Organizations.Select(x => new Tuple<Guid, int, Guid>(x.Key, x.Level, x.CompanyKey)).ToList();
        parents.AddRange(organizations);

        var parentKey = organization.ParentKey;

        if (organization.Key == Guid.Empty)
        {
            //Create Organization
            organization.Key = Guid.NewGuid();

            if (organization.ParentKey != Guid.Empty || organization.ParentKey != null)
            {
                organization.ParentKey = organization.ParentKey;

                var parent = parents.FirstOrDefault(x => x.Item1 == parentKey);
                organization.Level = parent.Item2 + 1;
                organization.CompanyKey = parent.Item3;
            }

            _context.Organizations.Add(organization);
        }
        else
        {
            var organizationEntity = await _context.Organizations.FirstOrDefaultAsync(c => c.Key == organization.Key) ?? throw new Exception("Organization Not Found");

            organizationEntity.Code = organization.Code;
            organizationEntity.Name = organization.Name;

            if (organizationEntity.ParentKey != organization.ParentKey)
            {
                organizationEntity.ParentKey = organization.ParentKey;

                var parent = parents.FirstOrDefault(x => x.Item1 == parentKey);
                organizationEntity.Level = parent.Item2 + 1;
                organizationEntity.CompanyKey = parent.Item3;
            }

            _context.Organizations.Update(organizationEntity);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<CompanyPolicyItemReport>> GetCompanyPolicyReportQuery(CompanyPolicyReportDto report)
    {
        var policies = await (from cp in _context.CompanyPolicies
                              join com in _context.Companies on cp.CompanyKey equals com.Key
                              join org in _context.Organizations on cp.OrganizationKey equals org.Key
                              where cp.OrganizationKey == report.OrganizationKey &&
                                    (report.EffectiveDate == null || cp.EffectiveDate >= report.EffectiveDate)
                              orderby cp.EffectiveDate ascending
                              select new CompanyPolicyItemReport
                              {
                                  CompanyCode = com.Code,
                                  CompanyName = com.Name,
                                  CompanyAddress = com.Address,
                                  OrganizationName = org.Name,
                                  EffectiveDate = cp.EffectiveDate,
                                  ExpiredDate = cp.ExpiredDate,
                                  Policy = cp.Policy
                              }).Distinct().ToListAsync();
        
        return policies;
    }

    public async Task<(IEnumerable<GoJsOrganizationData> Organizations, IEnumerable<GoJsEmployeeData> Employees)> GetOrganizationStructure(Guid Key, bool isCompany)
    {
        if (isCompany)
        {
            var organizations = await _context.OrganizationHierarchies
                .Where(o => o.CompanyKey == Key)
                .Select(o => new GoJsOrganizationData
                {
                    Key = o.OrgKey,
                    Name = o.OrgName,
                    Title = o.NodeType,
                    Code = o.OrgCode,
                    ParentKey = o.ParentKey ?? Guid.Empty,
                    CompanyKey = o.CompanyKey,
                    LogoKey = o.LogoKey,
                    LogoName = o.LogoName,
                    LogoMimeType = o.LogoMimeType
                })
                .ToListAsync();

            // Get all employees for all departments in this company
            var employees = await _context.EmployeeHierarchies
                .Where(e => e.CompanyKey == Key)
                .Select(e => new GoJsEmployeeData
                {
                    Key = e.EmployeeKey,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Code = e.EmployeeCode,
                    PhoneNumber = e.PhoneNumber,
                    Email = e.Email,
                    PositionName = e.PositionName,
                    TitleName = e.TitleName,
                    OrgKey = e.OrgKey,
                    OrgName = e.OrgName,
                    ProfileImageKey = e.ProfileImageKey,
                    ProfileImageName = e.ProfileImageName,
                    ProfileImageMimeType = e.ProfileImageMimeType
                })
                .ToListAsync();

            return (organizations, employees);
        }
        else
        {
            var organizations = await _context.OrganizationHierarchies
                .Where(o => o.OrgKey == Key || o.ParentKey == Key)
                .Select(o => new GoJsOrganizationData
                {
                    Key = o.OrgKey,
                    Name = o.OrgName,
                    Title = o.NodeType,
                    Code = o.OrgCode,
                    ParentKey = o.ParentKey ?? Guid.Empty,
                    CompanyKey = o.CompanyKey,
                    LogoKey = o.LogoKey,
                    LogoName = o.LogoName,
                    LogoMimeType = o.LogoMimeType
                })
                .ToListAsync();

            var employees = await _context.EmployeeHierarchies
                .Where(e => e.OrgKey == Key)
                .Select(e => new GoJsEmployeeData
                {
                    Key = e.EmployeeKey,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Code = e.EmployeeCode,
                    PhoneNumber = e.PhoneNumber,
                    Email = e.Email,
                    PositionName = e.PositionName,
                    OrgKey = e.OrgKey,
                    ProfileImageKey = e.ProfileImageKey,
                    ProfileImageName = e.ProfileImageName,
                    ProfileImageMimeType = e.ProfileImageMimeType
                })
                .ToListAsync();

            return (organizations, employees);
        }
    }
}
