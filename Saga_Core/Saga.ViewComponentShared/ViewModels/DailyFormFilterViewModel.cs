
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.ViewComponentShared.Dtos;
using Saga.ViewComponentShared.Models;

namespace Saga.ViewComponentShared.ViewModels;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class DailyFormFilterViewModel : BasicFormFilterViewModel
{
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public void SetDefaultValue(DailyFormFilterDefault def)
    {
        EmployeeSelected = def.EmployeeSelected;
        CompanySelected = def.CompanySelected;
        OrganizationSelected = def.OrganizationSelected;
        PositionSelected = def.PositionSelected;
        TitleSelected = def.TitleSelected;
        StartDate = def.StartDate;
    }

    public void SetSelectedValue(DailyFormFilterDto dto)
    {
        EmployeeSelected = dto.EmployeeSelected;
        CompanySelected = dto.CompanySelected;
        OrganizationSelected = dto.OrganizationSelected;
        PositionSelected = dto.PositionSelected;
        TitleSelected = dto.TitleSelected;
        StartDate = dto.StartDate;
    }
    public DailyFormFilterDefault ConvertToDefault()
        => new()
        {
            EmployeeSelected = EmployeeSelected,
            CompanySelected = CompanySelected,
            OrganizationSelected = OrganizationSelected,
            PositionSelected = PositionSelected,
            TitleSelected = TitleSelected,
            StartDate = StartDate
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
