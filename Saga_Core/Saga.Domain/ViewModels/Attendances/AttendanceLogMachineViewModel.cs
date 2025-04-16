
using Microsoft.AspNetCore.Http;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class AttendanceLogMachineForm
{
    public Guid Key { get; set; }
    public Guid EmployeeKey { get; set; }
    public DateTime LogTime { get; set; }
    public InOutMode InOutMode { get; set; }
    public Employee Employee { get; set; } = null!;

    public AttendanceLogDto ConvertToAttendanceLogDto()
    {
        return new AttendanceLogDto
        {
            Key = Key,
            EmployeeKey = EmployeeKey,
            LogTime = LogTime,
            InOutMode = InOutMode
        };
    }
}

public class ImportFingerLog
{
    public IFormFile? UploadFile { get; set; }
}

public class AttendanceLogUserInfo
{
    public string EnrollNumber { get; set; } = string.Empty; //FingerPrint ID
    public DateOnly LogDate { get; set; }
    public TimeOnly LogTime { get; set; }
    public int VerifyMode { get; set; }
    public InOutMode InOutMode { get; set; }
    public int WorkCode { get; set; }
    public Guid EmployeeKey { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
}
