
using Saga.Domain.Dtos.Payrolls;
using Saga.Domain.Entities.Payrolls;

namespace Saga.Domain.ViewModels.Payrolls;

/// <summary>
/// ashari.herman 2025-03-10 slipi jakarta
/// </summary>

public class BpjsConfigViewModel
{
    public Guid Key { get; set; }
    public string Name { get; set; } = null!;
    public BpjsBaseOnCalculation BaseOnCalculation { get; set; } = BpjsBaseOnCalculation.BasicSalary;
    public decimal BaseOnFixedAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal MinAmount { get; set; }
    public string Description { get; set; } = string.Empty;

    public BpjsSubConfig[] BpjsSubs { get; set; } = [];
    public BpjsBaseOnCalculation[] BpjsBaseOnCalculations { get; set; } = [];

    public void FromDto(BpjsConfigDto dto)
    {
        this.Name = dto.Name;
        this.BaseOnCalculation = dto.BaseOnCalculation;
        this.BaseOnFixedAmount = dto.BaseOnFixedAmount;
        this.MaxAmount = dto.MaxAmount;
        this.MinAmount = dto.MinAmount;
        this.Description = dto.Description;
    }
}
