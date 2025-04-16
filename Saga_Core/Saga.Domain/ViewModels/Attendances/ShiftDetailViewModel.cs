using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.ViewModels.Attendances;

public class ShiftDetailItemList
{
    public Guid Key { get; set; }
    public Guid? ShiftKey { get; set; } = Guid.Empty;
    public Day? Day { get; set; } = null;
    public string? WorkName { get; set; } = String.Empty;
    public WorkType? WorkType { get; set; } = null;
    public string? In { get; set; } = String.Empty;
    public string? Out { get; set; } = String.Empty;
    public string? EarlyIn { get; set; } = String.Empty;
    public string? MaxOut { get; set; } = String.Empty;
    public int? LateTolerance { get; set; } = 0;
    public Shift? Shift { get; set; }
}

public class ShiftDetailList
{
    public IEnumerable<ShiftDetailItemList> ShiftDetails { get; set; } = [];
}

public class ShiftDetailForm : ShiftDetailItemList
{
    public bool? IsCutBreak { get; set; } = false;
    public bool? IsNextDay { get; set; } = false;

    public ShiftDetailDto ConvertToShiftDetailDto()
    {
        return new ShiftDetailDto
        {
            Key = this.Key,
            ShiftKey = this.ShiftKey ?? Guid.Empty,
            Day = this.Day ?? Enums.Day.Monday,
            WorkName = this.WorkName ?? String.Empty,
            WorkType = this.WorkType ?? Enums.WorkType.Work,
            In = this.In ?? String.Empty,
            Out = this.Out ?? String.Empty,
            EarlyIn = this.EarlyIn ?? String.Empty,
            MaxOut = this.MaxOut ?? String.Empty,
            LateTolerance = this.LateTolerance ?? 0,
            IsCutBreak = this.IsCutBreak ?? false,
            IsNextDay = this.IsNextDay ?? false
        };
    }
}

public class ShiftWorkNameItemList
{
    public Guid ShiftDetailKey { get; set; }
    public string? WorkName { get; set; } = String.Empty;
}

public class ShiftWorkNameList
{
    public IEnumerable<ShiftWorkNameItemList>? ShiftWorkNames { get; set; } = new List<ShiftWorkNameItemList>();
}
