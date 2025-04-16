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
using Saga.Validators.Attendances;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.OvertimeRateMediator;

#region "Get List OvertimeRate"
#region "Query"
public sealed record GetOvertimeRatesQuery(Expression<Func<OvertimeRate, bool>>[] wheres) : IRequest<OvertimeRateList>;
#endregion
#region "Handler"
public sealed class GetOvertimeRatesQueryHandler : IRequestHandler<GetOvertimeRatesQuery, OvertimeRateList>
{
    private readonly IDataContext _context;

    public GetOvertimeRatesQueryHandler(IDataContext context)
    {
        _context = context;
    }
    public async Task<OvertimeRateList> Handle(GetOvertimeRatesQuery request, CancellationToken cancellationToken)
    {
        var queries = from ovr in _context.OvertimeRates
                      join com in _context.Companies on ovr.CompanyKey equals com.Key
                      where ovr.DeletedAt == null
                      select new OvertimeRate
                      {
                          Key = ovr.Key,
                          CompanyKey = ovr.CompanyKey,
                          GroupName = ovr.GroupName,
                          BaseOnDay = ovr.BaseOnDay,
                          MaxHour = ovr.MaxHour,
                          Company = com
                      };

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x));
        }

        var overtimeRates = await queries.ToListAsync();

        var viewModel = new OvertimeRateList
        {
            OvertimeRates = overtimeRates.Select(ovr => ovr.ConvertToViewModelOvertimeRateItemList())
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Get List OvertimeRate With Pagination"
#region "Query"
public sealed record GetOvertimeRatesPaginationQuery(PaginationConfig pagination, Expression<Func<OvertimeRate, bool>>[] wheres) : IRequest<PaginatedList<OvertimeRate>>;
#endregion
#region "Handler"
public sealed class GetOvertimeRatesPaginationQueryHandler : IRequestHandler<GetOvertimeRatesPaginationQuery, PaginatedList<OvertimeRate>>
{
    private readonly IDataContext _context;

    public GetOvertimeRatesPaginationQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<OvertimeRate>> Handle(GetOvertimeRatesPaginationQuery request, CancellationToken cancellationToken)
    {
        var queries = from ovr in _context.OvertimeRates
                      join com in _context.Companies on ovr.CompanyKey equals com.Key
                      where ovr.DeletedAt == null
                      select new OvertimeRate
                      {
                          CompanyKey = ovr.CompanyKey,
                          GroupName = ovr.GroupName,
                          BaseOnDay = ovr.BaseOnDay,
                          MaxHour = ovr.MaxHour,
                          Company = com
                      };

        string search = request.pagination.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.GroupName, $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%"));
        }

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x));
        }

        var overtimeRates = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

        return await Task.FromResult(overtimeRates);
    }
}
#endregion
#endregion

#region "Get OvertimeRate By Id"
#region "Query"
public sealed record GetOvertimeRateQuery(Guid Key) : IRequest<OvertimeRateForm>;
#endregion
#region "Handler"
    public sealed class GetOvertimeRateQueryHandler : IRequestHandler<GetOvertimeRateQuery, OvertimeRateForm>
    {
        private readonly IDataContext _context;

        public GetOvertimeRateQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<OvertimeRateForm> Handle(GetOvertimeRateQuery request, CancellationToken cancellationToken)
        {
            var overtimeRate = await (from or in _context.OvertimeRates
                                      join com in _context.Companies on or.CompanyKey equals com.Key
                                      where or.Key == request.Key
                                      select new OvertimeRate
                                      {
                                          Key = or.Key,
                                          CompanyKey = or.CompanyKey,
                                          GroupName = or.GroupName,
                                          BaseOnDay = or.BaseOnDay,
                                          MaxHour = or.MaxHour,
                                          Company = com
                                      }).FirstOrDefaultAsync();

            if (overtimeRate == null)
                throw new Exception("Overtime Rate Not Found.");

            var overtimeRateDetails = await (from ord in _context.OvertimeRateDetails
                                             where ord.OvertimeRateKey == request.Key && ord.DeletedAt == null
                                             select new OvertimeRateDetail
                                             {
                                                 Key = ord.Key,
                                                 OvertimeRateKey = ord.OvertimeRateKey,
                                                 Level = ord.Level,
                                                 Hours = ord.Hours,
                                                 Multiply = ord.Multiply
                                             }).ToListAsync();

            var overtimeRateDetailsForm = overtimeRateDetails.Select(ord => ord.ConvertToViewModelOvertimeRateDetailForm());

            overtimeRate.OvertimeRateDetails = overtimeRateDetails;

            var viewModel = overtimeRate.ConvertToViewModelOvertimeRateForm();
            viewModel.JsonOvertimeRateDetails = JsonConvert.SerializeObject(overtimeRateDetailsForm, Formatting.None);

            return viewModel;
        }
    }
#endregion
#endregion

#region "Save OvertimeRate"
#region "Command"
    public sealed record SaveOvertimeRateCommand(OvertimeRateDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveOvertimeRateCommandHandler : IRequestHandler<SaveOvertimeRateCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<OvertimeRateDto> _overtimeRateValidator;
        private readonly IValidator<OvertimeRateDetailDto> _overtimeRateDetailValidator;

        public SaveOvertimeRateCommandHandler(IDataContext context, 
                                              IValidator<OvertimeRateDto> overtimeRateValidator,
                                              IValidator<OvertimeRateDetailDto> overtimeRateDetailValidator)
        {
            _context = context;
            _overtimeRateValidator = overtimeRateValidator;
            _overtimeRateDetailValidator = overtimeRateDetailValidator;
        }

        public async Task<Result> Handle(SaveOvertimeRateCommand command, CancellationToken cancellationToken)
        {
            try
            {
                //Validate overtimeRate 
                ValidationResult overtimeRateValidator = await _overtimeRateValidator.ValidateAsync(command.Form);
                if (!overtimeRateValidator.IsValid)
                {
                    var failures = overtimeRateValidator.Errors
                                                .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                                .ToList();
                    return Result.Failure(failures);
                }

                var overtimeRate = command.Form.ConvertToEntity();

                if (overtimeRate.Key == Guid.Empty)
                {
                    overtimeRate.Key = Guid.NewGuid();
                }

                //Check if overtimeRate exists
                var existingOvertimeRate = await _context.OvertimeRates.FirstOrDefaultAsync(or => or.Key == overtimeRate.Key && or.DeletedAt == null, cancellationToken);
                if (existingOvertimeRate == null)
                {
                    //Add new overtimeRate
                    _context.OvertimeRates.Add(overtimeRate);
                }
                else
                {
                    //Update existing overtimeRate
                    overtimeRate.CreatedAt = existingOvertimeRate.CreatedAt;
                    overtimeRate.CreatedBy = existingOvertimeRate.CreatedBy;
                    _context.OvertimeRates.Entry(existingOvertimeRate).CurrentValues.SetValues(overtimeRate);
                }

                //Save overtimeRate to get the Key
                await _context.SaveChangesAsync(cancellationToken);

                if (command.Form.OvertimeRateDetails != null && command.Form.OvertimeRateDetails.Any())
                {
                    foreach(var overtimeRateDetailDto in command.Form.OvertimeRateDetails)
                    {
                        overtimeRateDetailDto.Key = overtimeRate.Key;
                        ValidationResult overtimeRateDetailValidator = await _overtimeRateDetailValidator.ValidateAsync(overtimeRateDetailDto);
                        if (!overtimeRateDetailValidator.IsValid)
                            return Result.Failure(overtimeRateDetailValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    _ = await _context.OvertimeRateDetails
                                      .Where(x => x.OvertimeRateKey == overtimeRate.Key)
                                      .ExecuteDeleteAsync();

                    var overtimeRateDetailEntities = command.Form.OvertimeRateDetails.Select(ord =>
                    {
                        var entity = ord.ConvertToEntity();
                        entity.Key = Guid.NewGuid();
                        entity.OvertimeRateKey = overtimeRate.Key;
                        return entity;
                    });

                    await _context.OvertimeRateDetails.AddRangeAsync(overtimeRateDetailEntities, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }
            return Result.Success();
        }
    }
#endregion
#endregion

#region "Delete OvertimeRate"
#region "Command"
    public sealed record DeleteOvertimeRateCommand(Guid Key) : IRequest<Result<OvertimeRate>>;
#endregion
#region "Handler"
    public sealed class DeleteOvertimeRateCommandHandler : IRequestHandler<DeleteOvertimeRateCommand, Result<OvertimeRate>>
    {
        private readonly IDataContext _context;

        public DeleteOvertimeRateCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<OvertimeRate>> Handle(DeleteOvertimeRateCommand command, CancellationToken cancellationToken)
        {
            var overtimeRate = await _context.OvertimeRates.FirstOrDefaultAsync(x => x.Key == command.Key);

            try
            {
                if (overtimeRate == null)
                    throw new Exception("Overtime Rate not found.");

                //Check if OvertimeRateDetails exist for this OvertimeRate
                var overtimeRateDetails = await _context.OvertimeRateDetails.Where(sd => sd.OvertimeRateKey == overtimeRate.Key).ToListAsync();
                if (overtimeRateDetails.Any())
                {
                    _context.OvertimeRateDetails.RemoveRange(overtimeRateDetails);
                }

                _context.OvertimeRates.Remove(overtimeRate);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<OvertimeRate>.Failure(new[] { ex.Message });
            }
            return Result<OvertimeRate>.Success(overtimeRate);
        }
    }
#endregion
#endregion

#region "Get List Overtime Rate With Details"
#region "Query"
public sealed record GetOvertimeRateWithDetailsQuery(Expression<Func<OvertimeRate, bool>>[] wheres) : IRequest<IEnumerable<OvertimeRate>>;
#endregion
#region "Handler"
public sealed class GetOvertimeRateWithDetailsQueryHandler : IRequestHandler<GetOvertimeRateWithDetailsQuery, IEnumerable<OvertimeRate>>
{
    private readonly IDataContext _context;

    public GetOvertimeRateWithDetailsQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OvertimeRate>> Handle(GetOvertimeRateWithDetailsQuery request, CancellationToken cancellationToken)
    {
        var queries = await (from ovr in _context.OvertimeRates
                      join com in _context.Companies on ovr.CompanyKey equals com.Key
                      where ovr.DeletedAt == null
                      select new
                      {
                          OvertimeRate = ovr,
                          Company = com
                      }).ToListAsync();

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x.OvertimeRate)).ToList();
        }

        var overtimeRateDetails = await(from ord in _context.OvertimeRateDetails
                                        where ord.DeletedAt == null
                                        select new OvertimeRateDetail
                                        {
                                            Key = ord.Key,
                                            OvertimeRateKey = ord.OvertimeRateKey,
                                            Level = ord.Level,
                                            Hours = ord.Hours,
                                            Multiply = ord.Multiply
                                        }).ToListAsync();

        var overtimeRates = queries.Select(x => new OvertimeRate
        {
            Key = x.OvertimeRate.Key,
            CompanyKey = x.OvertimeRate.CompanyKey,
            GroupName = x.OvertimeRate.GroupName,
            BaseOnDay = x.OvertimeRate.BaseOnDay,
            MaxHour = x.OvertimeRate.MaxHour,
            Company = x.Company,
            OvertimeRateDetails = overtimeRateDetails.Where(ord => ord.OvertimeRateKey == x.OvertimeRate.Key).ToList()
        }).ToList();

        return overtimeRates;
    }
}
#endregion
#endregion