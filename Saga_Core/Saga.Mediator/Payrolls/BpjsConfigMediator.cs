
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Payrolls;
using Saga.Domain.Entities.Payrolls;
using Saga.DomainShared;
using Saga.Persistence.Context;
using System.Linq.Expressions;

namespace Saga.Mediator.Payrolls.BpjsConfigMediator;

/// <summary>
/// ashari.herman 2025-03-12 slipi jakarta
/// </summary>

#region Get List
public sealed record GetBpjsConfigsQuery(Expression<Func<BpjsConfig, bool>>[] wheres) : IRequest<IEnumerable<BpjsConfig>>;

public sealed class GetBpjsConfigsQueryHandler(IDataContext _context) : IRequestHandler<GetBpjsConfigsQuery, IEnumerable<BpjsConfig>>
{
    public async Task<IEnumerable<BpjsConfig>> Handle(GetBpjsConfigsQuery request, CancellationToken cancellationToken)
    {
        var finds = _context.BpjsConfig.AsQueryable();
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
public sealed record GetBpjsConfigQuery(Expression<Func<BpjsConfig, bool>>[] wheres) : IRequest<BpjsConfig>;

public sealed class GetBpjsConfigQueryHandler(IDataContext _context) : IRequestHandler<GetBpjsConfigQuery, BpjsConfig>
{
    public async Task<BpjsConfig> Handle(GetBpjsConfigQuery request, CancellationToken cancellationToken)
    {
        var finds = _context.BpjsConfig.AsQueryable();
        request.wheres.ToList()
            .ForEach(where =>
            {
                finds = finds.Where(where);
            });

        var result = finds.FirstOrDefault();
        if (result != null)
            result.BpjsSubs = _context.BpjsSubConfig
                                            .Where(x => x.BpjsConfigKey == result.Key)
                                            .OrderBy(x => x.Name)
                                            .ToArray();

        return await Task.FromResult(result);
    }
}
#endregion

#region save
public sealed record SaveBpjsConfigCommand(BpjsConfigDto dto) : IRequest<Result>;

public sealed class SaveBpjsConfigCommandHandler(
    IDataContext _context,
    IValidator<BpjsConfigDto> _validator
) : IRequestHandler<SaveBpjsConfigCommand, Result>
{
    public async Task<Result> Handle(SaveBpjsConfigCommand command, CancellationToken cancellationToken)
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
            var existing = await _context.BpjsConfig.FirstOrDefaultAsync(s => s.Key == entity.Key, cancellationToken);

            if (existing == null)
            {
                _context.BpjsConfig.Add(entity);
            }
            else
            {
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedBy = existing.CreatedBy;
                _context.BpjsConfig.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            return Result.Failure([$"Error saving BPJS Config: {ex.Message}"]);
        }
        return Result.Success();
    }
}
#endregion

#region Delete
public sealed record DeleteBpjsConfigCommand(Guid Key) : IRequest<Result<BpjsConfig>>;
public sealed class DeleteBpjsConfigCommandHandler(IDataContext _context) : IRequestHandler<DeleteBpjsConfigCommand, Result<BpjsConfig>>
{
    public async Task<Result<BpjsConfig>> Handle(DeleteBpjsConfigCommand command, CancellationToken cancellationToken)
    {
        var find = await _context.BpjsConfig.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (find == null)
                throw new Exception("BPJS Config's not found.");

            _context.BpjsConfig.Remove(find);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<BpjsConfig>.Failure(new[] { ex.Message });
        }

        return Result<BpjsConfig>.Success(find);
    }
}
#endregion

#region Save BpjsConfig Sub
public sealed record SaveBpjsSubConfigCommand(BpjsSubConfigDto Form) : IRequest<Result>;

public sealed class SaveBpjsSubConfigCommandHandler(
    IDataContext _context,
    IValidator<BpjsSubConfigDto> _validator
) : IRequestHandler<SaveBpjsSubConfigCommand, Result>
{
    public async Task<Result> Handle(SaveBpjsSubConfigCommand command, CancellationToken cancellationToken)
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
            var existing = await _context.BpjsSubConfig.FirstOrDefaultAsync(s => s.Key == entity.Key, cancellationToken);

            if (existing == null)
            {
                _context.BpjsSubConfig.Add(entity);
            }
            else
            {
                entity.CreatedAt = existing.CreatedAt;
                entity.CreatedBy = existing.CreatedBy;
                _context.BpjsSubConfig.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            return Result.Failure([$"Error saving BPJS Config : {ex.Message}"]);
        }
        return Result.Success();
    }
}
#endregion

#region Delete Sub
public sealed record GetBpjsSubConfigQuery(Guid Key) : IRequest<BpjsSubConfig?>;
public sealed class GetBpjsSubConfigQueryHandler(IDataContext _context) : IRequestHandler<GetBpjsSubConfigQuery, BpjsSubConfig?>
{
    public async Task<BpjsSubConfig?> Handle(GetBpjsSubConfigQuery command, CancellationToken cancellationToken)
    {
        var find = await _context.BpjsSubConfig.FirstOrDefaultAsync(x => x.Key == command.Key);

        return await Task.FromResult(find);
    }
}
#endregion

#region Delete Sub
public sealed record DeleteBpjsSubConfigCommand(Guid Key) : IRequest<Result>;
public sealed class DeleteBpjsSubConfigCommandHandler(IDataContext _context) : IRequestHandler<DeleteBpjsSubConfigCommand, Result>
{
    public async Task<Result> Handle(DeleteBpjsSubConfigCommand command, CancellationToken cancellationToken)
    {
        var find = await _context.BpjsSubConfig.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (find == null)
                throw new Exception("BPJS Config's not found.");

            _context.BpjsSubConfig.Remove(find);
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
