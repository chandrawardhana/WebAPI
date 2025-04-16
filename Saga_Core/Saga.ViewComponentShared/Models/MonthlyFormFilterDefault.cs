
namespace Saga.ViewComponentShared.Models;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class MonthlyFormFilterDefault : BasicFormFilterDefault
{
    public int MonthSelected { get; set; } = DateTime.Now.Month;
    public int YearSelected { get; set; } = DateTime.Now.Year;
}
