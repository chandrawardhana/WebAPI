using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance
{
    [Table("tbtattendance", Schema = "Attendance")]
    public class Attendance : AuditTrail
    {
        [Required]
        public Guid EmployeeKey { get; set; }
        [Required]
        [StringLength(30)]
        public string EmployeeCode { get; set; } = null!;
        [Required]
        [StringLength(100)]
        public string EmployeeName { get; set; } = null!;
        [Required]
        public Guid CompanyKey { get; set; }
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = null!;
        [Required]
        public Guid OrganizationKey { get; set; }
        [Required]
        [StringLength(50)]
        public string OrganizationName { get; set; } = null!;
        [Required]
        public Guid PositionKey { get; set; }
        [Required]
        [StringLength(50)]
        public string PositionName { get; set; } = null!;
        [Required]
        public Guid TitleKey { get; set; }
        [Required]
        [StringLength(50)]
        public string TitleName { get; set; } = null!;
        [Required]
        public DateOnly AttendanceDate { get; set; }
        [Required]
        [StringLength(20)]
        public string AttendanceDay { get; set; } = null!;
        [Required]
        public TimeOnly In { get; set; }
        [Required]
        public TimeOnly Out { get; set; }
        [Required]
        [MaxLength(100)]
        public string ShiftName { get; set; } = null!;
        [Required]        
        public TimeOnly WorkingHour { get; set; }
        [Required]
        public TimeOnly NormalHour { get; set; }
        [Required]
        public AttendanceStatus Status { get; set; }
        [MaxLength(200)]
        public string? Description { get; set; } = String.Empty;
        public bool? IsMobileApp { get; set; } = false;
        public bool? IsFingerPrintMachine { get; set; } = false;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public TimeOnly? TotalLate { get; set; }
        public bool? IsLatePermit { get; set; } = false;
        public TimeOnly? TimeIn { get; set; }
        public bool? IsLateDocument { get; set; } = false;
        [MaxLength(200)]
        public string? LatePermitReason { get; set; } = String.Empty;
        public bool? IsLeaveSubmission { get; set; } = false;
        public Guid? LeaveKey { get; set; }
        [MaxLength(10)]
        public string? LeaveCode { get; set; }
        [MaxLength(100)]
        public string? LeaveName { get; set; }
        public DateTime? LeaveDateStart { get; set; }
        public DateTime? LeaveDateEnd { get; set; }
        [MaxLength(200)]
        public string? LeaveDescription { get; set; } = String.Empty;
        public bool? IsEarlyOutPermit { get; set; } = false;
        public TimeOnly? EarlyOutPermitTimeOut { get; set; }
        [MaxLength(200)]
        public string? EarlyOutReason { get; set; } = String.Empty;
        public bool? IsOutPermit { get; set; } = false;
        public TimeOnly? OutPermitTimeOut { get; set; }
        public TimeOnly? OutPermitBackToOffice { get; set; }
        [MaxLength(200)]
        public string? OutPermitReason { get; set; } = String.Empty;
        public bool? IsOvertimeLetter { get; set; } = false;
        public TimeOnly? OvertimeIn { get; set; }
        public TimeOnly? OvertimeOut { get; set; }
        public TimeOnly? RealOvertime { get; set; }
        public TimeOnly? AccumlativeOvertime { get; set; }
        [MaxLength(10)]
        public string? AttendanceCode { get; set; }
        public bool? IsAlpha { get; set; } = false;
        public int? CountAlpha { get; set; } = 0;
        [Required]
        public TimeOnly ShiftInTime { get; set; }

        [NotMapped]
        public Employee? Employee { get; set; }
        [NotMapped]
        public Company? Company { get; set; }
        [NotMapped]
        public Organization? Organization { get; set; }
        [NotMapped]
        public Position? Position { get; set; }
        [NotMapped]
        public Title? Title { get; set; }
        [NotMapped]
        public Leave? Leave { get; set; }

        public AttendanceForm ConvertToAttendanceFormViewModel()
        {
            return new AttendanceForm
            {
                Key = this.Key,
                EmployeeKey = this.EmployeeKey,
                AttendanceDate = this.AttendanceDate,
                In = this.In,
                Out = this.Out,
                ShiftName = this.ShiftName,
                Status = this.Status,
                Description = this.Description,
                IsMobileApp = this.IsMobileApp,
                IsFingerPrintMachine = this.IsFingerPrintMachine,
                Latitude = this.Latitude,
                Longitude = this.Longitude,
                Employee = this.Employee
            };
        }
    }
}
