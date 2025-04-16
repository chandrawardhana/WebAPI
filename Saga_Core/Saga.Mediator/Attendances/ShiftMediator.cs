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

namespace Saga.Mediator.Attendances.ShiftMediator;

#region "Get List Shift"
#region "Query"
    public sealed record GetShiftsQuery(Expression<Func<Shift, bool>>[] wheres) : IRequest<ShiftList>;
#endregion
#region "Handler"
    public sealed class GetShiftsQueryHandler(IDataContext _context) : IRequestHandler<GetShiftsQuery, ShiftList>
    {
        public async Task<ShiftList> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
        {
            var queries = from sh in _context.Shifts
                          join com in _context.Companies on sh.CompanyKey equals com.Key
                          where sh.DeletedAt == null
                          select new Shift
                          {
                              Key = sh.Key,
                              CompanyKey = sh.CompanyKey,
                              ShiftGroupName = sh.ShiftGroupName,
                              Company = com
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var shifts = await queries.ToListAsync();

            var viewModel = new ShiftList
            {
                Shifts = shifts.Select(sh => sh.ConvertToViewModelShiftItemList())
            };

            return viewModel;
        }
    }
#endregion
#endregion

#region "Get List Shift With Pagination"
#region "Query"
    public sealed record GetShiftsPaginationQuery(PaginationConfig pagination, Expression<Func<Shift, bool>>[] wheres) : IRequest<PaginatedList<Shift>>;
#endregion
#region "Handler"
    public sealed class GetShiftsPaginationQueryHandler : IRequestHandler<GetShiftsPaginationQuery, PaginatedList<Shift>>
    {
        private readonly IDataContext _context;

        public GetShiftsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Shift>> Handle(GetShiftsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from sh in _context.Shifts
                          join com in _context.Companies on sh.CompanyKey equals com.Key
                          where sh.DeletedAt == null
                          select new Shift
                          {
                              Key = sh.Key,
                              CompanyKey = sh.CompanyKey,
                              ShiftGroupName = sh.ShiftGroupName,
                              Company = com
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.ShiftGroupName, $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var shifts = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
        
            return await Task.FromResult(shifts);
        }
    }
#endregion
#endregion

#region "Get Shift By Id"
#region "Query"
    public sealed record GetShiftQuery(Guid Key) : IRequest<ShiftForm>;
#endregion
#region "Handler"
    public sealed class GetShiftQueryHandler : IRequestHandler<GetShiftQuery, ShiftForm>
    {
        private readonly IDataContext _context;

        public GetShiftQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<ShiftForm> Handle(GetShiftQuery request, CancellationToken cancellationToken)
        {
            var shift = await (from sh in _context.Shifts
                               join com in _context.Companies on sh.CompanyKey equals com.Key
                               where sh.Key == request.Key
                               select new Shift
                               {
                                   Key = sh.Key,
                                   CompanyKey = sh.CompanyKey,
                                   ShiftGroupName = sh.ShiftGroupName,
                                   MaxLimit = sh.MaxLimit,
                                   Description = sh.Description,
                                   Company = com
                               }).FirstOrDefaultAsync();

            if (shift == null)
                throw new Exception("Shift Not Found");

            var shiftDetails = await (from sd in _context.ShiftDetails
                                      where sd.ShiftKey == request.Key && sd.DeletedAt == null
                                      select new ShiftDetail
                                      {
                                          Key = sd.Key,
                                          ShiftKey = sd.ShiftKey,
                                          Day = sd.Day,
                                          WorkName = sd.WorkName,
                                          WorkType = sd.WorkType,
                                          In = sd.In,
                                          Out = sd.Out,
                                          EarlyIn = sd.EarlyIn,
                                          MaxOut = sd.MaxOut,
                                          LateTolerance = sd.LateTolerance,
                                          IsCutBreak = sd.IsCutBreak,
                                          IsNextDay = sd.IsNextDay
                                      }).ToListAsync();

            var shiftDetailsForm = shiftDetails.Select(sd => sd.ConvertToViewModelShiftDetailForm());

            shift.ShiftDetails = shiftDetails;

            var viewModel = shift.ConvertToViewModelShiftForm();
            viewModel.JsonShiftDetails = JsonConvert.SerializeObject(shiftDetailsForm, Formatting.None);

            return viewModel;
        }
    }
#endregion
#endregion

#region "Save Shift"
#region "Command"
    public sealed record SaveShiftCommand(ShiftDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveShiftCommandHandler : IRequestHandler<SaveShiftCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<ShiftDto> _ShiftValidator;
        private readonly IValidator<ShiftDetailDto> _ShiftDetailValidator;

        public SaveShiftCommandHandler(IDataContext context, IValidator<ShiftDto> ShiftValidator, IValidator<ShiftDetailDto> ShiftDetailValidator)
        {
            _context = context;
            _ShiftValidator = ShiftValidator;
            _ShiftDetailValidator = ShiftDetailValidator;
        }

        public async Task<Result> Handle(SaveShiftCommand command, CancellationToken cancellationToken)
        {
            try
            {
                //Validate Shift
                ValidationResult shiftValidator = await _ShiftValidator.ValidateAsync(command.Form);
                if (!shiftValidator.IsValid)
                {
                    var failures = shiftValidator.Errors
                                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                            .ToList();
                    return Result.Failure(failures);
                }

                var shift = command.Form.ConvertToEntity();

                if (shift.Key == Guid.Empty)
                {
                    shift.Key = Guid.NewGuid();
                }

                // Check if shift exists
                var existingShift = await _context.Shifts.FirstOrDefaultAsync(s => s.Key == shift.Key, cancellationToken);

                if (existingShift == null)
                {
                    // Add new shift
                    _context.Shifts.Add(shift);
                }
                else
                {
                    // Update existing shift
                    shift.CreatedAt = existingShift.CreatedAt;
                    shift.CreatedBy = existingShift.CreatedBy;
                    _context.Shifts.Entry(existingShift).CurrentValues.SetValues(shift);
                }

                // Save shift to get the Key
                await _context.SaveChangesAsync(cancellationToken);

                if (command.Form.ShiftDetails != null && command.Form.ShiftDetails.Any())
                {
                    foreach (var shiftDetailDto in command.Form.ShiftDetails)
                    {
                        shiftDetailDto.ShiftKey = shift.Key;
                        ValidationResult shiftDetailValidator = await _ShiftDetailValidator.ValidateAsync(shiftDetailDto);
                        if (!shiftDetailValidator.IsValid)
                            return Result.Failure(shiftDetailValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    _ = await _context.ShiftDetails
                                      .Where(x => x.ShiftKey == shift.Key)
                                      .ExecuteDeleteAsync();

                    var shiftDetailEntities = command.Form.ShiftDetails.Select(sd =>
                    {
                        var entity = sd.ConvertToEntity();
                        entity.Key = Guid.NewGuid();
                        entity.ShiftKey = shift.Key;
                        return entity;
                    });

                    await _context.ShiftDetails.AddRangeAsync(shiftDetailEntities, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                }
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { $"Error saving shift: {ex.Message}" });
            }
            return Result.Success();
        }
    }
#endregion
#endregion

#region "Delete Shift"
#region "Command"
    public sealed record DeleteShiftCommand(Guid Key) : IRequest<Result<Shift>>;
#endregion
#region "Handler"
    public sealed class DeleteShiftCommandHandler : IRequestHandler<DeleteShiftCommand, Result<Shift>>
    {
        private readonly IDataContext _context;

        public DeleteShiftCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<Shift>> Handle(DeleteShiftCommand command, CancellationToken cancellationToken)
        {
            var shift = await _context.Shifts.FirstOrDefaultAsync(x => x.Key == command.Key);

            try
            {
                if (shift == null)
                    throw new Exception("Shift not found.");

                //Check if any ShiftDetails exist for this Shift
                var shiftDetails = await _context.ShiftDetails.Where(sd => sd.ShiftKey == shift.Key).ToListAsync();
                if (shiftDetails.Any())
                {
                    _context.ShiftDetails.RemoveRange(shiftDetails);
                }

                _context.Shifts.Remove(shift);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Shift>.Failure(new[] { ex.Message });
            }
            return Result<Shift>.Success(shift);
        }
    }
#endregion
#endregion

#region "Get List ShiftName for ShiftSchedule"
#region "Query"
    public sealed record GetShiftNamesQuery(Expression<Func<ShiftDetail, bool>>[] wheres, Guid? CompanyKey) : IRequest<ShiftWorkNameList>;
#endregion
#region "Handler"
    public sealed class GetShiftNamesQueryHandler : IRequestHandler<GetShiftNamesQuery, ShiftWorkNameList>
    {
        private readonly IDataContext _context;

        public GetShiftNamesQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<ShiftWorkNameList> Handle(GetShiftNamesQuery request, CancellationToken cancellationToken)
        {
            var queries = from sd in _context.ShiftDetails
                          join sh in _context.Shifts on sd.ShiftKey equals sh.Key
                          join com in _context.Companies on sh.CompanyKey equals com.Key
                          where sd.DeletedAt == null && sh.CompanyKey == request.CompanyKey
                          select new ShiftDetail
                          {
                              Key = sd.Key,
                              Day = sd.Day,
                              WorkName = sd.WorkName,
                              WorkType = sd.WorkType,
                              In = sd.In,
                              Out = sd.Out,
                              EarlyIn = sd.EarlyIn,
                              MaxOut = sd.MaxOut,
                              LateTolerance = sd.LateTolerance,
                              IsCutBreak = sd.IsCutBreak,
                              IsNextDay = sd.IsNextDay
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var shiftDetails = await queries.ToListAsync();
            var viewModel = new ShiftWorkNameList
            {
                ShiftWorkNames = shiftDetails.Select(x => x.ConvertToViewModelShiftWorkNameItemList())
            };

            return viewModel;
        }
    }
#endregion
#endregion

#region "Get List Shift With Details"
#region "Query"
public sealed record GetShiftWithDetailsQuery(Expression<Func<Shift, bool>>[] wheres) : IRequest<IEnumerable<Shift>>;
#endregion
#region "Handler"
public sealed class GetShiftWithDetailsQueryHandler : IRequestHandler<GetShiftWithDetailsQuery, IEnumerable<Shift>>
{
    private readonly IDataContext _context;

    public GetShiftWithDetailsQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shift>> Handle(GetShiftWithDetailsQuery request, CancellationToken cancellationToken)
    {
        var queries = from sh in _context.Shifts
                      join com in _context.Companies on sh.CompanyKey equals com.Key
                      where sh.DeletedAt == null
                      select new 
                      {
                          Shift = sh,
                          Company = com
                      };

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x.Shift));
        }

        var shiftDetails = await (from sd in _context.ShiftDetails
                                         where sd.DeletedAt == null
                                         select new ShiftDetail
                                         {
                                             Key = sd.Key,
                                             ShiftKey = sd.ShiftKey,
                                             Day = sd.Day,
                                             WorkName = sd.WorkName,
                                             WorkType = sd.WorkType,
                                             In = sd.In,
                                             Out = sd.Out,
                                             EarlyIn = sd.EarlyIn,
                                             MaxOut = sd.MaxOut,
                                             LateTolerance = sd.LateTolerance,
                                             IsCutBreak = sd.IsCutBreak,
                                             IsNextDay = sd.IsNextDay
                                         }).ToListAsync();

        var shifts = await queries.Select(x => new Shift
        {
            Key = x.Shift.Key,
            CompanyKey = x.Shift.CompanyKey,
            ShiftGroupName = x.Shift.ShiftGroupName,
            MaxLimit = x.Shift.MaxLimit,
            Description = x.Shift.Description,
            Company = x.Company,
            ShiftDetails = shiftDetails.Where(sd => sd.ShiftKey == x.Shift.Key).ToList()
        }).ToListAsync();

        return shifts;
    }
}
#endregion
#endregion