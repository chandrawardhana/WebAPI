
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Dtos.Payrolls;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Payrolls;
using Saga.DomainShared;
using Saga.Mediator.Attendances.ShiftMediator;
using Saga.Persistence.Context;
using System.Linq.Expressions;

namespace Saga.Mediator.Payrolls.AverageEffectiveRateMediator;

#region Get List
public sealed record GetAverageEffectiveRatesQuery(Expression<Func<AverageEffectiveRate, bool>>[] wheres) : IRequest<IEnumerable<AverageEffectiveRate>>;
public sealed class GetAverageEffectiveRatesQueryHandler(IDataContext _context) : IRequestHandler<GetAverageEffectiveRatesQuery, IEnumerable<AverageEffectiveRate>>
{
    public async Task<IEnumerable<AverageEffectiveRate>> Handle(GetAverageEffectiveRatesQuery request, CancellationToken cancellationToken)
    {
        var finds = _context.AverageEffectiveRate.AsQueryable();
        request.wheres.ToList().ForEach(where =>
        {
            finds = finds.Where(where);
        });

        var results = finds.ToList();
        results.ForEach(x =>
        {
            x.Details = [.. _context.AverageEffectiveRateDetail
                                .Where(y => y.ParentKey == x.Key)
                                .OrderBy(y => y.Order)];
        });

        return await Task.FromResult(results);
    }
}

#endregion


#region Get By ID
public sealed record GetAverageEffectiveRateByKeyQuery(Guid key) : IRequest<AverageEffectiveRate>;
public sealed class GetAverageEffectiveRateByKeyQueryHandler(IDataContext _context) : IRequestHandler<GetAverageEffectiveRateByKeyQuery, AverageEffectiveRate>
{
    public async Task<AverageEffectiveRate> Handle(GetAverageEffectiveRateByKeyQuery request, CancellationToken cancellationToken)
    {
        var find = _context.AverageEffectiveRate.FirstOrDefault(x => x.Key == request.key);
        if (find != null)
        {
            find.Details = [.. _context.AverageEffectiveRateDetail
                                .Where(y => y.ParentKey == find.Key)
                                .OrderBy(y => y.Order)];
        }
        return await Task.FromResult(find);
    }
}
#endregion

#region Save
public sealed record SaveAverageEffectiveRateCommand(AverageEffectiveRateDto Form) : IRequest<Result>;
public sealed class SaveAverageEffectiveRateCommandHandler(
    IDataContext _context,
    IValidator<AverageEffectiveRateDto> _rateValidator
) : IRequestHandler<SaveAverageEffectiveRateCommand, Result>
{
    public async Task<Result> Handle(SaveAverageEffectiveRateCommand command, CancellationToken cancellationToken)
    {
        try
        {
            ValidationResult rateValidator = await _rateValidator.ValidateAsync(command.Form, cancellationToken);
            if (!rateValidator.IsValid)
            {
                var failures = rateValidator.Errors
                                        .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                        .ToList();
                return Result.Failure(failures);
            }

            var rate = command.Form.ConvertToEntity();

            if (rate.Key == Guid.Empty)
                rate.Key = Guid.NewGuid();

            // Check if exists
            var existing = await _context.AverageEffectiveRate.FirstOrDefaultAsync(s => s.Key == rate.Key, cancellationToken);

            if (existing == null)
            {
                _context.AverageEffectiveRate.Add(rate);
            }
            else
            {
                rate.CreatedAt = existing.CreatedAt;
                rate.CreatedBy = existing.CreatedBy;
                _context.AverageEffectiveRate.Entry(existing).CurrentValues.SetValues(rate);
            }

            if (rate.Details != null && rate.Details.Any())
            {
                rate.Details.ToList().ForEach(x =>
                {
                    x.Key = Guid.NewGuid();
                    x.ParentKey = rate.Key;
                });
                _context.AverageEffectiveRateDetail.Where(x => x.ParentKey == rate.Key).ExecuteDelete();
                _context.AverageEffectiveRateDetail.AddRange(rate.Details);

            }
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(new[] { $"Error saving TER: {ex.Message}" });
        }
        return Result.Success();
    }
}
#endregion

#region Delete
public sealed record DeleteAverageEffectiveRateCommand(Guid Key) : IRequest<Result<AverageEffectiveRate>>;
public sealed class DeleteAverageEffectiveRateCommandHandler(IDataContext _context) : IRequestHandler<DeleteAverageEffectiveRateCommand, Result<AverageEffectiveRate>>
{
    public async Task<Result<AverageEffectiveRate>> Handle(DeleteAverageEffectiveRateCommand command, CancellationToken cancellationToken)
    {
        var find = await _context.AverageEffectiveRate.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (find == null)
                throw new Exception("Average Effective Rate's not found.");

            _context.AverageEffectiveRate.Remove(find);
            _context.AverageEffectiveRateDetail.Where(sd => sd.ParentKey == find.Key).ExecuteDelete();
            _ = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<AverageEffectiveRate>.Failure(new[] { ex.Message });
        }

        return Result<AverageEffectiveRate>.Success(find);
    }
}
#endregion