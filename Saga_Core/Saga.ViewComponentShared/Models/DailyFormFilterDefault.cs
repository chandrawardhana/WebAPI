
namespace Saga.ViewComponentShared.Models;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class DailyFormFilterDefault : BasicFormFilterDefault
{
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}
