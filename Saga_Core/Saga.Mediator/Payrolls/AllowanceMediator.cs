
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Payrolls;
using Saga.Domain.Entities.Payrolls;
using Saga.DomainShared;
using Saga.Persistence.Context;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Saga.Mediator.Payrolls.AllowanceMediator;

/// <summary>
/// ashari.herman 2025-03-07 slipi jakarta
/// </summary>

#region Get List
public sealed record GetAllowancesQuery(Expression<Func<Allowance, bool>>[] wheres) : IRequest<IEnumerable<Allowance>>;

public sealed class GetAllowancesQueryHandler(IDataContext _context) : IRequestHandler<GetAllowancesQuery, IEnumerable<Allowance>>
{
    public async Task<IEnumerable<Allowance>> Handle(GetAllowancesQuery request, CancellationToken cancellationToken)
    {
        var finds = _context.Allowance.AsQueryable();
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
public sealed record GetAllowanceQuery(Expression<Func<Allowance, bool>>[] wheres) : IRequest<Allowance>;

public sealed class GetAllowanceQueryHandler(IDataContext _context) : IRequestHandler<GetAllowanceQuery, Allowance>
{
    public async Task<Allowance> Handle(GetAllowanceQuery request, CancellationToken cancellationToken)
    {
        var finds = _context.Allowance.AsQueryable();
        request.wheres.ToList()
            .ForEach(where =>
            {
                finds = finds.Where(where);
            });

        var result = finds.FirstOrDefault();
        if(result != null)
            result.AllowanceSubs = _context.AllowanceSub
                                            .Where(x => x.AllowanceKey == result.Key)
                                            .OrderBy(x => x.Name)
                                            .ToArray();

        return await Task.FromResult(result);
    }
}
#endregion

#region save
public sealed record SaveAllowanceCommand(AllowanceDto dto) : IRequest<Result>;

public sealed class SaveAllowanceCommandHandler(
    IDataContext _context,
    IValidator<AllowanceDto> _validator
) : IRequestHandler<SaveAllowanceCommand, Result>
{
    public async Task<Result> Handle(SaveAllowanceCommand command, CancellationToken cancellationToken)
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
            var existing = await _context.Allowance.FirstOrDefaultAsync(s => s.Key == entity.Key, cancellationToken);

            if (existing == null)
            {
                _context.Allowance.Add(entity);
            }
            else
            {
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedBy = existing.CreatedBy;
                _context.Allowance.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            return Result.Failure([$"Error saving Allowance Config: {ex.Message}"]);
        }
        return Result.Success();
    }
}
#endregion

#region Delete
public sealed record DeleteAllowanceCommand(Guid Key) : IRequest<Result<Allowance>>;
public sealed class DeleteAllowanceCommandHandler(IDataContext _context) : IRequestHandler<DeleteAllowanceCommand, Result<Allowance>>
{
    public async Task<Result<Allowance>> Handle(DeleteAllowanceCommand command, CancellationToken cancellationToken)
    {
        var find = await _context.Allowance.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (find == null)
                throw new Exception("Allowance Config's not found.");

            _context.Allowance.Remove(find);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<Allowance>.Failure(new[] { ex.Message });
        }

        return Result<Allowance>.Success(find);
    }
}
#endregion

#region Save Allowance Sub
public sealed record SaveAllowanceSubCommand(AllowanceSubDto Form) : IRequest<Result>;

public sealed class SaveAllowanceSubCommandHandler(
    IDataContext _context,
    IValidator<AllowanceSubDto> _validator
) : IRequestHandler<SaveAllowanceSubCommand, Result>
{
    public async Task<Result> Handle(SaveAllowanceSubCommand command, CancellationToken cancellationToken)
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
            var existing = await _context.AllowanceSub.FirstOrDefaultAsync(s => s.Key == entity.Key, cancellationToken);

            if (existing == null)
            {
                _context.AllowanceSub.Add(entity);
            }
            else
            {
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedBy = existing.CreatedBy;
                _context.AllowanceSub.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            return Result.Failure([$"Error saving Allowance : {ex.Message}"]);
        }
        return Result.Success();
    }
}
#endregion

#region Delete Sub
public sealed record GetAllowanceSubQuery(Guid Key) : IRequest<AllowanceSub?>;
public sealed class GetAllowanceSubQueryHandler(IDataContext _context) : IRequestHandler<GetAllowanceSubQuery, AllowanceSub?>
{
    public async Task<AllowanceSub?> Handle(GetAllowanceSubQuery command, CancellationToken cancellationToken)
    {
        var find = await _context.AllowanceSub.FirstOrDefaultAsync(x => x.Key == command.Key);

        return await Task.FromResult(find);
    }
}
#endregion

#region Delete Sub
public sealed record DeleteAllowanceSubCommand(Guid Key) : IRequest<Result>;
public sealed class DeleteAllowanceSubCommandHandler(IDataContext _context) : IRequestHandler<DeleteAllowanceSubCommand, Result>
{
    public async Task<Result> Handle(DeleteAllowanceSubCommand command, CancellationToken cancellationToken)
    {
        var find = await _context.AllowanceSub.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (find == null)
                throw new Exception("Allowance's not found.");

            _context.AllowanceSub.Remove(find);
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
