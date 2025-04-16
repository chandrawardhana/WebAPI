using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class AttendancePointListItem
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? Name { get; set; } = String.Empty;   
    public string? Description { get; set; } = String.Empty;
    public Double? Latitude { get; set; } = 0;
    public Double? Longitude { get; set; } = 0;
    public int? RangeTolerance { get; set; } = 0;
    public Company? Company { get; set; }
}

public class AttendancePointList
{
    public IEnumerable<AttendancePointListItem> AttendancePoints { get; set; } = new List<AttendancePointListItem>();
}

public class AttendancePointForm : AttendancePointListItem
{
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();

    public AttendancePointDto ConvertToAttendancePointDto()
    {
        return new AttendancePointDto
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            Code = this.Code,
            Name = this.Name,
            Description = this.Description,
            Latitude = this.Latitude,
            Longitude = this.Longitude,
            RangeTolerance = this.RangeTolerance
        };
    }
}
