
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Payrolls;
using Saga.Domain.Entities.Payrolls;

namespace Saga.Domain.ViewModels.Payrolls;

/// <summary>
/// ashari.herman 2025-03-12 slipi jakarta
/// </summary>

public class PayslipTemplateViewModel
{
    public Guid Key { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PayslipTemplateDetail[] Details { get; set; } = [];

    public SelectListItem[] Existing { get; set; } = [];
    public SelectListItem[] Balances { get; set; } = [];
    public SelectListItem[] Components { get; set; } = [];

    public void FromDto(PayslipTemplateDto dto)
    {
        Key = dto.Key;
        Name = dto.Name;
        Description = dto.Description;
        Details = dto.Details;
    }
}
