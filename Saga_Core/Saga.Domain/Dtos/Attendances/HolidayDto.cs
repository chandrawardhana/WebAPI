using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class HolidayDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public string? Name { get; set; } = String.Empty;
    public int? Duration { get; set; } = 0;
    public string? Description { get; set; } = String.Empty;
    public Guid[]? CompanyKeys { get; set; } 
    public DateOnly? DateEvent { get; set; }
    public Guid[]? ExistingCompanies { get; set; }

    public Holiday ConvertToEntity()
    {
        return new Holiday 
        {
            Key = this.Key ?? Guid.Empty,
            Name = this.Name ?? String.Empty,
            Duration = this.Duration ?? 0,
            Description = this.Description ?? String.Empty,
            CompanyKeys = this.CompanyKeys ?? this.ExistingCompanies ?? Array.Empty<Guid>(),
            DateEvent = this.DateEvent ?? DateOnly.FromDateTime(DateTime.Now)
        };
    }
}
