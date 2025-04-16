using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.StandardWorkingMediator;

#region "Get List Standard Working"
#region "Query"
public sealed record GetStandardWorkingsQuery(Expression<Func<StandardWorking, bool>>[] wheres) : IRequest<StandardWorkingList>;
#endregion
#region "Handler"
    public sealed class GetStandardWorkingsQueryHandler : IRequestHandler<GetStandardWorkingsQuery, StandardWorkingList>
    {
        private readonly IDataContext _context;

        public GetStandardWorkingsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<StandardWorkingList> Handle(GetStandardWorkingsQuery request, CancellationToken cancellationToken)
        {
            var queries = from sw in _context.StandardWorkings
                          join com in _context.Companies on sw.CompanyKey equals com.Key
                          where sw.DeletedAt == null
                          select new StandardWorking
                          {
                              Key = sw.Key,
                              CompanyKey = sw.CompanyKey,
                              YearPeriod = sw.YearPeriod,
                              January = sw.January,
                              February = sw.February,
                              March = sw.March,
                              April = sw.April,
                              May = sw.May,
                              June = sw.June,
                              July = sw.July,
                              August = sw.August,
                              September = sw.September,
                              October = sw.October,
                              November = sw.November,
                              December = sw.December,
                              Company = com,
                              Description = sw.Description,
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var standardWorkings = await queries.ToListAsync();

            var viewModel = new StandardWorkingList
            {
                StandardWorkings = standardWorkings.Select(sw => sw.ConvertToViewModelStandardWorking())
            };

            return viewModel;
        }
    }
#endregion
#endregion

#region "Get List Standard Working With Pagination"
#region "Query"
    public sealed record GetStandardWorkingsPaginationQuery(PaginationConfig pagination, Expression<Func<StandardWorking, bool>>[] wheres) : IRequest<PaginatedList<StandardWorking>>;
#endregion
#region "Handler"
    public sealed class GetStandardWorkingsPaginationQueryHandler : IRequestHandler<GetStandardWorkingsPaginationQuery, PaginatedList<StandardWorking>>
    {
        private readonly IDataContext _context;

        public GetStandardWorkingsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<StandardWorking>> Handle(GetStandardWorkingsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from sw in _context.StandardWorkings
                          join com in _context.Companies on sw.CompanyKey equals com.Key
                          where sw.DeletedAt == null
                          select new StandardWorking
                          {
                              Key = sw.Key,
                              CompanyKey = sw.CompanyKey,
                              YearPeriod = sw.YearPeriod,
                              January = sw.January,
                              February = sw.February,
                              March = sw.March,
                              April = sw.April,
                              May = sw.May,
                              June = sw.June,
                              July = sw.July,
                              August = sw.August,
                              September = sw.September,
                              October = sw.October,
                              November = sw.November,
                              December = sw.December,
                              Company = com,
                              Description = sw.Description
                          };
            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.YearPeriod.ToString(), $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var standardWorkings = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            return await Task.FromResult(standardWorkings);
        }
    }
#endregion
#endregion

#region "Get Standard Working"
#region "Query"
    public sealed record GetStandardWorkingQuery(Guid Key) : IRequest<StandardWorkingForm>;
#endregion
#region "Handler"
    public sealed class GetStandardWorkingQueryHandler : IRequestHandler<GetStandardWorkingQuery, StandardWorkingForm>
    {
        private readonly IDataContext _context;

        public GetStandardWorkingQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<StandardWorkingForm> Handle(GetStandardWorkingQuery request, CancellationToken cancellationToken)
        {
            var standardWorking = await (from sw in _context.StandardWorkings
                                         join com in _context.Companies on sw.CompanyKey equals com.Key
                                         where sw.Key == request.Key
                                         select new StandardWorking
                                         {
                                             Key = sw.Key,
                                             CompanyKey = sw.CompanyKey,
                                             YearPeriod = sw.YearPeriod,
                                             January = sw.January,
                                             February = sw.February,
                                             March = sw.March,
                                             April = sw.April,
                                             May = sw.May,
                                             June = sw.June,
                                             July = sw.July,
                                             August = sw.August,
                                             September = sw.September,
                                             October = sw.October,
                                             November = sw.November,
                                             December = sw.December,
                                             Description = sw.Description,
                                             Company = com
                                         }).FirstOrDefaultAsync();

            if (standardWorking == null)
                throw new Exception("Standard Working not found.");

            return standardWorking.ConvertToViewModelStandardWorking();
        }
    }
#endregion
#endregion

#region "Save Standard Working"
#region "Command"
    public sealed record SaveStandardWorkingCommand(StandardWorkingDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveStandardWorkingCommandHandler : IRequestHandler<SaveStandardWorkingCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<StandardWorkingDto> _validator;

        public SaveStandardWorkingCommandHandler(IDataContext context, IValidator<StandardWorkingDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveStandardWorkingCommand command, CancellationToken cancellationToken)
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

                var standardWorking = command.Form.ConvertToEntity();
                if (standardWorking.Key == Guid.Empty)
                {
                    standardWorking.Key = Guid.NewGuid();
                }

                //Check if Standard Working Exists
                var existingStandardWorking = await _context.StandardWorkings.FirstOrDefaultAsync(x => x.Key == standardWorking.Key && x.DeletedAt == null, cancellationToken);

                if (existingStandardWorking == null)
                {
                    //Add new Standard Working
                    _context.StandardWorkings.Add(standardWorking);
                }
                else
                {
                    //Update existing Standard Working
                    standardWorking.CreatedAt = existingStandardWorking.CreatedAt;
                    standardWorking.CreatedBy = existingStandardWorking.CreatedBy;
                    _context.StandardWorkings.Entry(existingStandardWorking).CurrentValues.SetValues(standardWorking);
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

#region "Delete Standard Working"
#region "Command"
public sealed record DeleteStandardWorkingCommand(Guid Key) : IRequest<Result<StandardWorking>>;
#endregion
#region "Handler"
public sealed class DeleteStandardWorkingCommandHandler : IRequestHandler<DeleteStandardWorkingCommand, Result<StandardWorking>>
{
    private readonly IDataContext _context;

    public DeleteStandardWorkingCommandHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<Result<StandardWorking>> Handle(DeleteStandardWorkingCommand command, CancellationToken cancellationToken)
    {
        var standardWorking = await _context.StandardWorkings.FirstOrDefaultAsync(x => x.Key == command.Key);

        try
        {
            if (standardWorking == null)
                throw new Exception("Standard Working not found.");

            _context.StandardWorkings.Remove(standardWorking);

            var result = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<StandardWorking>.Failure(new[] { ex.Message });
        }

        return Result<StandardWorking>.Success(standardWorking);
    }
}
#endregion
#endregion
