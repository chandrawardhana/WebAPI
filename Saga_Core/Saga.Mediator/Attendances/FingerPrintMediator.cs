using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Employees;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.DomainShared.Interfaces;
using Saga.DomainShared.Models;
using Saga.Mediator.Attendances.AttendanceLogMachineMediator;
using Saga.Mediator.Employees.EmployeeMediator;
using Saga.Mediator.Services;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.FingerPrintMediator;

#region "Get List FingerPrint Machine"
#region "Query"
    public sealed record GetFingerPrintsQuery(Expression<Func<FingerPrint, bool>>[] wheres) : IRequest<List<FingerPrint>>;
#endregion
#region "Handler"
    public sealed class GetFingerPrintsQueryHandler : IRequestHandler<GetFingerPrintsQuery, List<FingerPrint>>
    {
        private readonly IDataContext _context;

        public GetFingerPrintsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<List<FingerPrint>> Handle(GetFingerPrintsQuery request, CancellationToken cancellationToken)
        {
            var queries = from fp in _context.FingerPrints
                          join com in _context.Companies on fp.CompanyKey equals com.Key
                          where fp.DeletedAt == null
                          select new FingerPrint
                          {
                              Key = fp.Key,
                              CompanyKey = fp.CompanyKey,
                              Code = fp.Code,
                              Name = fp.Name,
                              IPAddress = fp.IPAddress,
                              Method = fp.Method,
                              Port = fp.Port,
                              CommKey = fp.CommKey,
                              Comm = fp.Comm,
                              Baudrate = fp.Baudrate,
                              Description = fp.Description,
                              RetrieveScheduleTimes = fp.RetrieveScheduleTimes,
                              SerialNumber = fp.SerialNumber,
                              Company = com
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var fingerPrints = await queries.ToListAsync();

            return fingerPrints;
        }
    }
#endregion
#endregion

#region "Get List FingerPrint Machine With Pagination"
#region "Query"
    public sealed record GetFingerPrintsPaginationQuery(PaginationConfig pagination, Expression<Func<FingerPrint, bool>>[] wheres) : IRequest<PaginatedList<FingerPrint>>;
#endregion
#region "Handler"
    public sealed class GetFingerPrintsPaginationQueryHandler : IRequestHandler<GetFingerPrintsPaginationQuery, PaginatedList<FingerPrint>>
    {
        private readonly IDataContext _context;

        public GetFingerPrintsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<FingerPrint>> Handle(GetFingerPrintsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from fp in _context.FingerPrints
                          join com in _context.Companies on fp.CompanyKey equals com.Key
                          where fp.DeletedAt == null
                          select new FingerPrint
                          {
                              Key = fp.Key,
                              CompanyKey = fp.CompanyKey,
                              Code = fp.Code,
                              Name = fp.Name,
                              IPAddress = fp.IPAddress,
                              Method = fp.Method,
                              Port = fp.Port,
                              CommKey = fp.CommKey,
                              Comm = fp.Comm,
                              Baudrate = fp.Baudrate,
                              Description = fp.Description,
                              SerialNumber = fp.SerialNumber,
                              Company = com
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%") || EF.Functions.ILike(b.Method.ToString(), $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var fingerPrints = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
        
            return await Task.FromResult(fingerPrints);
        }
    }
#endregion
#endregion

#region "Get By Id FingerPrint Machine"
#region "Query"
    public sealed record GetFingerPrintQuery(Guid Key) : IRequest<FingerPrintForm>;
#endregion
#region "Handler"
    public sealed class GetFingerPrintQueryHandler : IRequestHandler<GetFingerPrintQuery, FingerPrintForm>
    {
        private readonly IDataContext _context;

        public GetFingerPrintQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<FingerPrintForm> Handle(GetFingerPrintQuery request, CancellationToken cancellationToken)
        {
            var fingerPrint = await (from fp in _context.FingerPrints
                                     join com in _context.Companies on fp.CompanyKey equals com.Key
                                     where fp.Key == request.Key
                                     select new FingerPrint
                                     {
                                         Key = fp.Key,
                                         CompanyKey = fp.CompanyKey,
                                         Code = fp.Code,
                                         Name = fp.Name,
                                         IPAddress = fp.IPAddress,
                                         Method = fp.Method,
                                         Port = fp.Port,
                                         CommKey = fp.CommKey,
                                         Comm = fp.Comm,
                                         Baudrate = fp.Baudrate,
                                         Description = fp.Description,
                                         RetrieveScheduleTimes = fp.RetrieveScheduleTimes,
                                         SerialNumber = fp.SerialNumber,
                                         Company = com
                                     }).FirstOrDefaultAsync();

            if (fingerPrint == null)
                throw new Exception("Finger Print Machine not found.");

            var viewModel = fingerPrint.ConvertToFingerPrintFormViewModel();

            if (fingerPrint.RetrieveScheduleTimes != null && fingerPrint.RetrieveScheduleTimes.Any())
            {
                var scheduleItems = fingerPrint.RetrieveScheduleTimes
                    .Select((time, index) => new
                    {
                        no = index + 1,
                        RetrieveScheduleTimes = $"{time.Hours:D2}:{time.Minutes:D2}"
                    })
                    .OrderBy(x => TimeSpan.Parse(x.RetrieveScheduleTimes))
                    .ToArray();

                viewModel.JsonRetrieveScheduleTimes = JsonConvert.SerializeObject(
                    scheduleItems,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.None
                    });
            }
            else
            {
                viewModel.JsonRetrieveScheduleTimes = "[]";
            }

            return viewModel;
        }
    }
#endregion
#endregion

#region "Save FingerPrint Machine"
#region "Command"
    public sealed record SaveFingerPrintCommand(FingerPrintDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveFingerPrintCommandHandler : IRequestHandler<SaveFingerPrintCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<FingerPrintDto> _validator;

        public SaveFingerPrintCommandHandler(IDataContext context, IValidator<FingerPrintDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveFingerPrintCommand command, CancellationToken cancellationToken)
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

                var fingerPrint = command.Form.ConvertToEntity();
                if (fingerPrint.Key == Guid.Empty)
                {
                    fingerPrint.Key = Guid.NewGuid();
                }

                //Check if fingerPrint exist
                var existingFingerPrint = await _context.FingerPrints.FirstOrDefaultAsync(x => x.Key == fingerPrint.Key && x.DeletedAt == null, cancellationToken);
                if (existingFingerPrint == null)
                {
                    //Add new FingerPrint
                    _context.FingerPrints.Add(fingerPrint);
                }
                else
                {
                    //Update existing FingerPrint
                    fingerPrint.CreatedAt = existingFingerPrint.CreatedAt;
                    fingerPrint.CreatedBy = existingFingerPrint.CreatedBy;
                    _context.FingerPrints.Entry(existingFingerPrint).CurrentValues.SetValues(fingerPrint);
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

#region "Delete FingerPrint Machine"
#region "Command"
    public sealed record DeleteFingerPrintCommand(Guid Key) : IRequest<Result<FingerPrint>>;
#endregion
#region "Handler"
    public sealed class DeleteFingerPrintCommandHandler : IRequestHandler<DeleteFingerPrintCommand, Result<FingerPrint>>
    {
        private readonly IDataContext _context;

        public DeleteFingerPrintCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<FingerPrint>> Handle(DeleteFingerPrintCommand command, CancellationToken cancellationToken)
        {
            var fingerPrint = await _context.FingerPrints.FirstOrDefaultAsync(x => x.Key == command.Key, cancellationToken);

            try
            {
                if (fingerPrint == null)
                    throw new Exception("FingerPrint Machine not found.");

                _context.FingerPrints.Remove(fingerPrint);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<FingerPrint>.Failure(new[] { ex.Message });
            }

            return Result<FingerPrint>.Success(fingerPrint);
        }
    }
#endregion
#endregion

#region "Test Connection FingerPrint Machine"
#region "Command"
    public sealed record TestConnectionFingerPrintCommand(TestConnectionDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class TestConnectionFingerPrintCommandHandler(IAttendanceLogMachine _attendanceLogMachine) : IRequestHandler<TestConnectionFingerPrintCommand, Result>
    {
        public async Task<Result> Handle(TestConnectionFingerPrintCommand command, CancellationToken cancellationToken)
            => await _attendanceLogMachine.ConnectToFingerDeviceAsync(command.Form.Method, command.Form.IPAddress ?? String.Empty, command.Form.Port ?? 0, command.Form.Comm ?? String.Empty, command.Form.Baudrate ?? 0, command.Form.SerialNumber ?? String.Empty);
    }
#endregion
#endregion

#region "Retrieve Attendance Log FingerPrint Machine"
#region "Command"
    public sealed record RetrieveAttendanceLogCommand(Guid Key) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class RetrieveAttendanceLogCommandHandler(IDataContext _context,
                                                            ILogger<RetrieveAttendanceLogCommand> _logger,
                                                            IMediator _mediator,
                                                            IAttendanceLogMachine _attendanceLogMachine) : IRequestHandler<RetrieveAttendanceLogCommand, Result>
    {
        public async Task<Result> Handle(RetrieveAttendanceLogCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var device = await (from fp in _context.FingerPrints
                                    join com in _context.Companies on fp.CompanyKey equals com.Key
                                    where fp.Key == command.Key
                                    select new FingerPrint
                                    {
                                        Key = fp.Key,
                                        CompanyKey = fp.CompanyKey,
                                        Code = fp.Code,
                                        Name = fp.Name,
                                        IPAddress = fp.IPAddress,
                                        Method = fp.Method,
                                        Port = fp.Port,
                                        CommKey = fp.CommKey,
                                        Comm = fp.Comm,
                                        Baudrate = fp.Baudrate,
                                        Description = fp.Description,
                                        RetrieveScheduleTimes = fp.RetrieveScheduleTimes,
                                        Company = com
                                    }).FirstOrDefaultAsync();

                if (device == null)
                    throw new Exception("Finger Print Device not found.");

                var (lastCaptureTime, employeeAttendances, existingAttendances) = await GetAttendanceDataAsync(device);

                var logs = await _attendanceLogMachine.RetrieveLogsAsync(device, employeeAttendances, existingAttendances, lastCaptureTime, cancellationToken);

                if (logs.Any())
                {
                    var attendanceLogs = new AttendanceLogMachineDto
                    {
                        AttendanceLogMachines = logs.Select(x => x.ConvertToAttendanceLogDto()).ToList()
                    };

                    var result = await _mediator.Send(new SaveAttendanceLogMachinesCommand(attendanceLogs));

                    LogAttendanceRecords(logs, employeeAttendances);
                    _logger.LogInformation("Successfully retrieved {Count} logs from device {DeviceName}",
                        logs.Count, device.Name);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }

            return Result.Success();
        }

        private async Task<(DateTime lastCaptureTime, List<EmployeeAttendance> employeeAttendances, List<AttendanceLogMachineForm> existingAttendances)> GetAttendanceDataAsync(FingerPrint device)
        {
            var lastCaptureTime = await _mediator.Send(new GetLastCaptureTimeQuery());

            _logger.LogInformation(
                "Last capture time for device {DeviceName}: {LastCaptureTime}",
                device.Name,
                lastCaptureTime);

            var employeeAttendances = await _mediator.Send(new GetEmployeeAttendancesWithFingerPrintIDQuery(device.CompanyKey));

            var existingAttendances = await _mediator.Send(new GetExistingAttendancesQuery(lastCaptureTime));

            return (lastCaptureTime, employeeAttendances, existingAttendances);
        }

        private void LogAttendanceRecords(List<AttendanceLogMachineForm> logs, List<EmployeeAttendance> employeeAttendances)
        {
            foreach (var attendance in logs)
            {
                var employee = employeeAttendances.FirstOrDefault(ea => ea.EmployeeKey == attendance.EmployeeKey)?.Employee;
                if (employee != null)
                {
                    _logger.LogInformation(
                        "Created attendance record - Employee: {EmployeeName} ({EmployeeCode}), Time: {LogTime}, InOutMode: {MappedMode}",
                        $"{employee.FirstName} {employee.LastName}", employee.Code, attendance.LogTime, attendance.InOutMode);
                }
            }
        }
    }
#endregion
#endregion

#region "Get FingerPrint Data Retrieve"
#region "Query"
public sealed record GetFingerPrintDataRetrieveQuery(PaginationConfig pagination) : IRequest<PaginatedList<RetrieveDataFingerPrint>>;
#endregion
#region "Handler"
public sealed record GetFingerPrintDataRetrieveQueryHandler(IDataContext _context,
                                                            IAttendanceLogMachine _attendanceLogMachine) : IRequestHandler<GetFingerPrintDataRetrieveQuery, PaginatedList<RetrieveDataFingerPrint>>
{
    public async Task<PaginatedList<RetrieveDataFingerPrint>> Handle(GetFingerPrintDataRetrieveQuery request, CancellationToken cancellationToken)
    {
        var queries = from fp in _context.FingerPrints
                      join com in _context.Companies on fp.CompanyKey equals com.Key
                      where fp.DeletedAt == null
                      select new FingerPrint
                      {
                          Key = fp.Key,
                          CompanyKey = fp.CompanyKey,
                          Code = fp.Code,
                          Name = fp.Name,
                          IPAddress = fp.IPAddress,
                          Method = fp.Method,
                          Port = fp.Port,
                          CommKey = fp.CommKey,
                          Comm = fp.Comm,
                          Baudrate = fp.Baudrate,
                          Description = fp.Description,
                          SerialNumber = fp.SerialNumber,
                          Company = com
                      };

        string search = request.pagination.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%"));
        }

        var fingerPrints = await queries.ToListAsync();

        var retrieveDataList = new List<RetrieveDataFingerPrint>();

        foreach(var fp in fingerPrints)
        {
            var connectionResult = await _attendanceLogMachine.ConnectToFingerDeviceAsync(
                fp.Method,
                fp.IPAddress ?? string.Empty,
                fp.Port,
                fp.Comm ?? string.Empty,
                fp.Baudrate ?? 0,
                fp.SerialNumber ?? string.Empty);

            retrieveDataList.Add(new RetrieveDataFingerPrint
            {
                Key = fp.Key,
                CompanyKey = fp.CompanyKey,
                Code = fp.Code,
                Name = fp.Name,
                IPAddress = fp.IPAddress,
                Port = fp.Port,
                CommKey = fp.CommKey ?? string.Empty,
                Status = connectionResult.Succeeded,
                Company = fp.Company
            });
        }

        return new PaginatedList<RetrieveDataFingerPrint>(
            retrieveDataList,
            retrieveDataList.Count,
            request.pagination.PageNumber,
            request.pagination.PageSize);
    }
}
#endregion
#endregion
