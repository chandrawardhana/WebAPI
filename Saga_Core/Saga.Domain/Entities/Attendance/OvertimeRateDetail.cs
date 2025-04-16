using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmovertimeratedetail", Schema = "Attendance")]
public class OvertimeRateDetail : AuditTrail
{
    [Required]
    public Guid OvertimeRateKey { get; set; }
    [Required]
    public int Level { get; set; }
    [Required]
    public int Hours { get; set; }
    [Required]
    public float Multiply { get; set; }

    [NotMapped]
    public OvertimeRate? OvertimeRate { get; set; }

    public OvertimeRateDetailForm ConvertToViewModelOvertimeRateDetailForm()
    {
        return new OvertimeRateDetailForm
        {
            Key = this.Key,
            OvertimeRateKey = this.OvertimeRateKey,
            Level = this.Level,
            Hours = this.Hours,
            Multiply = this.Multiply,
            OvertimeRate = this.OvertimeRate
        };
    }
}
