using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.ApprovalConfigMediator;

#region "Get List Approval Config"
#region "Query"
public sealed record GetApprovalConfigsQuery(Expression<Func<ApprovalConfig, bool>>[] wheres) : IRequest<IEnumerable<ApprovalConfigListItem>>;
#endregion
#region "Handler"
public sealed class GetApprovalConfigsQueryHandler : IRequestHandler<GetApprovalConfigsQuery, IEnumerable<ApprovalConfigListItem>>
{
    private readonly IDataContext _context;

    public GetApprovalConfigsQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ApprovalConfigListItem>> Handle(GetApprovalConfigsQuery request, CancellationToken cancellationToken)
    {
        var queries = from apc in _context.ApprovalConfigs
                      join org in _context.Organizations on apc.OrganizationKey equals org.Key
                      join com in _context.Companies on org.CompanyKey equals com.Key
                      where apc.DeletedAt == null
                      select new
                      {
                          ApprovalConfig = apc,
                          Organization = org,
                          Company = com
                      };

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x.ApprovalConfig));
        }

        var approvalConfigs = await queries.ToListAsync();

        var viewModel = approvalConfigs.Select(x => x.ApprovalConfig.ConvertToListItem(x.Company)).ToList();
        
        return viewModel;
    }
}
#endregion
#endregion

#region "Get List Approval Config With Pagination"
#region "Query"
public sealed record GetApprovalConfigsPaginationQuery(PaginationConfig pagination, Expression<Func<ApprovalConfig, bool>>[] wheres) : IRequest<PaginatedList<ApprovalConfigListItem>>;
#endregion
#region "Handler"
public sealed class GetApprovalConfigsPaginationQueryHandler : IRequestHandler<GetApprovalConfigsPaginationQuery, PaginatedList<ApprovalConfigListItem>>
{
    private readonly IDataContext _context;

    public GetApprovalConfigsPaginationQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ApprovalConfigListItem>> Handle(GetApprovalConfigsPaginationQuery request, CancellationToken cancellationToken)
    {
        var queries = from apc in _context.ApprovalConfigs
                      join org in _context.Organizations on apc.OrganizationKey equals org.Key
                      join com in _context.Companies on org.CompanyKey equals com.Key
                      where apc.DeletedAt == null
                      select new
                      {
                          ApprovalConfig = apc,
                          Organization = org,
                          Company = com
                      };

        string search = request.pagination.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Company.Code, $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%") || EF.Functions.ILike(b.Organization.Name, $"%{search}%") || EF.Functions.ILike(b.ApprovalConfig.Description, $"%{search}%"));
        }

        var result = await queries.Select(x => new ApprovalConfigListItem
        {
            Key = x.ApprovalConfig.Key,
            OrganizationKey = x.ApprovalConfig.OrganizationKey,
            CompanyCode = x.Company.Code,
            CompanyName = x.Company.Name,
            Description = x.ApprovalConfig.Description,
            Organization = x.Organization
        }).PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

        return await Task.FromResult(result);
    }
}
#endregion
#endregion

#region "Get By Id Approval Config"
#region "Query"
public sealed record GetApprovalConfigQuery(Guid Key) : IRequest<ApprovalConfigForm>;
#endregion
#region "Handler"
public sealed class GetApprovalConfigQueryHandler : IRequestHandler<GetApprovalConfigQuery, ApprovalConfigForm>
{
    private readonly IDataContext _context; 

    public GetApprovalConfigQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<ApprovalConfigForm> Handle(GetApprovalConfigQuery request, CancellationToken cancellationToken)
    {
        var approvalConfig = await (from apc in _context.ApprovalConfigs
                                    join org in _context.Organizations on apc.OrganizationKey equals org.Key
                                    where apc.Key == request.Key
                                    select new ApprovalConfig
                                    {
                                        Key = apc.Key,
                                        OrganizationKey = apc.OrganizationKey,
                                        Description = apc.Description,
                                        Organization = org
                                    }).FirstOrDefaultAsync();

        if (approvalConfig == null)
            throw new Exception("Approval Config Not Found");

        var approvers = await (from apr in _context.Approvers
                               join e in _context.Employees on apr.EmployeeKey equals e.Key
                               where apr.ApprovalConfigKey == approvalConfig.Key && apr.DeletedAt == null
                               select new Approver
                               {
                                   Key = apr.Key,
                                   ApprovalConfigKey = apr.ApprovalConfigKey,
                                   EmployeeKey = apr.EmployeeKey,
                                   Name = apr.Name,
                                   Level = apr.Level,
                                   Action = apr.Action,
                                   ApprovalConfig = approvalConfig,
                                   User = e
                               }).ToListAsync();

        var approversForm = approvers.Select(x => x.ConvertToApproverListItem());
        approvalConfig.Approvers = approvers;

        var viewModel = approvalConfig.ConvertToForm();
        viewModel.JsonApprovers = JsonConvert.SerializeObject(approversForm, Formatting.None);

        return viewModel;
    }
}
#endregion
#endregion

#region "Save Approval Config"
#region "Command"
public sealed record SaveApprovalConfigCommand(ApprovalConfigDto Form) : IRequest<Result>;
#endregion
#region "Handler"
public sealed class SaveApprovalConfigCommandHandler : IRequestHandler<SaveApprovalConfigCommand, Result>
{
    private readonly IDataContext _context;
    private readonly IValidator<ApprovalConfigDto> _validator;
    private readonly IValidator<ApproverDto> _approverValidator;

    public SaveApprovalConfigCommandHandler(IDataContext context, IValidator<ApprovalConfigDto> validator, IValidator<ApproverDto> approverValidator)
    {
        _context = context;
        _validator = validator;
        _approverValidator = approverValidator;
    }

    public async Task<Result> Handle(SaveApprovalConfigCommand command, CancellationToken cancellationToken)
    {
        try
        {
            //Validate Approval Config
            ValidationResult validator = await _validator.ValidateAsync(command.Form);
            if (!validator.IsValid)
            {
                var failures = validator.Errors
                                        .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                        .ToList();
                return Result.Failure(failures);
            }

            var approvalConfig = command.Form.ConvertToEntity();

            if (approvalConfig.Key == Guid.Empty)
            {
                approvalConfig.Key = Guid.NewGuid();
            }

            //Check if approval config is existing
            var existingApprovalConfig = await _context.ApprovalConfigs.FirstOrDefaultAsync(x => x.Key == approvalConfig.Key, cancellationToken);
            if (existingApprovalConfig == null)
            {
                //Add new Approval Config
                _context.ApprovalConfigs.Add(approvalConfig);
            }
            else
            {
                //Update existing Approval Config
                _context.ApprovalConfigs.Update(approvalConfig);
            }

            if (command.Form.Approvers != null && command.Form.Approvers.Any())
            {
                foreach (var approverDto in command.Form.Approvers)
                {
                    approverDto.ApprovalConfigKey = approvalConfig.Key;
                    ValidationResult approverValidator = await _approverValidator.ValidateAsync(approverDto);
                    if (!approverValidator.IsValid)
                        return Result.Failure(approverValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                }

                _ = await _context.Approvers
                                  .Where(x => x.ApprovalConfigKey == approvalConfig.Key)
                                  .ExecuteDeleteAsync();

                var approversEntities = command.Form.Approvers.Select(x =>
                {
                    var entity = x.ConvertToEntity();
                    entity.Key = Guid.NewGuid();
                    entity.ApprovalConfigKey = approvalConfig.Key;
                    return entity;
                });

                await _context.Approvers.AddRangeAsync(approversEntities, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            return Result.Failure(new[] { $"Error saving approval config: {ex.Message}" });
        }

        return Result.Success();
    }
}
#endregion
#endregion

#region "Delete Approval Config"
#region "Command"
public sealed record DeleteApprovalConfigCommand(Guid Key) : IRequest<Result<ApprovalConfig>>;
#endregion
#region "Handler"
public sealed class DeleteApprovalConfigCommandHandler : IRequestHandler<DeleteApprovalConfigCommand, Result<ApprovalConfig>>
{
    private readonly IDataContext _context;

    public DeleteApprovalConfigCommandHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<Result<ApprovalConfig>> Handle(DeleteApprovalConfigCommand command, CancellationToken cancellationToken)
    {
        var approvalConfig = await _context.ApprovalConfigs.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (approvalConfig == null)
                throw new Exception("Approval Config Not Found");

            var approvers = await _context.Approvers.Where(x => x.ApprovalConfigKey == approvalConfig.Key).ToListAsync();
            if (approvers.Any())
            {
                _context.Approvers.RemoveRange(approvers);
            }

            _context.ApprovalConfigs.Remove(approvalConfig);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<ApprovalConfig>.Failure(new[] { ex.Message });
        }

        return Result<ApprovalConfig>.Success(approvalConfig);
    }
}
#endregion
#endregion
