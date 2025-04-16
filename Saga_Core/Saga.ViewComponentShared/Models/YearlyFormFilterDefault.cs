
namespace Saga.ViewComponentShared.Models;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class YearlyFormFilterDefault : BasicFormFilterDefault
{
    public int YearSelected { get; set; } = DateTime.Now.Year;
}
