
namespace Saga.ViewComponentShared.Models;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class DateRangeFormFilterDefault : BasicFormFilterDefault
{
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-14));
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}
