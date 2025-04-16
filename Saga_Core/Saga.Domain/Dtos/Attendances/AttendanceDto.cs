using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class AttendanceDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public DateOnly? AttendanceDate { get; set; }
    public TimeOnly? In { get; set; }
    public TimeOnly? Out { get; set; }
    public String? ShiftName { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Description { get; set; } = String.Empty;
    public bool? IsMobileApp { get; set; } = false;
    public bool? IsFingerPrintMachine { get; set; } = false;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public Attendance ConvertToEntity()
    {
        return new Attendance
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            AttendanceDate = this.AttendanceDate ?? DateOnly.FromDateTime(DateTime.Now),
            In = this.In ?? TimeOnly.MinValue,
            Out = this.Out ?? TimeOnly.MinValue,
            ShiftName = this.ShiftName ?? String.Empty,
            Status = this.Status,
            Description = this.Description ?? String.Empty,
            IsMobileApp = this.IsMobileApp ?? false,
            IsFingerPrintMachine = this.IsFingerPrintMachine ?? false,
            Latitude = this.Latitude ?? 0,
            Longitude = this.Longitude ?? 0
        };
    }

}

public class GeneralAttendanceReportDto
{
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public Guid? OrganizationKey { get; set; } = Guid.Empty;
    public Guid? PositionKey { get; set; } = Guid.Empty;
    public Guid? TitleKey { get; set; } = Guid.Empty;
}

public class AttendanceDailyReportDto : GeneralAttendanceReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Xlsx;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class AttendanceWeeklyReportDto : GeneralAttendanceReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Xlsx;
    public DateOnly? StartDate { get; set; }
}

public class AttendanceMonthlyReportDto : GeneralAttendanceReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Xlsx;
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
}

public class AttendanceRecapitulationReportDto : GeneralAttendanceReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Xlsx;
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
}

public class PermitDetailReportDto : GeneralAttendanceReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Xlsx;
    public PermitReportCategory Category { get; set; } = PermitReportCategory.EarlyOutPermit;
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
}

public class ShiftScheduleDetailReportDto : GeneralAttendanceReportDto
{
    public DocumentGeneratorFormat? DocumentGeneratorFormat { get; set; } = Enums.DocumentGeneratorFormat.Xlsx;
    public int? SelectedMonth { get; set; }
    public int? SelectedYear { get; set; }
}
