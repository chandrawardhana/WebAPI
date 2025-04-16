using MediatR;
using Microsoft.Extensions.Logging;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.DomainShared.Interfaces;
using Saga.Infrastructure.Extensions;
using Saga.Mediator.Employees.EmployeeMediator;
using Saga.Mediator.Organizations.CompanyMediator;
using System.Text;
using zkemkeeper;

namespace Saga.Infrastructure.Services;

public class AttendanceLogMachineService : IAttendanceLogMachine
{
    private readonly ILogger<AttendanceLogMachineService> _logger;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<Guid, CZKEMClass> _deviceConnections;
    private readonly IMediator _mediator;

    public AttendanceLogMachineService(ILogger<AttendanceLogMachineService> logger, HttpClient httpClient, IMediator mediator)
    {
        _logger = logger;
        _httpClient = httpClient;
        _deviceConnections = new Dictionary<Guid, CZKEMClass>();
        _mediator = mediator;
    }

    public Task<Result> ConnectToFingerDeviceAsync(ConnectionMethod method, string ipAddress, int port, string comm, int baudRate, string serialNumber)
    {
        try
        {
            CZKEMClass? zkemKeeper = null;
            bool isConnected = false;

            try
            {
                zkemKeeper = new CZKEMClass();
                isConnected = ConnectToFingerDeviceExtension.ConnectToDevice(zkemKeeper, method, ipAddress, port, comm, baudRate, _httpClient, serialNumber);

                if (!isConnected)
                {
                    return Task.FromResult(Result.Failure(new[] { $"Failed to connect to device ({ipAddress})" }));
                }

                return Task.FromResult(Result.Success());
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result.Failure(new[] { $"Error connecting to device: {ex.Message}" }));
            }
            finally
            {
                if (zkemKeeper != null && isConnected)
                {
                    try
                    {
                        zkemKeeper.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        // Log the error instead of returning from finally block
                        _logger.LogWarning($"Error disconnecting from device : {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure(new[] { ex.Message }));
        }
    }

    public Task<List<AttendanceLogMachineForm>> RetrieveLogsAsync(FingerPrint device, List<EmployeeAttendance> employeeAttendances, List<AttendanceLogMachineForm> existingAttendances, DateTime lastCaptureTime, CancellationToken stoppingToken)
    {
        return device.Method switch
        {
            ConnectionMethod.Web => RetrieveWebLogsAsync(device, employeeAttendances, existingAttendances, lastCaptureTime, stoppingToken),
            ConnectionMethod.Serial => RetrieveZKLogsAsync(device, employeeAttendances, existingAttendances, lastCaptureTime, stoppingToken),
            ConnectionMethod.Port => RetrieveZKLogsAsync(device, employeeAttendances, existingAttendances, lastCaptureTime, stoppingToken),
            _ => Task.FromResult(new List<AttendanceLogMachineForm>())
        };
    }

    private async Task<List<AttendanceLogMachineForm>> RetrieveWebLogsAsync(FingerPrint device, List<EmployeeAttendance> employeeAttendances, List<AttendanceLogMachineForm> existingAttendances, DateTime lastCaptureTime, CancellationToken stoppingToken)
    {
        var attendances = new List<AttendanceLogMachineForm>();

        try
        {
            // Initial handshake
            var handshakeUrl = $"http://{device.IPAddress}:{device.Port}/iclock/cdata?SN={device.SerialNumber}&options=all&pushver=2.2.14&language=73";
            var handshakeResponse = await _httpClient.GetAsync(handshakeUrl, stoppingToken);
            handshakeResponse.EnsureSuccessStatusCode();

            // Prepare attendance query
            var startTime = lastCaptureTime != default ? lastCaptureTime : DateTime.Today;
            var endTime = DateTime.Now;
            var cmdId = Guid.NewGuid().ToString("N");

            var queryUrl = $"http://{device.IPAddress}:{device.Port}/iclock/getrequest?SN={device.SerialNumber}";
            var queryContent = $"C:{cmdId}:DATA QUERY ATTLOG StartTime={startTime:yyyy-MM-dd HH:mm:ss}\tEndTime={endTime:yyyy-MM-dd HH:mm:ss}";

            var response = await _httpClient.PostAsync(queryUrl, new StringContent(queryContent, Encoding.UTF8), stoppingToken);

            response.EnsureSuccessStatusCode();
            var logData = await response.Content.ReadAsStringAsync();

            ProcessWebLogData(logData, employeeAttendances, existingAttendances, attendances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs from web device {DeviceName}", device.Name);
        }

        return attendances;
    }


    private async Task<List<AttendanceLogMachineForm>> RetrieveZKLogsAsync(FingerPrint device, List<EmployeeAttendance> employeeAttendances, List<AttendanceLogMachineForm> existingAttendances, DateTime lastCaptureTime, CancellationToken stoppingToken)
    {
        var attendances = new List<AttendanceLogMachineForm>();
        CZKEMClass? zkemKeeper = null;
        bool isConnected = false;

        try
        {
            if (!_deviceConnections.TryGetValue(device.Key, out zkemKeeper))
            {
                zkemKeeper = new CZKEMClass();
                _deviceConnections[device.Key] = zkemKeeper;
            }

            isConnected = ConnectToFingerDeviceExtension.ConnectToDevice(zkemKeeper, device.Method, device.IPAddress, device.Port, device.Comm, device.Baudrate);

            if (!isConnected)
            {
                _logger.LogWarning("Failed to connect to device {DeviceName} ({DeviceIP})",
                    device.Name, device.IPAddress);
                return attendances;
            }

            ProcessZKDeviceLogs(zkemKeeper, employeeAttendances, existingAttendances, attendances, lastCaptureTime);
        }
        finally
        {
            if (zkemKeeper != null && isConnected)
            {
                try
                {
                    zkemKeeper.EnableDevice(1, true);
                    zkemKeeper.Disconnect();
                    _deviceConnections.Remove(device.Key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disconnecting from device {DeviceName}", device.Name);
                }
            }
        }

        return await Task.FromResult(attendances);
    }

    private void ProcessWebLogData(string logData, List<EmployeeAttendance> employeeAttendances, List<AttendanceLogMachineForm> existingAttendances, List<AttendanceLogMachineForm> attendances)
    {
        foreach (var line in logData.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\t');
            if (parts.Length < 3) continue;

            var enrollNumber = parts[0];
            if (DateTime.TryParse(parts[1], out var logTime))
            {
                var inOutMode = ParseInOutMode(parts[2]);
                ProcessAttendanceRecord(employeeAttendances, existingAttendances, attendances, enrollNumber, logTime, inOutMode);
            }
        }
    }

    private void ProcessZKDeviceLogs(CZKEMClass zkemKeeper, List<EmployeeAttendance> employeeAttendances, List<AttendanceLogMachineForm> existingAttendances, List<AttendanceLogMachineForm> attendances, DateTime lastCaptureTime)
    {
        const int machineNumber = 1;
        zkemKeeper.EnableDevice(machineNumber, false);

        if (zkemKeeper.ReadGeneralLogData(machineNumber))
        {
            string enrollNumber = string.Empty;
            int verifyMode = 0;
            int inOutMode = 0;
            int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, workCode = 0;

            while (zkemKeeper.SSR_GetGeneralLogData(machineNumber, out enrollNumber, out verifyMode,
                   out inOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode))
            {
                var logTime = new DateTime(year, month, day, hour, minute, second);
                if (lastCaptureTime != default && logTime <= lastCaptureTime)
                {
                    continue;
                }

                ProcessAttendanceRecord(employeeAttendances, existingAttendances, attendances, enrollNumber, logTime, (InOutMode)inOutMode);
            }
        }
    }

    private void ProcessAttendanceRecord(List<EmployeeAttendance> employeeAttendances, List<AttendanceLogMachineForm> existingAttendances, List<AttendanceLogMachineForm> attendances, string enrollNumber, DateTime logTime, InOutMode inOutMode)
    {
        var matchingEmployee = employeeAttendances.FirstOrDefault(ea =>
            ea.FingerPrintID == enrollNumber);

        if (matchingEmployee != null && !IsDuplicateAttendance(existingAttendances, matchingEmployee.EmployeeKey, inOutMode, logTime))
        {
            var attendance = CreateAttendanceLogRecord(matchingEmployee.EmployeeKey, logTime, inOutMode);

            attendances.Add(attendance);
            existingAttendances.Add(attendance);
        }
    }

    private InOutMode ParseInOutMode(string mode) => mode.ToLower() switch
    {
        "0" => InOutMode.CheckIn,
        "1" => InOutMode.CheckOut,
        "2" => InOutMode.BreakOut,
        "3" => InOutMode.BreakIn,
        "4" => InOutMode.OTIn,
        "5" => InOutMode.OTOut,
        _ => InOutMode.CheckIn
    };

    private bool IsDuplicateAttendance(List<AttendanceLogMachineForm> existingAttendances, Guid employeeKey, InOutMode inOutMode, DateTime logTime)
    {
        const int timeThresholdSeconds = 60;
        return existingAttendances.Any(ea =>
            ea.EmployeeKey == employeeKey &&
            ea.InOutMode == inOutMode &&
            Math.Abs((ea.LogTime - logTime).TotalSeconds) <= timeThresholdSeconds);
    }

    private AttendanceLogMachineForm CreateAttendanceLogRecord(Guid employeeKey, DateTime logTime, InOutMode inOutMode)
    {
        return new AttendanceLogMachineForm
        {
            Key = Guid.NewGuid(),
            EmployeeKey = employeeKey,
            LogTime = logTime,
            InOutMode = inOutMode
        };
    }

    public async Task<List<AttendanceLogUserInfo>> ParseAttendanceLogFileAsync(Stream fileStream)
    {
        List<AttendanceLogUserInfo> result = new List<AttendanceLogUserInfo>();
        var companies = await _mediator.Send(new GetCompaniesQuery([]));
        var companyKeys = companies.Companies.Select(x => x.Key).Distinct().ToArray();
        var employeeAttendances = await _mediator.Send(new GetEmployeeAttendancesQuery([x => companyKeys.Contains(x.Employee.CompanyKey)]));

        try
        {
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string? line;
                while((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] parts = line.Split('\t');
                    if (parts.Length < 5)
                    {
                        _logger.LogWarning("Invalid line format: {line}", line);
                        continue;
                    }

                    string enrollNumber = parts[0];

                    var matchingEmployee = employeeAttendances.FirstOrDefault(ea => ea.FingerPrintID == enrollNumber);

                    if (!DateTime.TryParse(parts[1], out DateTime originalLogTime))
                    {
                        _logger.LogWarning("Invalid dateTime format : {date}", parts[1]);
                        continue;
                    }

                    DateOnly logDate = DateOnly.FromDateTime(originalLogTime);
                    TimeOnly logTime = TimeOnly.FromDateTime(originalLogTime);

                    if (!int.TryParse(parts[2], out int verifyMode))
                    {
                        _logger.LogWarning("Invalid verify mode : {verifyMode}", parts[2]);
                        continue;
                    }

                    if (!int.TryParse(parts[3], out int InOutModeInt))
                    {
                        _logger.LogWarning("Invalid in/out mode : {mode}", parts[3]);
                        continue;
                    }

                    if (!int.TryParse(parts[4], out int workCode))
                    {
                        _logger.LogWarning("Invalid work code : {code}", parts[4]);
                        continue;
                    }

                    result.Add(new AttendanceLogUserInfo
                    {
                        EnrollNumber = enrollNumber,
                        LogDate = logDate,
                        LogTime = logTime,
                        VerifyMode = verifyMode,
                        InOutMode = (InOutMode)InOutModeInt,
                        WorkCode = workCode,
                        EmployeeKey = matchingEmployee.Key,
                        EmployeeName = (matchingEmployee?.Employee?.FirstName ?? String.Empty) + " " + (matchingEmployee?.Employee?.LastName ?? String.Empty)
                    });
                }
            }

            _logger.LogInformation("Successfully parsed {count} attendance records from file", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing attendance log file");
            return new List<AttendanceLogUserInfo>();
        }
    }
}
