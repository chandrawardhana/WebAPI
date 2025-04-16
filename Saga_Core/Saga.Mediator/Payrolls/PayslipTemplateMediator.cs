
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Payrolls;
using Saga.Domain.Entities.Payrolls;
using Saga.DomainShared;
using Saga.Persistence.Context;
using System.Linq.Expressions;

namespace Saga.Mediator.Payrolls.PayslipTemplateMediator;

/// <summary>
/// ashari.herman 2025-03-12 slipi jakarta
/// </summary>

#region Get List
public sealed record GetPayslipTemplatesQuery(Expression<Func<PayslipTemplate, bool>>[] wheres) : IRequest<IEnumerable<PayslipTemplate>>;

public sealed class GetPayslipTemplatesQueryHandler(IDataContext _context) : IRequestHandler<GetPayslipTemplatesQuery, IEnumerable<PayslipTemplate>>
{
    public async Task<IEnumerable<PayslipTemplate>> Handle(GetPayslipTemplatesQuery request, CancellationToken cancellationToken)
    {
        var finds = _context.PayslipTemplate.AsQueryable();
        request.wheres.ToList()
            .ForEach(where =>
            {
                finds = finds.Where(where);
            });

        return await Task.FromResult(finds.ToArray());
    }
}
#endregion

#region Get By ID
public sealed record GetPayslipTemplateQuery(Expression<Func<PayslipTemplate, bool>>[] wheres) : IRequest<PayslipTemplate>;

public sealed class GetPayslipTemplateQueryHandler(IDataContext _context) : IRequestHandler<GetPayslipTemplateQuery, PayslipTemplate>
{
    public async Task<PayslipTemplate> Handle(GetPayslipTemplateQuery request, CancellationToken cancellationToken)
    {
        var finds = _context.PayslipTemplate.AsQueryable();
        request.wheres.ToList()
            .ForEach(where =>
            {
                finds = finds.Where(where);
            });

        var result = finds.FirstOrDefault();
        if (result != null)
            result.Details = [.. _context.PayslipTemplateDetail
                                            .Where(x => x.ParentKey == result.Key)
                                            .OrderBy(x => x.Order)];

        return await Task.FromResult(result);
    }
}
#endregion

#region save
public sealed record SavePayslipTemplateCommand(PayslipTemplateDto dto) : IRequest<Result>;

public sealed class SavePayslipTemplateCommandHandler(
    IDataContext _context,
    IValidator<PayslipTemplateDto> _validator
) : IRequestHandler<SavePayslipTemplateCommand, Result>
{
    public async Task<Result> Handle(SavePayslipTemplateCommand command, CancellationToken cancellationToken)
    {
        try
        {
            ValidationResult validation = await _validator.ValidateAsync(command.dto, cancellationToken);
            if (!validation.IsValid)
            {
                var failures = validation.Errors
                                        .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                        .ToList();
                return Result.Failure(failures);
            }

            var entity = command.dto.ConvertToEntity();

            if (entity.Key == Guid.Empty)
                entity.Key = Guid.NewGuid();

            // Check if exists
            var existing = await _context.PayslipTemplate.FirstOrDefaultAsync(s => s.Key == entity.Key, cancellationToken);

            if (existing == null)
            {
                _context.PayslipTemplate.Add(entity);
            }
            else
            {
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedBy = existing.CreatedBy;
                _context.PayslipTemplate.Entry(existing).CurrentValues.SetValues(entity);
            }

            _context.PayslipTemplateDetail.Where(x => x.ParentKey == entity.Key).ExecuteDelete();
            entity.Details.ToList().ForEach(x =>
            {
                x.Key = Guid.NewGuid();
                x.ParentKey = entity.Key;
            });
            _context.PayslipTemplateDetail.AddRange(entity.Details);

            await _context.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            return Result.Failure([$"Error saving Payslip Template: {ex.Message}"]);
        }
        return Result.Success();
    }
}
#endregion

#region Delete
public sealed record DeletePayslipTemplateCommand(Guid Key) : IRequest<Result<PayslipTemplate>>;
public sealed class DeletePayslipTemplateCommandHandler(IDataContext _context) : IRequestHandler<DeletePayslipTemplateCommand, Result<PayslipTemplate>>
{
    public async Task<Result<PayslipTemplate>> Handle(DeletePayslipTemplateCommand command, CancellationToken cancellationToken)
    {
        var find = await _context.PayslipTemplate.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (find == null)
                throw new Exception("Payslip Template's not found.");

            _context.PayslipTemplate.Remove(find);
            
            var details = _context.PayslipTemplateDetail.Where(x => x.ParentKey == find.Key).ToArray();
            _context.PayslipTemplateDetail.RemoveRange(details);

            _ = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<PayslipTemplate>.Failure(new[] { ex.Message });
        }

        return Result<PayslipTemplate>.Success(find);
    }
}
#endregion

#region Delete Sub
public sealed record DeletePayslipTemplateDetailCommand(Guid Key) : IRequest<Result>;
public sealed class DeletePayslipTemplateDetailCommandHandler(IDataContext _context) : IRequestHandler<DeletePayslipTemplateDetailCommand, Result>
{
    public async Task<Result> Handle(DeletePayslipTemplateDetailCommand command, CancellationToken cancellationToken)
    {
        var find = await _context.PayslipTemplateDetail.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (find == null)
                throw new Exception("Payslip Template's not found.");

            _context.PayslipTemplateDetail.Remove(find);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure([ex.Message]);
        }

        return Result.Success();
    }
}
#endregion
