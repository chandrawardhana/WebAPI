using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.Persistence.Context;
using Saga.Validators.Attendances;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.ShiftScheduleMediator;

#region "Get List Shift Schedule"
#region "Query"
    public sealed record GetShiftSchedulesQuery(Expression<Func<ShiftSchedule, bool>>[] wheres) : IRequest<ShiftScheduleList>;
#endregion
#region "Handler"
    public sealed class GetShiftSchedulesQueryHandler : IRequestHandler<GetShiftSchedulesQuery, ShiftScheduleList>
    {
        private readonly IDataContext _context;

        public GetShiftSchedulesQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<ShiftScheduleList> Handle(GetShiftSchedulesQuery request, CancellationToken cancellationToken)
        {
            var queries = from shf in _context.ShiftSchedules
                          join com in _context.Companies on shf.CompanyKey equals com.Key
                          where shf.DeletedAt == null
                          select new ShiftSchedule
                          {
                              Key = shf.Key,
                              CompanyKey = shf.CompanyKey,
                              GroupName = shf.GroupName,
                              YearPeriod = shf.YearPeriod,
                              MonthPeriod = shf.MonthPeriod,
                              IsRoaster = shf.IsRoaster,
                              Company = com
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var shiftSchedules = await queries.ToListAsync();

            var viewModel = new ShiftScheduleList
            {
                ShiftSchedules = shiftSchedules.Select(sh => sh.ConvertToViewModelShiftScheduleItemList())
            };

            return viewModel;
        }
    }
#endregion
#endregion

#region "Get Shift Schedule By Id"
#region "Query"
    public sealed record GetShiftScheduleQuery(Guid Key) : IRequest<ShiftScheduleForm>;
#endregion
#region "Handler"
    public sealed class GetShiftScheduleQueryHandler : IRequestHandler<GetShiftScheduleQuery, ShiftScheduleForm>
    {
        private readonly IDataContext _context;

        public GetShiftScheduleQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<ShiftScheduleForm> Handle(GetShiftScheduleQuery request, CancellationToken cancellationToken)
        {
            var shiftSchedule = await (from shf in _context.ShiftSchedules
                                       join com in _context.Companies on shf.CompanyKey equals com.Key
                                       where shf.Key == request.Key
                                       select new ShiftSchedule
                                       {
                                           Key = shf.Key,
                                           CompanyKey = shf.CompanyKey,
                                           GroupName = shf.GroupName,
                                           YearPeriod = shf.YearPeriod,
                                           MonthPeriod = shf.MonthPeriod,
                                           IsRoaster = shf.IsRoaster,
                                           Company = com
                                       }).FirstOrDefaultAsync();

            if (shiftSchedule == null)
                throw new Exception("Shift Schedule Not Found");

            var shiftScheduleDetails = _context.ShiftScheduleDetails.Where(x => x.ShiftScheduleKey == shiftSchedule.Key);
            /*
            var shiftScheduleDetails = await (from ssd in _context.ShiftScheduleDetails
                                              join sd in _context.ShiftDetails on ssd.ShiftDetailKey equals sd.Key
                                              where ssd.ShiftScheduleKey == shiftSchedule.Key && ssd.DeletedAt == null
                                              select new ShiftScheduleDetail
                                              {
                                                  Key = ssd.Key,
                                                  ShiftScheduleKey = ssd.ShiftScheduleKey,
                                                  ShiftDetailKey = ssd.ShiftDetailKey,
                                                  Date = ssd.Date,
                                                  ShiftDetail = sd
                                              }).ToListAsync();
            */
            var shiftScheduleDetailsForm = shiftScheduleDetails.Select(ssd => ssd.ConvertToViewModelShiftScheduleDetailForm());

            shiftSchedule.ShiftScheduleDetails = shiftScheduleDetails;

            var viewModel = shiftSchedule.ConvertToViewModelShiftScheduleForm();
            viewModel.JsonShiftScheduleDetails = JsonConvert.SerializeObject(shiftScheduleDetailsForm, Formatting.None);
        
            return viewModel;
        }
    }
#endregion
#endregion

#region "Save Shift Schedule"
#region "Command"
    public sealed record SaveShiftScheduleCommand(ShiftScheduleDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveShiftScheduleCommandHandler : IRequestHandler<SaveShiftScheduleCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<ShiftScheduleDto> _shiftScheduleValidator;
        private readonly IValidator<ShiftScheduleDetailDto> _shiftScheduleDetailValidator;

        public SaveShiftScheduleCommandHandler(IDataContext context, IValidator<ShiftScheduleDto> shiftScheduleValidator, IValidator<ShiftScheduleDetailDto> shiftScheduleDetailValidator)
        {
            _context = context;
            _shiftScheduleValidator = shiftScheduleValidator;
            _shiftScheduleDetailValidator = shiftScheduleDetailValidator;
        }

        public async Task<Result> Handle(SaveShiftScheduleCommand command, CancellationToken cancellationToken)
        {
            try
            {
                //Validate ShiftSchedule
                ValidationResult shiftScheduleValidator = await _shiftScheduleValidator.ValidateAsync(command.Form);
                if (!shiftScheduleValidator.IsValid)
                {
                    var failures = shiftScheduleValidator.Errors
                                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                            .ToList();
                    return Result.Failure(failures);
                }

                var shiftSchedule = command.Form.ConvertToEntity();

                if (shiftSchedule.Key == Guid.Empty)
                {
                    shiftSchedule.Key = Guid.NewGuid();
                }

                //Check if shift schedule exist
                var existingShiftSchedule = await _context.ShiftSchedules.FirstOrDefaultAsync(x => x.Key == shiftSchedule.Key && x.DeletedAt == null, cancellationToken);
                if (existingShiftSchedule == null)
                {
                    //Add new Shift Schedule
                    _context.ShiftSchedules.Add(shiftSchedule);
                }
                else
                {
                    //Update existing Shift Schedule
                    shiftSchedule.CreatedAt = existingShiftSchedule.CreatedAt;
                    shiftSchedule.CreatedBy = existingShiftSchedule.CreatedBy;
                    _context.ShiftSchedules.Entry(existingShiftSchedule).CurrentValues.SetValues(shiftSchedule);
                }

                //Save Shift Schedule to get the key
                await _context.SaveChangesAsync(cancellationToken);

                if (command.Form.ShiftScheduleDetails != null && command.Form.ShiftScheduleDetails.Any())
                {
                    foreach (var shiftScheduleDetailDto in command.Form.ShiftScheduleDetails)
                    {
                        shiftScheduleDetailDto.ShiftScheduleKey = shiftSchedule.Key;
                        ValidationResult shiftScheduleDetailValidator = await _shiftScheduleDetailValidator.ValidateAsync(shiftScheduleDetailDto);
                        if (!shiftScheduleDetailValidator.IsValid)
                            return Result.Failure(shiftScheduleDetailValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    _ = await _context.ShiftScheduleDetails
                                      .Where(x => x.ShiftScheduleKey == shiftSchedule.Key)
                                      .ExecuteDeleteAsync();

                    var shiftScheduleDetailEntities = command.Form.ShiftScheduleDetails.Select(x =>
                    {
                        var entity = x.ConvertToEntity();
                        entity.Key = Guid.NewGuid();
                        entity.ShiftScheduleKey = shiftSchedule.Key;
                        return entity;
                    }).ToList();

                    await _context.ShiftScheduleDetails.AddRangeAsync(shiftScheduleDetailEntities);
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

#region "Delete Shift Schedule"
#region "Command"
    public sealed record DeleteShiftScheduleCommand(Guid Key) : IRequest<Result<ShiftSchedule>>;
#endregion
#region "Handler"
    public sealed class DeleteShiftScheduleCommandHandler : IRequestHandler<DeleteShiftScheduleCommand, Result<ShiftSchedule>>
    {
        private readonly IDataContext _context;

        public DeleteShiftScheduleCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<ShiftSchedule>> Handle(DeleteShiftScheduleCommand command, CancellationToken cancellationToken)
        {
            var shiftSchedule = await _context.ShiftSchedules.FirstOrDefaultAsync(x => x.Key == command.Key);

            try
            {
                if (shiftSchedule == null)
                    throw new Exception("Shift Schedule not found.");

                //Check if any ShiftScheduleDetail exist for this ShiftSchedule
                var shiftScheduleDetails = await _context.ShiftScheduleDetails.Where(sd => sd.ShiftScheduleKey == shiftSchedule.Key).ToListAsync();
                if (shiftScheduleDetails.Any())
                {
                    _context.ShiftScheduleDetails.RemoveRange(shiftScheduleDetails);
                }

                _context.ShiftSchedules.Remove(shiftSchedule);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<ShiftSchedule>.Failure(new[] { ex.Message });
            }

            return Result<ShiftSchedule>.Success(shiftSchedule);
        }
    }
#endregion
#endregion

#region "Get Employee Shift"
#region "Query"
public sealed record GetEmployeeShiftQuery(Guid EmployeeKey, DateOnly date) : IRequest<ShiftScheduleDetail>;
#endregion
#region "Handler"
public sealed class GetEmployeeShiftQueryHandler : IRequestHandler<GetEmployeeShiftQuery, ShiftScheduleDetail>
{
    private readonly IDataContext _context;

    public GetEmployeeShiftQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<ShiftScheduleDetail> Handle(GetEmployeeShiftQuery request, CancellationToken cancellationToken)
    {
        var shiftScheduleDetail = await (from e in _context.Employees
                                 join ea in _context.EmployeesAttendances on e.Key equals ea.EmployeeKey
                                 join sh in _context.Shifts on ea.ShiftKey equals sh.Key
                                 join shd in _context.ShiftDetails on sh.Key equals shd.Key
                                 join ss in _context.ShiftSchedules on ea.ShiftScheduleKey equals ss.Key
                                 join ssd in _context.ShiftScheduleDetails on ss.Key equals ssd.ShiftScheduleKey
                                 where e.Key == request.EmployeeKey && ssd.Date == request.date
                                 select new ShiftScheduleDetail
                                 {
                                     Key = ssd.Key,
                                     Date = ssd.Date,
                                     ShiftName = ssd.ShiftName,
                                     ShiftSchedule = ss,
                                     ShiftDetail = shd
                                 }).FirstOrDefaultAsync();

        if (shiftScheduleDetail == null)
            throw new Exception("Shift Not Found");

        return shiftScheduleDetail;
    }
}
#endregion
#endregion

#region "Get List Shift Schedule With Details"
#region "Query"
public sealed record GetShiftScheduleWithDetailsQuery(Expression<Func<ShiftSchedule, bool>>[] wheres) : IRequest<IEnumerable<ShiftSchedule>>;
#endregion
#region "Handler"
public sealed class GetShiftScheduleWithDetailsQueryHandler : IRequestHandler<GetShiftScheduleWithDetailsQuery, IEnumerable<ShiftSchedule>>
{
    private readonly IDataContext _context;

    public GetShiftScheduleWithDetailsQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ShiftSchedule>> Handle(GetShiftScheduleWithDetailsQuery request, CancellationToken cancellationToken)
    {
        var queries = from shf in _context.ShiftSchedules
                      join com in _context.Companies on shf.CompanyKey equals com.Key
                      where shf.DeletedAt == null
                      select new 
                      {
                          ShiftSchedule = shf,
                          Company = com
                      };

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x.ShiftSchedule));
        }

        var shiftScheduleDetails = await(from ssd in _context.ShiftScheduleDetails
                                         join sd in _context.ShiftDetails on ssd.ShiftDetailKey equals sd.Key
                                         where ssd.DeletedAt == null
                                         select new ShiftScheduleDetail
                                         {
                                             Key = ssd.Key,
                                             ShiftScheduleKey = ssd.ShiftScheduleKey,
                                             ShiftDetailKey = ssd.ShiftDetailKey,
                                             Date = ssd.Date,
                                             ShiftDetail = sd
                                         }).ToListAsync();

        var shiftSchedules = await queries.Select(x => new ShiftSchedule
        {
            Key = x.ShiftSchedule.Key,
            CompanyKey = x.ShiftSchedule.CompanyKey,
            GroupName = x.ShiftSchedule.GroupName,
            YearPeriod = x.ShiftSchedule.YearPeriod,
            MonthPeriod = x.ShiftSchedule.MonthPeriod,
            IsRoaster = x.ShiftSchedule.IsRoaster,
            Company = x.Company,
            ShiftScheduleDetails = shiftScheduleDetails.Where(ssd => ssd.ShiftScheduleKey == x.ShiftSchedule.Key).ToList()
        }).ToListAsync();

        return shiftSchedules;
    }
}
#endregion
#endregion
