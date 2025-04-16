using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Dtos.Attendances;

public class AttendanceLogDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid EmployeeKey { get; set; }
    public DateTime LogTime { get; set; }
    public InOutMode InOutMode { get; set; }

    public AttendanceLogMachine ConvertToEntity()
    {
        return new AttendanceLogMachine
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            LogTime = this.LogTime,
            InOutMode = this.InOutMode,
        };
    }
}

public class AttendanceLogMachineDto
{
    public List<AttendanceLogDto> AttendanceLogMachines { get; set; } = new List<AttendanceLogDto>();
}

public class ImportFingerLogDto
{
    public List<AttendanceLogUserInfo> AttendanceLogUserInfos { get; set; } = new List<AttendanceLogUserInfo>();

    public AttendanceLogMachineDto ConvertToAttendanceLogMachineDto()
    {
        var dto = new AttendanceLogMachineDto();
        var groupedByEmployeeAndDate = AttendanceLogUserInfos.Where(x => x.EmployeeKey != Guid.Empty)
                                                             .GroupBy(x => new
                                                             {
                                                                 x.EmployeeKey,
                                                                 x.LogDate
                                                             });

        foreach (var group in groupedByEmployeeAndDate)
        {
            dto.AttendanceLogMachines.Add(new AttendanceLogDto
            {
                Key = Guid.NewGuid(),
                EmployeeKey = group.Key.EmployeeKey,
                LogTime = CombineDateAndTime(group.Key.LogDate, group.First().LogTime),
                InOutMode = InOutMode.CheckIn
            });
        }

        return dto;
    }

    private DateTime CombineDateAndTime(DateOnly date, TimeOnly time)
    {
        return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
    }
}
