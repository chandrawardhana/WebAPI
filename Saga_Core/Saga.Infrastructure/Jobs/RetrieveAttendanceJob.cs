using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Employees;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared.Interfaces;
using Saga.Mediator.Attendances.AttendanceLogMachineMediator;
using Saga.Mediator.Attendances.FingerPrintMediator;
using Saga.Mediator.Employees.EmployeeMediator;
using Saga.Mediator.Services;
using System.Linq.Expressions;

namespace Saga.Infrastructure.Jobs
{
    public class RetrieveAttendanceJob : BackgroundService
    {
        private readonly ILogger<RetrieveAttendanceJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public RetrieveAttendanceJob(ILogger<RetrieveAttendanceJob> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    List<Expression<Func<FingerPrint, bool>>> wheres = new();
                    var fingerPrints = await mediator.Send(new GetFingerPrintsQuery(wheres.ToArray()), stoppingToken);
                    bool allDevicesProcessed = true;
                    var currentTime = DateTime.Now.TimeOfDay;

                    foreach (var device in fingerPrints)
                    {
                        if (device.RetrieveScheduleTimes == null || !device.RetrieveScheduleTimes.Any())
                            continue;

                        if (device.RetrieveScheduleTimes.Any(t => Math.Abs((t - currentTime).TotalMinutes) <= 5))
                        {
                            allDevicesProcessed = false;
                            await RetrieveDeviceLogAsync(device, stoppingToken);
                        }
                    }

                    if (allDevicesProcessed)
                    {
                        _logger.LogInformation("All attendance logs retrieved successfully. Stopping the service.");
                        _cancellationTokenSource.Cancel();
                        await StopAsync(stoppingToken);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while retrieving attendance logs");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task RetrieveDeviceLogAsync(FingerPrint device, CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var attendanceLogMachine = scope.ServiceProvider.GetRequiredService<IAttendanceLogMachine>();

                var (lastCaptureTime, employeeAttendances, existingAttendances) = await GetAttendanceDataAsync(device);

                var logs = await attendanceLogMachine.RetrieveLogsAsync(
                    device,
                    employeeAttendances,
                    existingAttendances,
                    lastCaptureTime,
                    stoppingToken);

                if (logs.Any())
                {
                    var attendanceLogs = new AttendanceLogMachineDto
                    {
                        AttendanceLogMachines = logs.Select(x => x.ConvertToAttendanceLogDto()).ToList()
                    };

                    var result = await mediator.Send(new SaveAttendanceLogMachinesCommand(attendanceLogs));

                    LogAttendanceRecords(logs, employeeAttendances);
                    _logger.LogInformation("Successfully retrieved {Count} logs from device {DeviceName}",
                        logs.Count, device.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs from device {DeviceName}", device.Name);
            }
        }

        private async Task<(DateTime lastCaptureTime, List<EmployeeAttendance> employeeAttendances, List<AttendanceLogMachineForm> existingAttendances)> GetAttendanceDataAsync(FingerPrint device)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var lastCaptureTime = await mediator.Send(new GetLastCaptureTimeQuery());

            _logger.LogInformation(
                "Last capture time for device {DeviceName}: {LastCaptureTime}",
                device.Name,
                lastCaptureTime);

            var employeeAttendances = await mediator.Send(new GetEmployeeAttendancesWithFingerPrintIDQuery(device.CompanyKey));

            var existingAttendances = await mediator.Send(new GetExistingAttendancesQuery(lastCaptureTime));

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
}
