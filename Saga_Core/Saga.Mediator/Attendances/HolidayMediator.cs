using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;
using Saga.Domain.ViewModels.Organizations;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.HolidayMediator;

#region "Get List Holiday"
#region "Query"
    public sealed record GetHolidaysQuery(Expression<Func<Holiday, bool>>[] wheres) : IRequest<List<Holiday>>;
#endregion
#region "Handler"
    public sealed class GetHolidaysQueryHandler : IRequestHandler<GetHolidaysQuery, List<Holiday>>
    {
        private readonly IDataContext _context;

        public GetHolidaysQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<List<Holiday>> Handle(GetHolidaysQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Holidays.Where(holiday => holiday.DeletedAt == null);

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(filter);
            }

            var holidays = await queries.ToListAsync(cancellationToken);

            //var companyKeys = holidays.SelectMany(h => h.CompanyKeys).Distinct().ToList();

            //var companies = await _context.Companies.Where(c => companyKeys.Contains(c.Key)).ToListAsync();

            return holidays;
        }
    }
#endregion
#endregion

#region "Get List Holiday With Pagination"
#region "Query"
    public sealed record GetHolidaysPaginationQuery(PaginationConfig pagination, Expression<Func<Holiday, bool>>[] wheres) : IRequest<PaginatedList<HolidayListItem>>;
#endregion
#region "Handler"
    public sealed class GetHolidaysPaginationQueryHandler : IRequestHandler<GetHolidaysPaginationQuery, PaginatedList<HolidayListItem>>
    {
        private readonly IDataContext _context;

        public GetHolidaysPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<HolidayListItem>> Handle(GetHolidaysPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from holiday in _context.Holidays
                          where holiday.DeletedAt == null
                          select holiday;

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Name, $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var holidays = await queries.Select(x => new HolidayListItem
            {
                Key = x.Key,
                Name = x.Name,
                Duration = x.Duration,
                Description = x.Description,
                DateEvent = x.DateEvent,
                CompanyKeys = x.CompanyKeys
            }).PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            var companyKeys = holidays.Items.SelectMany(h => h.CompanyKeys).Distinct().ToList();

            var companies = await _context.Companies.Where(c => companyKeys.Contains(c.Key))
            .Select(c => new CompanyForm
            {
                Key = c.Key,
                Name = c.Name
            }).ToListAsync();

            foreach (var holiday in holidays.Items)
            {
                holiday.Companies = companies
                    .Where(c => holiday.CompanyKeys.Contains(c.Key))
                    .ToList();
            }

            return holidays;
        }
    }
#endregion
#endregion

#region "Get By Id Holiday"
#region "Query"
    public sealed record GetHolidayQuery(Guid Key) : IRequest<HolidayForm>;
#endregion
#region "Handler"
    public sealed class GetHolidayQueryHandler : IRequestHandler<GetHolidayQuery, HolidayForm>
    {
        private readonly IDataContext _context;

        public GetHolidayQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<HolidayForm> Handle(GetHolidayQuery request, CancellationToken cancellationToken)
        {
            var holiday = await _context.Holidays.FirstOrDefaultAsync(hdy => hdy.Key == request.Key);

            if (holiday == null)
                throw new Exception("Holiday Not Found");

            var companyKeys = holiday.CompanyKeys;
            var companies = await _context.Companies.Where(c => companyKeys.Contains(c.Key)).ToListAsync();

            return holiday.ConvertToViewModelHolidayForm(companies);
        }
    }
#endregion
#endregion

#region "Save Holiday"
#region "Command"
    public sealed record SaveHolidayCommand(HolidayDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveHolidayCommandHandler : IRequestHandler<SaveHolidayCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<HolidayDto> _validator;

        public SaveHolidayCommandHandler(IDataContext context, IValidator<HolidayDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveHolidayCommand command, CancellationToken cancellationToken)
        {
            try
            {
                ValidationResult validator = await _validator.ValidateAsync(command.Form);
                if (!validator.IsValid)
                {
                    var failures = validator.Errors
                                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                            .ToList();
                    return Result.Failure(failures);
                }

                var holiday = command.Form.ConvertToEntity();
                if (holiday.Key == Guid.Empty)
                {
                    holiday.Key = Guid.NewGuid();
                }

                //Check if holiday exists
                var existingHoliday = await _context.Holidays.FirstOrDefaultAsync(x => x.Key == holiday.Key && x.DeletedAt == null, cancellationToken);

                if (existingHoliday == null)
                {
                    //Add new Holiday
                    _context.Holidays.Add(holiday);
                }
                else
                {
                    //Update existing Holiday
                    holiday.CreatedAt = existingHoliday.CreatedAt;
                    holiday.CreatedBy = existingHoliday.CreatedBy;
                    _context.Holidays.Entry(existingHoliday).CurrentValues.SetValues(holiday);
                }

                var result = await _context.SaveChangesAsync(cancellationToken);
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

#region "Delete Holiday"
#region "Command"
    public sealed record DeleteHolidayCommand(Guid Key) : IRequest<Result<Holiday>>;
#endregion
#region "Handler"
    public sealed class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand, Result<Holiday>>
    {
        private readonly IDataContext _context;

        public DeleteHolidayCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<Holiday>> Handle(DeleteHolidayCommand command, CancellationToken cancellationToken)
        {
            var holiday = await _context.Holidays.FirstOrDefaultAsync(x => x.Key == command.Key);

            try
            {
                if (holiday == null)
                    throw new Exception("Holiday Not Found");

                _context.Holidays.Remove(holiday);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Holiday>.Failure(new[] { ex.Message });
            }
            return Result<Holiday>.Success(holiday);
        }
    }
#endregion
#endregion
