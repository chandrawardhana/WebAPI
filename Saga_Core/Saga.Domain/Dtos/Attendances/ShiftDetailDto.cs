using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Dtos.Attendances;

public class ShiftDetailDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? ShiftKey { get; set; } = Guid.Empty;
    public Day Day { get; set; }
    public string? WorkName { get; set; } = String.Empty;
    public WorkType WorkType { get; set; }
    public string? In { get; set; } = String.Empty;
    public string? Out { get; set; } = String.Empty;
    public string? EarlyIn { get; set; } = String.Empty;
    public string? MaxOut { get; set; } = String.Empty;
    public int? LateTolerance { get; set; } = 0;
    public bool? IsCutBreak { get; set; } = false;
    public bool? IsNextDay { get; set; } = false;

    public ShiftDetail ConvertToEntity() 
    {
        return new ShiftDetail
        {
            Key = this.Key ?? Guid.Empty,
            ShiftKey = this.ShiftKey ?? Guid.Empty,
            Day = this.Day,
            WorkName = this.WorkName ?? String.Empty,
            WorkType = this.WorkType,
            In = this.In ?? String.Empty,
            Out = this.Out ?? String.Empty,
            EarlyIn = this.EarlyIn ?? String.Empty,
            MaxOut = this.MaxOut ?? String.Empty,
            LateTolerance = this.LateTolerance ?? 0,
            IsCutBreak = this.IsCutBreak ?? false,
            IsNextDay = this.IsNextDay ?? false
        };
    }

    public ShiftDetailForm ConvertToModelView()
        => new ()
        {
            Key = this.Key ?? Guid.Empty,
            ShiftKey = this.ShiftKey ?? Guid.Empty,
            Day = this.Day,
            WorkName = this.WorkName ?? String.Empty,
            WorkType = this.WorkType,
            In = this.In ?? String.Empty,
            Out = this.Out ?? String.Empty,
            EarlyIn = this.EarlyIn ?? String.Empty,
            MaxOut = this.MaxOut ?? String.Empty,
            LateTolerance = this.LateTolerance ?? 0,
            IsCutBreak = this.IsCutBreak ?? false,
            IsNextDay = this.IsNextDay ?? false
        };
}
