using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmattendancepoint", Schema = "Attendance")]
public class AttendancePoint : AuditTrail
{
    [Required]
    public Guid CompanyKey { get; set; }
    [Required]
    [StringLength(10)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;
    [Required]
    public Double Latitude { get; set; }
    [Required]
    public Double Longitude { get; set; }
    [Required]
    public int RangeTolerance { get; set; }

    [NotMapped]
    public Company? Company { get; set; }

    public AttendancePointForm ConvertToViewModelAttendancePoint()
    {
        return new AttendancePointForm
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description,
            Latitude = this.Latitude,
            Longitude = this.Longitude,
            RangeTolerance = this.RangeTolerance,
            Company = this.Company
        };
    }
}
