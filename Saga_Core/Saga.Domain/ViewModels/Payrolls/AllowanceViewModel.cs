
using Saga.Domain.Dtos.Payrolls;
using Saga.Domain.Entities.Payrolls;

namespace Saga.Domain.ViewModels.Payrolls;

/// <summary>
/// ashari.herman 2025-03-07 slipi jakarta
/// </summary>

public class AllowanceViewModel
{
    public Guid Key { get; set; }
    public string Name { get; set; } = null!;
    public bool IsFixedAllowance { get; set; }
    public TaxVariable TaxVariable { get; set; } = TaxVariable.None;
    public string Description { get; set; } = string.Empty;

    public AllowanceSub[] AllowanceSubs { get; set; } = [];
    public TaxVariable[] TaxVariables { get; set; } = [];

    public void FromDto(AllowanceDto dto)
    {
        this.Name = dto.Name;
        this.IsFixedAllowance = dto.IsFixedAllowance;
        this.TaxVariable = dto.TaxVariable;
        this.Description = dto.Description;
    }
}
