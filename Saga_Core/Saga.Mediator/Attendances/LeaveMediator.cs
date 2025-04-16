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

namespace Saga.Mediator.Attendances.LeaveMediator;

#region "Get List Leave"
#region "Query"
    public sealed record GetLeavesQuery(Expression<Func<Leave, bool>>[] wheres) : IRequest<IEnumerable<Leave>>;
#endregion
#region "Handler"
    public sealed class GetLeavesQueryHandler : IRequestHandler<GetLeavesQuery, IEnumerable<Leave>>
    {
        private readonly IDataContext _context;

        public GetLeavesQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Leave>> Handle(GetLeavesQuery request, CancellationToken cancellationToken)
        {
            var queries = from leave in _context.Leaves
                          join company in _context.Companies on leave.CompanyKey equals company.Key
                          where leave.DeletedAt == null
                          select new Leave
                          {
                              Key = leave.Key,
                              CompanyKey = leave.CompanyKey,
                              Code = leave.Code,
                              Name = leave.Name,
                              MaxDays = leave.MaxDays,
                              MinSubmission = leave.MinSubmission,
                              MaxSubmission = leave.MaxSubmission,
                              IsByWeekDay = leave.IsByWeekDay,
                              IsResidue = leave.IsResidue,
                              Company = company
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var leaves = await queries.ToListAsync();

            return leaves;
        }
    }
#endregion
#endregion

#region "Get List Leave With Pagination"
#region "Query"
    public sealed record GetLeavesPaginationQuery(PaginationConfig pagination, Expression<Func<Leave, bool>>[] wheres) : IRequest<PaginatedList<Leave>>;
#endregion
#region "Handler"
    public sealed class GetLeavesPaginationQueryHandler : IRequestHandler<GetLeavesPaginationQuery, PaginatedList<Leave>>
    {
        private readonly IDataContext _context;

        public GetLeavesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Leave>> Handle(GetLeavesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from leave in _context.Leaves
                          join company in _context.Companies on leave.CompanyKey equals company.Key
                          where leave.DeletedAt == null
                          select new Leave
                          {
                              Key = leave.Key,
                              CompanyKey = leave.CompanyKey,
                              Code = leave.Code,
                              Name = leave.Name,
                              MaxDays = leave.MaxDays,
                              MinSubmission = leave.MinSubmission,
                              MaxSubmission = leave.MaxSubmission,
                              IsByWeekDay = leave.IsByWeekDay,
                              IsResidue = leave.IsResidue,
                              Company = company
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var leaves = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            return await Task.FromResult(leaves);
        }
    }
#endregion
#endregion

#region "Get By Id Leave"
#region "Query"
    public sealed record GetLeaveQuery(Guid Key) : IRequest<Leave>;
#endregion
#region "Handler"
    public sealed class GetLeaveQueryHandler : IRequestHandler<GetLeaveQuery, Leave>
    {
        private readonly IDataContext _context;

        public GetLeaveQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Leave> Handle(GetLeaveQuery request, CancellationToken cancellationToken)
        {
            var leave = await (from lea in _context.Leaves
                               join com in _context.Companies on lea.CompanyKey equals com.Key
                               where lea.Key == request.Key
                               select new Leave
                               {
                                   Key = lea.Key,
                                   CompanyKey = lea.CompanyKey,
                                   Code = lea.Code,
                                   Name = lea.Name,
                                   MaxDays = lea.MaxDays,
                                   MinSubmission = lea.MinSubmission,
                                   MaxSubmission = lea.MaxSubmission,
                                   IsByWeekDay = lea.IsByWeekDay,
                                   IsResidue = lea.IsResidue,
                                   Description = lea.Description,
                                   Company = com
                               }).FirstOrDefaultAsync();

            if (leave == null)
                throw new Exception("Leave not found.");

            return leave;
        }
    }
#endregion
#endregion

#region "Save Leave"
#region "Command"
    public sealed record SaveLeaveCommand(LeaveDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveLeaveCommandHandler : IRequestHandler<SaveLeaveCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<LeaveDto> _validator;

        public SaveLeaveCommandHandler(IDataContext context, IValidator<LeaveDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveLeaveCommand command, CancellationToken cancellationToken)
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

                var leave = command.Form.ConvertToEntity();
                if (leave.Key == Guid.Empty)
                {
                    leave.Key = Guid.NewGuid();
                }

                //Check if leave exists
                var existingLeave = await _context.Leaves.FirstOrDefaultAsync(x => x.Key == leave.Key && x.DeletedAt == null, cancellationToken);
                if (existingLeave == null)
                {
                    //Add new Leave
                    _context.Leaves.Add(leave);
                }
                else
                {
                    //Update existing Leave
                    leave.CreatedAt = existingLeave.CreatedAt;
                    leave.CreatedBy = existingLeave.CreatedBy;
                    _context.Leaves.Entry(existingLeave).CurrentValues.SetValues(leave);
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

#region "Delete Leave"
#region "Command"
    public sealed record DeleteLeaveCommand(Guid Key) : IRequest<Result<Leave>>;
#endregion
#region "Handler"
    public sealed class DeleteLeaveCommandHandler : IRequestHandler<DeleteLeaveCommand, Result<Leave>>
    {
        private readonly IDataContext _context;

        public DeleteLeaveCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<Leave>> Handle(DeleteLeaveCommand command, CancellationToken cancellationToken)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(x => x.Key == command.Key);

            try
            {
                if (leave == null)
                    throw new Exception("Leave not found.");

                _context.Leaves.Remove(leave);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Leave>.Failure(new[] { ex.Message });
            }

            return Result<Leave>.Success(leave);
        }
    }
#endregion
#endregion
