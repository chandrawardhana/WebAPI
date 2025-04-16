using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Attendances;

namespace Saga.DomainShared.Interfaces;

public interface IAttendanceLogMachine
{
    Task<Result> ConnectToFingerDeviceAsync(ConnectionMethod method, string ipAddress, int port, string comm, int baudRate, string serialNumber);
    Task<List<AttendanceLogMachineForm>> RetrieveLogsAsync(
            FingerPrint device,
            List<EmployeeAttendance> employeeAttendances,
            List<AttendanceLogMachineForm> existingAttendances,
            DateTime lastCaptureTime,
            CancellationToken stoppingToken);
    Task<List<AttendanceLogUserInfo>> ParseAttendanceLogFileAsync(Stream fileStream);
}
