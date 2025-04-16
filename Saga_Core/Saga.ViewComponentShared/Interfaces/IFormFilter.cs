
using Saga.ViewComponentShared.ViewModels;

namespace Saga.ViewComponentShared.Interfaces;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>
public interface IFormFilter
{
    Task<BasicFormFilterViewModel> GetBasicFilterReportViewModelAsync();
    Task<YearlyFormFilterViewModel> GetYearlyFilterReportViewModelAsync();
    Task<MonthlyFormFilterViewModel> GetMonthlyFilterReportViewModelAsync();
    Task<DateRangeFormFilterViewModel> GetDateRangeFilterReportViewModelAsync();
    Task<DailyFormFilterViewModel> GetDailyFilterReportViewModelAsync();
}
