
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.ViewComponentShared.Dtos;
using Saga.ViewComponentShared.Models;

namespace Saga.ViewComponentShared.ViewModels;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class YearlyFormFilterViewModel : BasicFormFilterViewModel
{
    public int YearSelected { get; set; } = DateTime.Now.Year;
    public IEnumerable<SelectListItem> YearPeriodes { get; set; } = [];

    public void SetDefaultValue(YearlyFormFilterDefault def)
    {
        EmployeeSelected = def.EmployeeSelected;
        CompanySelected = def.CompanySelected;
        OrganizationSelected = def.OrganizationSelected;
        PositionSelected = def.PositionSelected;
        TitleSelected = def.TitleSelected;
        YearSelected = def.YearSelected;
    }

    public void SetSelectedValue(YearlyFormFilterDto dto)
    {
        EmployeeSelected = dto.EmployeeSelected;
        CompanySelected = dto.CompanySelected;
        OrganizationSelected = dto.OrganizationSelected;
        PositionSelected = dto.PositionSelected;
        TitleSelected = dto.TitleSelected;
        YearSelected = dto.YearSelected;
    }
    public YearlyFormFilterDefault ConvertToDefault()
        => new()
        {
            EmployeeSelected = EmployeeSelected,
            CompanySelected = CompanySelected,
            OrganizationSelected = OrganizationSelected,
            PositionSelected = PositionSelected,
            TitleSelected = TitleSelected,
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
