
using Saga.Domain.Enums;
using System.Globalization;

namespace Saga.DomainShared.Constants;

public static class AppGlobalization
{
    public static DayOfWeek[] Weekdays => [
        DayOfWeek.Monday, 
        DayOfWeek.Tuesday, 
        DayOfWeek.Wednesday, 
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    ];
    public static string[] MonthNames 
        => ((new CultureInfo("en-US")).DateTimeFormat.MonthNames)
            .Where(x => !string.IsNullOrEmpty(x)).ToArray();
    public static int[] GetYearPeriod(int yearStart, int range = 1) => Enumerable.Range(yearStart, range).ToArray();

    public static DayOfWeek CurrentDay => DateTime.Now.DayOfWeek;
    public static int CurrentDate => DateTime.Now.Day;
    public static int CurrentMonth => DateTime.Now.Month;
    public static int CurrentYear => DateTime.Now.Year;

    public static string CultureUS => "en-US";
    public static string CultureId => "id";
    public static string CultureKorea => "ko";
}
