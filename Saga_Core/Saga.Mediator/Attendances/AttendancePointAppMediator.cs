using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.DomainShared.Helpers;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.AttendancePointAppMediator;

#region "Get List Attendance Point App"
#region "Query"
    public sealed record GetAttendancePointAppsQuery(Expression<Func<AttendancePointApp, bool>>[] wheres) : IRequest<IEnumerable<AttendancePointApp>>;
#endregion
#region "Handler"
    public sealed class GetAttendancePointAppsQueryHandler : IRequestHandler<GetAttendancePointAppsQuery, IEnumerable<AttendancePointApp>>
    {
        private readonly IDataContext _context;

        public GetAttendancePointAppsQueryHandler(IDataContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AttendancePointApp>> Handle(GetAttendancePointAppsQuery request, CancellationToken cancellationToken)
        {
            var queries = from ap in _context.AttendancePointApps
                          join e in _context.Employees on ap.EmployeeKey equals e.Key
                          where ap.DeletedAt == null
                          select new AttendancePointApp
                          {
                              Key = ap.Key,
                              EmployeeKey = ap.EmployeeKey,
                              Latitude = ap.Latitude,
                              Longitude = ap.Longitude,
                              InOutMode = ap.InOutMode,
                              AbsenceTime = ap.AbsenceTime,
                              Employee = e
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(filter);
            }

            var attendances = await queries.ToListAsync();

            return attendances;
        }
    }
#endregion
#endregion

#region "Get List Attendance Point App With Pagination"
#region "Query"
    public sealed record GetAttendancePointAppsPaginationQuery(PaginationConfig pagination, RetrieveAttendancePointDto filter) : IRequest<PaginatedList<AttendancePointItemPaginationList>>;
#endregion
#region "Handler"
    public sealed class GetAttendancePointAppsPaginationQueryHandler : IRequestHandler<GetAttendancePointAppsPaginationQuery, PaginatedList<AttendancePointItemPaginationList>>
    {
        private readonly IDataContext _context;

        public GetAttendancePointAppsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<AttendancePointItemPaginationList>> Handle(GetAttendancePointAppsPaginationQuery request, CancellationToken cancellationToken)
        {
            var parameters = new List<object>();
            var whereClauses = new List<string> { @"""DeletedAt"" IS NULL" };

            // Build date filters using TO_DATE
            if (request.filter.DateStart.HasValue)
            {
                whereClauses.Add(@" CAST(""AbsenceTime"" AS DATE) >= TO_DATE({0}, 'YYYY-MM-DD')");
                parameters.Add(request.filter.DateStart.Value.ToInvariantString());
            }
            if (request.filter.DateEnd.HasValue)
            {
                whereClauses.Add(@" CAST(""AbsenceTime"" AS DATE) <= TO_DATE({" + parameters.Count + @"}, 'YYYY-MM-DD')");
                parameters.Add(request.filter.DateEnd.Value.ToInvariantString());
            }

            // Construct SQL query
            string whereClause = string.Join(" AND ", whereClauses);
            string sql = $@"SELECT * FROM ""Attendance"".""tbtattendancepointapp"" WHERE {whereClause}";

            // Create base query with raw SQL
            var baseQuery = _context.AttendancePointApps.FromSqlRaw(sql, parameters.ToArray());

            // Join with other tables and apply remaining filters
            var queries = from ap in baseQuery
                          join e in _context.Employees on ap.EmployeeKey equals e.Key
                          join com in _context.Companies on e.CompanyKey equals com.Key
                          join org in _context.Organizations on e.OrganizationKey equals org.Key
                          where
                              (request.filter.EmployeeKey.HasValue && request.filter.EmployeeKey != Guid.Empty ? ap.EmployeeKey == request.filter.EmployeeKey : true) &&
                              (request.filter.CompanyKey.HasValue && request.filter.CompanyKey != Guid.Empty ? e.CompanyKey == request.filter.CompanyKey : true)
                          select new
                          {
                              AttendancePointApp = ap,
                              Employee = e,
                              Company = com,
                              Organization = org
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Employee.Code, $"%{search}%") || EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") || EF.Functions.ILike(b.Employee.LastName, $"%{search}%"));
            }

            var attendancePointApps = await queries.ToListAsync();

            var attendancePointAppList = attendancePointApps
                                .Select(x => new AttendancePointItemPaginationList
                                {
                                    Key = x.AttendancePointApp.Key,
                                    EmployeeKey = x.AttendancePointApp.EmployeeKey,
                                    EmployeeId = x.Employee.Code,
                                    EmployeeName = (x.Employee.FirstName ?? String.Empty) + " " + (x.Employee.LastName ?? String.Empty),
                                    CompanyName = x.Company.Name,
                                    OrganizationName = x.Organization.Name,
                                    AbsenceDate = DateOnly.FromDateTime(x.AttendancePointApp.AbsenceTime),
                                    AbsenceTime = TimeOnly.FromDateTime(x.AttendancePointApp.AbsenceTime),
                                    Mode = Enum.GetName(typeof(InOutMode), x.AttendancePointApp.InOutMode) ?? String.Empty,
                                    Latitude = x.AttendancePointApp.Latitude,
                                    Longitude = x.AttendancePointApp.Longitude
                                }).ToList();

            return new PaginatedList<AttendancePointItemPaginationList>(attendancePointAppList, attendancePointAppList.Count(), request.pagination.PageNumber, request.pagination.PageSize);
        }
    }
#endregion
#endregion

#region "Get Attendance Point App By Id"
#region "Query"
    public sealed record GetAttendancePointAppQuery(Guid Key) : IRequest<AttendancePointAppForm>;
#endregion
#region "Handler"
    public sealed class GetAttendancePointAppQueryHandler : IRequestHandler<GetAttendancePointAppQuery, AttendancePointAppForm>
    {
        private readonly IDataContext _context;

        public GetAttendancePointAppQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<AttendancePointAppForm> Handle(GetAttendancePointAppQuery request, CancellationToken cancellationToken)
        {
            var attendancePointApp = await (from ap in _context.AttendancePointApps
                                            join e in _context.Employees on ap.EmployeeKey equals e.Key
                                            where ap.Key == request.Key
                                            select new AttendancePointApp
                                            {
                                                Key = ap.Key,
                                                EmployeeKey = ap.EmployeeKey,
                                                Latitude = ap.Latitude,
                                                Longitude = ap.Longitude,
                                                InOutMode = ap.InOutMode,
                                                AbsenceTime = ap.AbsenceTime,
                                                Employee = e,
                                         }).FirstOrDefaultAsync();

            if (attendancePointApp == null)
                throw new Exception("Attendance Point App not found.");

            var title = await _context.Titles.FirstOrDefaultAsync(x => x.Key == attendancePointApp.Employee.TitleKey);

            var shift = await (from ea in _context.EmployeesAttendances
                               join s in _context.Shifts on ea.ShiftKey equals s.Key
                               join com in _context.Companies on s.CompanyKey equals com.Key
                               where ea.EmployeeKey == attendancePointApp.EmployeeKey
                               select new Shift
                               {
                                   CompanyKey = s.CompanyKey,
                                   ShiftGroupName = s.ShiftGroupName,
                                   MaxLimit = s.MaxLimit,
                                   Description = s.Description,
                                   Company = com
                               }).FirstOrDefaultAsync();

            var shiftSchedule = await (from ea in _context.EmployeesAttendances
                                       join sh in _context.ShiftSchedules on ea.ShiftScheduleKey equals sh.Key
                                       join com in _context.Companies on sh.CompanyKey equals com.Key
                                       where ea.EmployeeKey == attendancePointApp.EmployeeKey
                                       select new ShiftSchedule
                                       {
                                           CompanyKey = sh.CompanyKey,
                                           GroupName = sh.GroupName,
                                           YearPeriod = sh.YearPeriod,
                                           MonthPeriod = sh.MonthPeriod,
                                           IsRoaster = sh.IsRoaster,
                                           Company = com
                                       }).FirstOrDefaultAsync();

            var viewModel = attendancePointApp.ConvertToAttendancePointAppFormViewModel(title, shift, shiftSchedule);
        
            return viewModel;
        }
    }
#endregion
#endregion

#region "Save Attendance Point App"
#region "Command"
    public sealed record SaveAttendancePointAppCommand(AttendancePointAppDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveAttendancePointAppCommandHandler : IRequestHandler<SaveAttendancePointAppCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<AttendancePointAppDto> _validator;

        public SaveAttendancePointAppCommandHandler(IDataContext context, IValidator<AttendancePointAppDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveAttendancePointAppCommand command, CancellationToken cancellationToken)
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

                var attendancePointApp = command.Form.ConvertToEntity();
                if (attendancePointApp.Key == Guid.Empty)
                {
                    attendancePointApp.Key = Guid.NewGuid();
                }

                //Check if Attendance Point App is exists
                var existingAttendancePointApp = await _context.AttendancePointApps.FirstOrDefaultAsync(x => x.Key == attendancePointApp.Key && x.DeletedAt == null);
                if (existingAttendancePointApp == null)
                {
                    //Add new Attendance Point App
                    _context.AttendancePointApps.Add(attendancePointApp);
                } else
                {
                    attendancePointApp.CreatedAt = existingAttendancePointApp.CreatedAt;
                    attendancePointApp.CreatedBy = existingAttendancePointApp.CreatedBy;
                    _context.AttendancePointApps.Entry(existingAttendancePointApp).CurrentValues.SetValues(attendancePointApp);
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

#region "Delete Attendance Point App"
#region "Command"
    public sealed record DeleteAttendancePointAppCommand(Guid Key) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class DeleteAttendancePointAppCommandHandler : IRequestHandler<DeleteAttendancePointAppCommand, Result>
    {
        private readonly IDataContext _context;

        public DeleteAttendancePointAppCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteAttendancePointAppCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var attendancePointApp = await _context.AttendancePointApps.FirstOrDefaultAsync(x => x.Key == command.Key);
                if (attendancePointApp == null)
                    throw new Exception("Attendance Point App not found.");

                _context.AttendancePointApps.Remove(attendancePointApp);
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
