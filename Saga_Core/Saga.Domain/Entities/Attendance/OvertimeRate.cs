using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmovertimerate", Schema = "Attendance")]
public class OvertimeRate : AuditTrail
{
    [Required]
    public Guid CompanyKey { get; set; }
    [Required]
    [StringLength(200)]
    public string GroupName { get; set; } = null!;
    [Required]
    public Day BaseOnDay { get; set; }
    [Required]
    public int MaxHour { get; set; }

    [NotMapped]
    public Company? Company { get; set; }
    [NotMapped]
    public IEnumerable<OvertimeRateDetail>? OvertimeRateDetails { get; set; }

    public OvertimeRateItemList ConvertToViewModelOvertimeRateItemList()
    {
        return new OvertimeRateItemList
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            GroupName = this.GroupName,
            BaseOnDay = this.BaseOnDay,
            MaxHour = this.MaxHour,
            Company = this.Company
        };
    }

    public OvertimeRateForm ConvertToViewModelOvertimeRateForm()
    {
        return new OvertimeRateForm
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            GroupName = this.GroupName,
            BaseOnDay = this.BaseOnDay,
            MaxHour = this.MaxHour,
            OvertimeRateDetails = this.OvertimeRateDetails?.Select(x => x.ConvertToViewModelOvertimeRateDetailForm()).ToList(),
            JsonOvertimeRateDetails = String.Empty
        };
    }
}
