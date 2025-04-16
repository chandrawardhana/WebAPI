using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.ViewModels.Attendances;

public class OvertimeRateDetailForm
{
    public Guid Key { get; set; }
    public Guid? OvertimeRateKey { get; set; } = Guid.Empty;
    public int? Level { get; set; } = 0;
    public int? Hours { get; set; } = 0;
    public float? Multiply { get; set; } = 1.5f;
    public OvertimeRate? OvertimeRate { get; set; }

    public OvertimeRateDetailDto ConvertToOvertimeRateDetailDto()
    {
        return new OvertimeRateDetailDto
        {
            Key = this.Key,
            OvertimeRateKey = this.OvertimeRateKey ?? Guid.Empty,
            Level = this.Level ?? 0,
            Hours = this.Hours ?? 0,
            Multiply = this.Multiply ?? 1.5f
        };
    }
}
