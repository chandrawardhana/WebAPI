
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Payrolls;
using Saga.Domain.Entities.Payrolls;
using Saga.DomainShared;
using Saga.Persistence.Context;
using System.Linq.Expressions;

namespace Saga.Mediator.Payrolls.PayrollTaxConfigMediator;


#region Get List
public sealed record GetPayrollTaxConfigsQuery(Expression<Func<PayrollTaxConfig, bool>>[] wheres) : IRequest<IEnumerable<PayrollTaxConfig>>;
public sealed class GetPayrollTaxConfigsQueryHandler(IDataContext _context) : IRequestHandler<GetPayrollTaxConfigsQuery, IEnumerable<PayrollTaxConfig>>
{
    public async Task<IEnumerable<PayrollTaxConfig>> Handle(GetPayrollTaxConfigsQuery request, CancellationToken cancellationToken)
    {
        var finds = _context.PayrollTaxConfig.AsQueryable();
        request.wheres.ToList().ForEach(where =>
        {
            finds = finds.Where(where);
        });

        return await Task.FromResult(finds.ToArray());
    }
}

#endregion


#region Get By ID
public sealed record GetPayrollTaxConfigByKeyQuery(Guid key) : IRequest<PayrollTaxConfig>;
public sealed class GetPayrollTaxConfigByKeyQueryHandler(IDataContext _context) : IRequestHandler<GetPayrollTaxConfigByKeyQuery, PayrollTaxConfig>
{
    public async Task<PayrollTaxConfig> Handle(GetPayrollTaxConfigByKeyQuery request, CancellationToken cancellationToken)
    {
        var find = _context.PayrollTaxConfig.FirstOrDefault(x => x.Key == request.key);
       
        return await Task.FromResult(find);
    }
}
#endregion

#region Save
public sealed record SavePayrollTaxConfigCommand(PayrollTaxConfigDto Form) : IRequest<Result>;
public sealed class SavePayrollTaxConfigCommandHandler(
    IDataContext _context,
    IValidator<PayrollTaxConfigDto> _validator
) : IRequestHandler<SavePayrollTaxConfigCommand, Result>
{
    public async Task<Result> Handle(SavePayrollTaxConfigCommand command, CancellationToken cancellationToken)
    {
        try
        {
            ValidationResult validation = await _validator.ValidateAsync(command.Form, cancellationToken);
            if (!validation.IsValid)
            {
                var failures = validation.Errors
                                        .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                        .ToList();
                return Result.Failure(failures);
            }

            var entity = command.Form.ConvertToEntity();

            if (entity.Key == Guid.Empty)
                entity.Key = Guid.NewGuid();

            // Check if exists
            var existing = await _context.PayrollTaxConfig.FirstOrDefaultAsync(s => s.Key == entity.Key, cancellationToken);

            if (existing == null)
            {
                _context.PayrollTaxConfig.Add(entity);
            }
            else
            {
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedBy = existing.CreatedBy;
                _context.PayrollTaxConfig.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            return Result.Failure([$"Error saving PPH21 Config: {ex.Message}"]);
        }
        return Result.Success();
    }
}
#endregion

#region Delete
public sealed record DeletePayrollTaxConfigCommand(Guid Key) : IRequest<Result<PayrollTaxConfig>>;
public sealed class DeletePayrollTaxConfigCommandHandler(IDataContext _context) : IRequestHandler<DeletePayrollTaxConfigCommand, Result<PayrollTaxConfig>>
{
    public async Task<Result<PayrollTaxConfig>> Handle(DeletePayrollTaxConfigCommand command, CancellationToken cancellationToken)
    {
        var find = await _context.PayrollTaxConfig.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (find == null)
                throw new Exception("PPH21 Config's not found.");

            _context.PayrollTaxConfig.Remove(find);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<PayrollTaxConfig>.Failure(new[] { ex.Message });
        }

        return Result<PayrollTaxConfig>.Success(find);
    }
}
#endregion
