
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Enums;
using Saga.DomainShared.Extensions;
using Saga.Mediator.Employees.EmployeeMediator;
using Saga.Mediator.Organizations.CompanyMediator;
using Saga.Mediator.Organizations.OrganizationMediator;
using Saga.Mediator.Organizations.PositionMediator;
using Saga.Mediator.Organizations.TitleMediator;
using Saga.ViewComponentShared.Interfaces;
using Saga.ViewComponentShared.ViewModels;

namespace Saga.ViewComponentShared.Services;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class FormFilterService(IMediator _mediator) : IFormFilter
{
    public async Task<BasicFormFilterViewModel> GetBasicFilterReportViewModelAsync()
    {
        BasicFormFilterViewModel model = new();
        model.Employees = await GetEmployees();
        model.Companies = await GetCompanies();
        model.Organizations = await GetOrganizations();
        model.Positions = await GetPositions();
        model.Titles = await GetTitles();

        return await Task.FromResult(model);
    }

    public async Task<YearlyFormFilterViewModel> GetYearlyFilterReportViewModelAsync()
    {
        var basic = await GetBasicFilterReportViewModelAsync();

        YearlyFormFilterViewModel model = new()
        {
            Employees = basic.Employees,
            Organizations = basic.Organizations,
            Companies = basic.Companies,
            Positions = basic.Positions,
            Titles = basic.Titles
        };

        // year
        var years = Enumerable.Range(DateTime.Now.Year - 4, 5);
        model.YearPeriodes = years.OrderDescending().Select(x => new SelectListItem(x.ToString(), x.ToString()));

        return await Task.FromResult(model);
    }

    public async Task<MonthlyFormFilterViewModel> GetMonthlyFilterReportViewModelAsync()
    {
        var basic = await GetBasicFilterReportViewModelAsync();

        MonthlyFormFilterViewModel model = new()
        {
            Employees = basic.Employees,
            Organizations = basic.Organizations,
            Companies = basic.Companies,
            Positions = basic.Positions,
            Titles = basic.Titles
        };

        // month
        var months = Enumerable.Range(1, 12);
        model.MonthPeriodes = months.Select(x =>
        {
            MonthName month = (MonthName)x;
            return new SelectListItem(month.GetDisplayName(), x.ToString());
        });

        // year
        var years = Enumerable.Range(DateTime.Now.Year - 4, 5);
        model.YearPeriodes = years.OrderDescending().Select(x => new SelectListItem(x.ToString(), x.ToString()));

        return await Task.FromResult(model);
    }

    public async Task<DateRangeFormFilterViewModel> GetDateRangeFilterReportViewModelAsync()
    {
        var basic = await GetBasicFilterReportViewModelAsync();

        DateRangeFormFilterViewModel model = new()
        {
            Employees = basic.Employees,
            Organizations = basic.Organizations,
            Companies = basic.Companies,
            Positions = basic.Positions,
            Titles = basic.Titles
        };

        return await Task.FromResult(model);
    }

    public async Task<DailyFormFilterViewModel> GetDailyFilterReportViewModelAsync()
    {
        var basic = await GetBasicFilterReportViewModelAsync();

        DailyFormFilterViewModel model = new()
        {
            Employees = basic.Employees,
            Organizations = basic.Organizations,
            Companies = basic.Companies,
            Positions = basic.Positions,
            Titles = basic.Titles
        };

        return await Task.FromResult(model);
    }

    private async Task<IEnumerable<SelectListItem>> GetEmployees()
    {
        var employees = (await _mediator.Send(new GetEmployeesQuery([]))).Employees
                        .OrderBy(x => x.FullName)
                        .Select(x => new SelectListItem(x.FullName, x.Key.ToString()))
                        .ToList();
        employees.Insert(0, new SelectListItem("All", Guid.Empty.ToString()));

        return await Task.FromResult(employees);
    }

    private async Task<IEnumerable<SelectListItem>> GetCompanies()
    {
        // company
        var companies = (await _mediator.Send(new GetCompaniesQuery([]))).Companies
                        .OrderBy(x => x.Name)
                        .Select(x => new SelectListItem(string.Format("{0} ({1})", x.Code, x.Name), x.Key.ToString()))
                        .ToList();
        companies.Insert(0, new SelectListItem("All", Guid.Empty.ToString()));
        return await Task.FromResult(companies);
    }

    private async Task<IEnumerable<SelectListItem>> GetOrganizations()
    {
        var orgs = (await _mediator.Send(new GetOrganizationsQuery([]))).Organizations
                        .OrderBy(x => x.Name)
                        .Select(x => new SelectListItem(string.Format("{0} ({1})", x.Code, x.Name), x.Key.ToString()))
                        .ToList();
        orgs.Insert(0, new SelectListItem("All", Guid.Empty.ToString()));
        return await Task.FromResult(orgs);
    }

    private async Task<IEnumerable<SelectListItem>> GetPositions()
    {
        var positions = (await _mediator.Send(new GetPositionsQuery([]))).Positions
                        .OrderBy(x => x.Name)
                        .Select(x => new SelectListItem(string.Format("{0} ({1})", x.Code, x.Name), x.Key.ToString()))
                        .ToList();
        positions.Insert(0, new SelectListItem("All", Guid.Empty.ToString()));
        return await Task.FromResult(positions);
    }

    private async Task<IEnumerable<SelectListItem>> GetTitles()
    {
        var titles = (await _mediator.Send(new GetTitlesQuery([]))).Titles
                        .OrderBy(x => x.Name)
                        .Select(x => new SelectListItem(string.Format("{0} ({1})", x.Code, x.Name), x.Key.ToString()))
                        .ToList();
        titles.Insert(0, new SelectListItem("All", Guid.Empty.ToString()));
        return await Task.FromResult(titles);
    }
}
