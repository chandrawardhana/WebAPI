using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.Persistence.Context;
using System.Linq;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.AttendanceLogMachineMediator;

#region "Get Last Capture Time"
#region "Query"
public sealed record GetLastCaptureTimeQuery() : IRequest<DateTime>;
#endregion
#region "Handler"
public sealed class GetLastCaptureTimeQueryHandler : IRequestHandler<GetLastCaptureTimeQuery, DateTime>
{
    private readonly IDataContext _context;

    public GetLastCaptureTimeQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<DateTime> Handle(GetLastCaptureTimeQuery request, CancellationToken cancellationToken)
    {
        var lastCaptureTime = await (from at in _context.AttendanceLogMachines
                                     join e in _context.Employees on at.EmployeeKey equals e.Key
                                     where at.DeletedAt == null
                                     orderby at.LogTime descending
                                     select new AttendanceLogMachine
                                     {
                                         Key = at.Key,
                                         EmployeeKey = at.EmployeeKey,
                                         LogTime = at.LogTime,
                                         InOutMode = at.InOutMode,
                                         Employee = e
                                     }
                                    ).FirstOrDefaultAsync();

        return lastCaptureTime?.LogTime ?? default(DateTime);
    }
}
#endregion
#endregion

#region "Get Existing Attendances"
#region "Query"
public sealed record GetExistingAttendancesQuery(DateTime lastCaptureTime) : IRequest<List<AttendanceLogMachineForm>>;
#endregion
#region "Handler"
public sealed class GetExistingAttendancesQueryHandler : IRequestHandler<GetExistingAttendancesQuery, List<AttendanceLogMachineForm>>
{
    private readonly IDataContext _context;

    public GetExistingAttendancesQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<List<AttendanceLogMachineForm>> Handle(GetExistingAttendancesQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var queries = from at in _context.AttendanceLogMachines
                      join e in _context.Employees on at.EmployeeKey equals e.Key
                      where at.LogTime >= (request.lastCaptureTime != default ? request.lastCaptureTime : today) &&
                           at.LogTime < tomorrow &&
                           at.DeletedAt == null
                      select new AttendanceLogMachine
                      {
                          Key = at.Key,
                          EmployeeKey = at.EmployeeKey,
                          LogTime = at.LogTime,
                          InOutMode = at.InOutMode,
                          Employee = e
                      };

        var existingAttendances = await queries.ToListAsync();

        var viewModel = existingAttendances.Select(x => x.ConvertToAttendanceLogMachineFormViewModel()).ToList();

        return viewModel;
    }
}
#endregion
#endregion

#region "Save List Attendance From FingerPrint Machine"
#region "Command"
public sealed record SaveAttendanceLogMachinesCommand(AttendanceLogMachineDto Form) : IRequest<Result>;
#endregion
#region "Handler"
public sealed class SaveAttendanceLogMachinesCommandHandler : IRequestHandler<SaveAttendanceLogMachinesCommand, Result>
{
    private readonly IDataContext _context;

    public SaveAttendanceLogMachinesCommandHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SaveAttendanceLogMachinesCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (command.Form.AttendanceLogMachines != null && command.Form.AttendanceLogMachines.Any())
            {
                var attendanceLogs = command.Form.AttendanceLogMachines.Select(x => x.ConvertToEntity());

                _context.AttendanceLogMachines.AddRange(attendanceLogs);

                var result = await _context.SaveChangesAsync(cancellationToken);
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

#region "Get List Attendance Log"
#region "Query"
public sealed record GetAttendanceLogMachinesQuery(Expression<Func<AttendanceLogMachine, bool>>[] wheres) : IRequest<IEnumerable<AttendanceLogMachine>>;
#endregion
#region "Handler"
public sealed class GetAttendanceLogMachinesQueryHandler : IRequestHandler<GetAttendanceLogMachinesQuery, IEnumerable<AttendanceLogMachine>>
{
    private readonly IDataContext _context;

    public GetAttendanceLogMachinesQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AttendanceLogMachine>> Handle(GetAttendanceLogMachinesQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.AttendanceLogMachines.AsQueryable().Where(x => x.DeletedAt == null);

        request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });
        var attendanceLogs = await queries.ToListAsync();

        return attendanceLogs;
    }
}
#endregion
#endregion

#region "Save Import Log"
#region "Command"
public sealed record SaveImportLogCommand(ImportFingerLogDto Form) : IRequest<Result>;
#endregion
#region "Handler"
public sealed class SaveImportLogCommandHandler(IDataContext _context) : IRequestHandler<SaveImportLogCommand, Result>
{
    public async Task<Result> Handle(SaveImportLogCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (command.Form == null || !command.Form.AttendanceLogUserInfos.Any())
                return Result.Failure(new[] { "No attendance records to import" });

            var attendanceLogMachineDto = command.Form.ConvertToAttendanceLogMachineDto();

            if (!attendanceLogMachineDto.AttendanceLogMachines.Any())
                return Result.Failure(new[] { "No valid attendance records to import after filtering" });

            var attendanceLogMachines = attendanceLogMachineDto.AttendanceLogMachines.Select(x => x.ConvertToEntity());

            _context.AttendanceLogMachines.AddRange(attendanceLogMachines);

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
