using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Dtos.Attendances;

public class OvertimeRateDetailDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? OvertimeRateKey { get; set; } = Guid.Empty;
    public int? Level { get; set; } = 0;
    public int? Hours { get; set; } = 0;
    public float? Multiply { get; set; } = 1.5f;

    public OvertimeRateDetail ConvertToEntity()
    {
        return new OvertimeRateDetail
        {
            Key = this.Key ?? Guid.Empty,
            OvertimeRateKey = this.OvertimeRateKey ?? Guid.Empty,
            Level = this.Level ?? 0,
            Hours = this.Hours ?? 0,
            Multiply = this.Multiply ?? 1.5f
        };
    }

    public OvertimeRateDetailForm ConvertToModelView()
        => new()
        {
            Key = this.Key ?? Guid.Empty,
            OvertimeRateKey = this.OvertimeRateKey ?? Guid.Empty,
            Level = this.Level ?? 0,
            Hours = this.Hours ?? 0,
            Multiply = this.Multiply ?? 1.5f
        };
}
