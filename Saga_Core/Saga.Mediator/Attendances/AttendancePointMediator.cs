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

namespace Saga.Mediator.Attendances.AttendancePointMediator;

#region "Get List Attendance Point"
#region "Query"
    public sealed record GetAttendancePointsQuery(Expression<Func<AttendancePoint, bool>>[] wheres) : IRequest<AttendancePointList>;
#endregion
#region "Handler"
    public sealed class GetAttendancePointsQueryHandler : IRequestHandler<GetAttendancePointsQuery, AttendancePointList>
    {
        private readonly IDataContext _context;

        public GetAttendancePointsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<AttendancePointList> Handle(GetAttendancePointsQuery request, CancellationToken cancellationToken)
        {
            var queries = from atp in _context.AttendancePoints
                          join com in _context.Companies on atp.CompanyKey equals com.Key
                          where atp.DeletedAt == null
                          select new AttendancePoint
                          {
                              Key = atp.Key,
                              CompanyKey = atp.CompanyKey,
                              Code = atp.Code,
                              Name = atp.Name,
                              Description = atp.Description,
                              Latitude = atp.Latitude,
                              Longitude = atp.Longitude,
                              RangeTolerance = atp.RangeTolerance,
                              Company = com
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var attendancePoints = await queries.ToListAsync();

            var viewModel = new AttendancePointList
            {
                AttendancePoints = attendancePoints.Select(atp => atp.ConvertToViewModelAttendancePoint())
            };

            return viewModel;
        }
    }
#endregion
#endregion

#region "Get List Attendance Point With Pagination"
#region "Query"
    public sealed record GetAttendancePointsPaginationQuery(PaginationConfig pagination, Expression<Func<AttendancePoint, bool>>[] wheres) : IRequest<PaginatedList<AttendancePoint>>;
#endregion
#region "Handler"
    public sealed class GetAttendancePointsPaginationQueryHandler : IRequestHandler<GetAttendancePointsPaginationQuery, PaginatedList<AttendancePoint>>
    {
        private readonly IDataContext _context;

        public GetAttendancePointsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<AttendancePoint>> Handle(GetAttendancePointsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from atp in _context.AttendancePoints
                          join com in _context.Companies on atp.CompanyKey equals com.Key
                          where atp.DeletedAt == null
                          select new AttendancePoint
                          {
                              Key = atp.Key,
                              CompanyKey = atp.CompanyKey,
                              Code = atp.Code,
                              Name = atp.Name,
                              Description = atp.Description,
                              Latitude = atp.Latitude,
                              Longitude = atp.Longitude,
                              RangeTolerance = atp.RangeTolerance,
                              Company = com
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

            var attendancePoints = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
        
            return await Task.FromResult(attendancePoints);
        }
    }
#endregion
#endregion

#region "Get Attendance Point By Id"
#region "Query"
    public sealed record GetAttendancePointQuery(Guid Key) : IRequest<AttendancePointForm>;
#endregion
#region "Handler"
    public sealed class GetAttendancePointQueryHandler : IRequestHandler<GetAttendancePointQuery, AttendancePointForm>
    {

        private readonly IDataContext _context;

        public GetAttendancePointQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<AttendancePointForm> Handle(GetAttendancePointQuery request, CancellationToken cancellationToken)
        {
            var attendancePoint = await (from atp in _context.AttendancePoints
                                         join com in _context.Companies on atp.CompanyKey equals com.Key
                                         where atp.Key == request.Key
                                         select new AttendancePoint
                                         {
                                             Key = atp.Key,
                                             CompanyKey = atp.CompanyKey,
                                             Code = atp.Code,
                                             Name = atp.Name,
                                             Description = atp.Description,
                                             Latitude = atp.Latitude,
                                             Longitude = atp.Longitude,
                                             RangeTolerance = atp.RangeTolerance,
                                             Company = com
                                         }).FirstOrDefaultAsync();

            if (attendancePoint == null)
                throw new Exception("Attendance Point not found.");

            return attendancePoint.ConvertToViewModelAttendancePoint();
        }
    }
#endregion
#endregion

#region "Save Attendance Point"
#region "Command"
    public sealed record SaveAttendancePointCommand(AttendancePointDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveAttendancePointCommandHandler : IRequestHandler<SaveAttendancePointCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<AttendancePointDto> _validator;

        public SaveAttendancePointCommandHandler(IDataContext context, IValidator<AttendancePointDto> validator)
        { 
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveAttendancePointCommand command, CancellationToken cancellationToken)
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

                var attendancePoint = command.Form.ConvertToEntity();
                if (attendancePoint.Key == Guid.Empty)
                {
                    attendancePoint.Key = Guid.NewGuid();
                }

                //Check if Attendance Point Exists
                var existingAttendancePoint = await _context.AttendancePoints.FirstOrDefaultAsync(x => x.Key == attendancePoint.Key && x.DeletedAt == null);

                if (existingAttendancePoint == null)
                {
                    //Add new Attendance Point
                    _context.AttendancePoints.Add(attendancePoint);
                }
                else
                {
                    attendancePoint.CreatedAt = existingAttendancePoint.CreatedAt;
                    attendancePoint.CreatedBy = existingAttendancePoint.CreatedBy;
                    _context.AttendancePoints.Entry(existingAttendancePoint).CurrentValues.SetValues(attendancePoint);
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

#region "Delete Attendance Point"
#region "Command"
    public sealed record DeleteAttendancePointCommand(Guid Key) : IRequest<Result<AttendancePoint>>;
#endregion
#region "Handler"
    public sealed class DeleteAttendancePointCommandHandler : IRequestHandler<DeleteAttendancePointCommand, Result<AttendancePoint>>
    {
        private readonly IDataContext _context;

        public DeleteAttendancePointCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<AttendancePoint>> Handle(DeleteAttendancePointCommand command, CancellationToken cancellationToken)
        {
            var attendancePoint = await _context.AttendancePoints.FirstOrDefaultAsync(x => x.Key == command.Key);

            try
            {
                if (attendancePoint == null)
                    throw new Exception("Attendance Point not found.");

                _context.AttendancePoints.Remove(attendancePoint);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<AttendancePoint>.Failure(new[] { ex.Message });
            }

            return Result<AttendancePoint>.Success(attendancePoint);
        }
    }
#endregion
#endregion
