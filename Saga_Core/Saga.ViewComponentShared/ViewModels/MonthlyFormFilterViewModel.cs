
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.ViewComponentShared.Dtos;
using Saga.ViewComponentShared.Models;

namespace Saga.ViewComponentShared.ViewModels;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class MonthlyFormFilterViewModel : BasicFormFilterViewModel
{
    public int MonthSelected { get; set; } = DateTime.Now.Month;
    public int YearSelected { get; set; } = DateTime.Now.Year;
    public IEnumerable<SelectListItem> MonthPeriodes { get; set; } = [];
    public IEnumerable<SelectListItem> YearPeriodes { get; set; } = [];

    public void SetDefaultValue(MonthlyFormFilterDefault def)
    {
        EmployeeSelected = def.EmployeeSelected;
        CompanySelected = def.CompanySelected;
        OrganizationSelected = def.OrganizationSelected;
        PositionSelected = def.PositionSelected;
        TitleSelected = def.TitleSelected;
        MonthSelected = def.MonthSelected;
        YearSelected = def.YearSelected;
    }

    public void SetSelectedValue(MonthlyFormFilterDto dto)
    {
        EmployeeSelected = dto.EmployeeSelected;
        CompanySelected = dto.CompanySelected;
        OrganizationSelected = dto.OrganizationSelected;
        PositionSelected = dto.PositionSelected;
        TitleSelected = dto.TitleSelected;
        MonthSelected = dto.MonthSelected;
        YearSelected = dto.YearSelected;
    }
    public MonthlyFormFilterDefault ConvertToDefault()
        => new()
        {
            EmployeeSelected = EmployeeSelected,
            CompanySelected = CompanySelected,
            OrganizationSelected = OrganizationSelected,
            PositionSelected = PositionSelected,
            TitleSelected = TitleSelected,
            MonthSelected = MonthSelected,
            YearSelected = YearSelected
        };

    public BasicFormFilterDefault ConvertToBasicDefault()
        => new()
        {
            EmployeeSelected = EmployeeSelected,
            CompanySelected = CompanySelected,
            OrganizationSelected = OrganizationSelected,
            PositionSelected = PositionSelected,
            TitleSelected = TitleSelected
        };
}
