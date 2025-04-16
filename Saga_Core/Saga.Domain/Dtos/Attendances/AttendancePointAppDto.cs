
using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class AttendancePointAppDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid EmployeeKey { get; set; }
    public Double Latitude { get; set; }
    public Double Longitude { get; set; }
    public InOutMode InOutMode { get; set; }
    public DateTime AbsenceTime { get; set; }

    public AttendancePointApp ConvertToEntity()
    {
        return new AttendancePointApp
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            Latitude = this.Latitude,
            Longitude = this.Longitude,
            InOutMode = this.InOutMode,
            AbsenceTime = this.AbsenceTime,
        };
    }
}

public class RetrieveAttendancePointDto
{
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public DateOnly? DateStart { get; set; }
    public DateOnly? DateEnd { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}

public class AttendancePointFilterFormDto
{
    public Guid CompanyKey { set; get; } = Guid.Empty;
    public Guid EmployeeKey { set; get; } = Guid.Empty;
    public DateOnly StartDate { set; get; }
    public DateOnly EndDate { set; get; }
}
