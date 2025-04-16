
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.ViewComponentShared.Dtos;
using Saga.ViewComponentShared.Models;

namespace Saga.ViewComponentShared.ViewModels;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class BasicFormFilterViewModel : BasicFormFilterDefault
{
    public IEnumerable<SelectListItem> Employees { get; set; } = [];
    public IEnumerable<SelectListItem> Companies { get; set; } = [];
    public IEnumerable<SelectListItem> Organizations { get; set; } = [];
    public IEnumerable<SelectListItem> Positions { get; set; } = [];
    public IEnumerable<SelectListItem> Titles { get; set; } = [];
    

    public void SetDefaultValue(BasicFormFilterDefault def)
    {
        EmployeeSelected = def.EmployeeSelected;
        CompanySelected = def.CompanySelected;
        OrganizationSelected = def.OrganizationSelected;
        PositionSelected = def.PositionSelected;
        TitleSelected = def.TitleSelected;
    }

    public void SetSelectedValue(BasicFormFilterDto dto)
    {
        EmployeeSelected = dto.EmployeeSelected;
        CompanySelected = dto.CompanySelected;
        OrganizationSelected = dto.OrganizationSelected;
        PositionSelected = dto.PositionSelected;
        TitleSelected = dto.TitleSelected;
    }
    public BasicFormFilterDefault ConvertToDefault()
        => new()
        {
            EmployeeSelected = EmployeeSelected,
            CompanySelected = CompanySelected,
            OrganizationSelected = OrganizationSelected,
            PositionSelected = PositionSelected,
            TitleSelected = TitleSelected
        };
}
