using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class ShiftDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? ShiftGroupName { get; set; } = String.Empty;
    public int? MaxLimit { get; set; } = 0;
    public string? Description { get; set; } = String.Empty;
    public List<ShiftDetailDto>? ShiftDetails { get; set; } = [];

    public Shift ConvertToEntity()
    {
        return new Shift
        {
            Key = this.Key ?? Guid.Empty,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            ShiftGroupName = this.ShiftGroupName ?? String.Empty,
            MaxLimit = this.MaxLimit,
            Description = this.Description,
            ShiftDetails = this.ShiftDetails?.Select(sd => sd.ConvertToEntity()).ToList()
        };
    }
}
