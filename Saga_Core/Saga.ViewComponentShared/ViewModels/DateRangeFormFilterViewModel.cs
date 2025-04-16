
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.ViewComponentShared.Dtos;
using Saga.ViewComponentShared.Models;

namespace Saga.ViewComponentShared.ViewModels;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public class DateRangeFormFilterViewModel : BasicFormFilterViewModel
{
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-14));
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public void SetDefaultValue(DateRangeFormFilterDefault def)
    {
        EmployeeSelected = def.EmployeeSelected;
        CompanySelected = def.CompanySelected;
        OrganizationSelected = def.OrganizationSelected;
        PositionSelected = def.PositionSelected;
        TitleSelected = def.TitleSelected;
        StartDate = def.StartDate;
        EndDate = def.EndDate;
    }

    public void SetSelectedValue(DateRangeFormFilterDto dto)
    {
        EmployeeSelected = dto.EmployeeSelected;
        CompanySelected = dto.CompanySelected;
        OrganizationSelected = dto.OrganizationSelected;
        PositionSelected = dto.PositionSelected;
        TitleSelected = dto.TitleSelected;
        StartDate = dto.StartDate;
        EndDate = dto.EndDate;
    }
    public DateRangeFormFilterDefault ConvertToDefault()
        => new()
        {
            EmployeeSelected = EmployeeSelected,
            CompanySelected = CompanySelected,
            OrganizationSelected = OrganizationSelected,
            PositionSelected = PositionSelected,
            TitleSelected = TitleSelected,
            StartDate = StartDate,
            EndDate = EndDate
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
