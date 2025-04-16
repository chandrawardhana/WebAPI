using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmshiftdetail", Schema = "Attendance")]
public class ShiftDetail : AuditTrail
{
    [Required]
    public Guid ShiftKey { get; set; }
    [Required]
    public Day Day { get; set; }
    [Required]
    [StringLength(50)]
    public string WorkName { get; set; } = null!;
    [Required]
    public WorkType WorkType { get; set; }
    [Required]
    [StringLength(5)]
    public string In { get; set; } = null!;
    [Required]
    [StringLength(5)]
    public string Out { get; set; } = null!;
    [StringLength(5)]
    public string? EarlyIn { get; set; } = string.Empty;
    [StringLength(5)]
    public string? MaxOut { get; set; } = string.Empty;
    [Required]
    public int LateTolerance { get; set; }
    public bool? IsCutBreak { get; set; } = false;
    public bool? IsNextDay { get; set; } = false;

    [NotMapped]
    public Shift? Shift { get; set; }

    public ShiftDetailForm ConvertToViewModelShiftDetailForm()
    {
        return new ShiftDetailForm
        {
            Key = this.Key,
            ShiftKey = this.ShiftKey,
            Day = this.Day,
            WorkName = this.WorkName,
            WorkType = this.WorkType,
            In = this.In,
            Out = this.Out,
            EarlyIn = this.EarlyIn,
            MaxOut = this.MaxOut,
            IsCutBreak = this.IsCutBreak,
            IsNextDay = this.IsNextDay,
            Shift = this.Shift
        };
    }

    public ShiftDetailItemList ConvertToViewModelShiftDetailItemList()
    {
        return new ShiftDetailItemList
        {
            Key = this.Key,
            ShiftKey = this.ShiftKey,
            Day = this.Day,
            WorkName = this.WorkName,
            WorkType = this.WorkType,
            In = this.In,
            Out = this.Out,
            EarlyIn = this.EarlyIn,
            MaxOut = this.MaxOut,
            Shift = this.Shift
        };
    }

    public ShiftWorkNameItemList ConvertToViewModelShiftWorkNameItemList()
    {
        return new ShiftWorkNameItemList
        {
            ShiftDetailKey = this.Key,
            WorkName = this.WorkName
        };
    }
}
