using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmshift", Schema = "Attendance")]
public class Shift : AuditTrail
{
    [Required]
    public Guid CompanyKey { get; set; }
    [Required]
    [StringLength(200)]
    public string ShiftGroupName { get; set; } = null!;
    public int? MaxLimit { get; set; } = 0;
    [StringLength(200)]
    public string? Description { get; set; } = String.Empty;

    [NotMapped]
    public Company? Company { get; set; }
    [NotMapped]
    public IEnumerable<ShiftDetail>? ShiftDetails { get; set; }

    public ShiftItemList ConvertToViewModelShiftItemList()
    {
        return new ShiftItemList
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            ShiftGroupName = this.ShiftGroupName,
            Company = this.Company
        };
    }

    public ShiftForm ConvertToViewModelShiftForm()
    {
        return new ShiftForm
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            ShiftGroupName = this.ShiftGroupName,
            Company = this.Company,
            ShiftDetails = this.ShiftDetails?.Select(sd => sd.ConvertToViewModelShiftDetailForm()),
            //JsonShiftDetails would typically be handled at the presentation layer
            JsonShiftDetails = string.Empty,
            MaxLimit = this.MaxLimit,
            Description = this.Description
        };
    }
}
