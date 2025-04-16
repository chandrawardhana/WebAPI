using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class OvertimeRateDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? GroupName { get; set; } = String.Empty;
    public Day BaseOnDay { get; set; }
    public int? MaxHour { get; set; } = 0;
    public List<OvertimeRateDetailDto>? OvertimeRateDetails { get; set; } = [];

    public OvertimeRate ConvertToEntity()
    {
        return new OvertimeRate
        {
            Key = this.Key ?? Guid.Empty,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            GroupName = this.GroupName ?? String.Empty,
            BaseOnDay = this.BaseOnDay,
            MaxHour = this.MaxHour ?? 0,
            OvertimeRateDetails = this.OvertimeRateDetails?.Select(x => x.ConvertToEntity()).ToList()
        };
    }
}
