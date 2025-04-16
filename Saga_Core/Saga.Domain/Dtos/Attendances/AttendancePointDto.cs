using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class AttendancePointDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? Name { get; set; } = String.Empty;
    public string? Description { get; set; } = String.Empty;
    public Double? Latitude { get; set; } = 0;
    public Double? Longitude { get; set; } = 0;
    public int? RangeTolerance { get; set; } = 0;

    public AttendancePoint ConvertToEntity()
    {
        return new AttendancePoint
        {
            Key = this.Key ?? Guid.Empty,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            Code = this.Code ?? String.Empty,
            Name = this.Name ?? String.Empty,
            Description = this.Description ?? String.Empty,
            Latitude = this.Latitude ?? 0,
            Longitude = this.Longitude ?? 0,
            RangeTolerance = this.RangeTolerance ?? 0
        };
    }
}
