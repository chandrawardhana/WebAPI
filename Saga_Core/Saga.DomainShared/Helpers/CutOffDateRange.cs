using Saga.Domain.ViewModels.Attendances;
using System.Globalization;

namespace Saga.DomainShared.Helpers;

public static class CutOffDateRange
{
    public static (int startDay, int endDay) GetCutOffDays(CutOffListItem cutoff, int selectedMonth)
    {
        return selectedMonth switch
        {
            1 => (cutoff.JanStart ?? 0, cutoff.JanEnd ?? 0),
            2 => (cutoff.FebStart ?? 0, cutoff.FebEnd ?? 0),
            3 => (cutoff.MarStart ?? 0, cutoff.MarEnd ?? 0),
            4 => (cutoff.AprStart ?? 0, cutoff.AprEnd ?? 0),
            5 => (cutoff.MayStart ?? 0, cutoff.MayEnd ?? 0),
            6 => (cutoff.JunStart ?? 0, cutoff.JunEnd ?? 0),
            7 => (cutoff.JulStart ?? 0, cutoff.JulEnd ?? 0),
            8 => (cutoff.AugStart ?? 0, cutoff.AugEnd ?? 0),
            9 => (cutoff.SepStart ?? 0, cutoff.SepEnd ?? 0),
            10 => (cutoff.OctStart ?? 0, cutoff.OctEnd ?? 0),
            11 => (cutoff.NovStart ?? 0, cutoff.NovEnd ?? 0),
            12 => (cutoff.DecStart ?? 0, cutoff.DecEnd ?? 0),
            _ => throw new ArgumentException("Invalid month specified")
        };
    }

    public static (DateOnly StartDate, DateOnly EndDate) CalculateCutOffDateRange(int selectedMonth, int selectedYear, int startDay, int endDay)
    {
        var startYear = selectedMonth == 1 ? selectedYear - 1 : selectedYear;
        var startMonth = selectedMonth == 1 ? 12 : selectedMonth - 1;

        // Create DateOnly objects with invariant culture formatting
        var startDate = DateOnly.Parse($"{startYear}-{startMonth:00}-{startDay:00}", CultureInfo.InvariantCulture);
        var endDate = DateOnly.Parse($"{selectedYear}-{selectedMonth:00}-{endDay:00}", CultureInfo.InvariantCulture);

        return (startDate, endDate);
    }

    public static string ToInvariantString(this DateOnly date)
    {
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
