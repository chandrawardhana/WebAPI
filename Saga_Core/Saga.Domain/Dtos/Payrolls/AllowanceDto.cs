
using Saga.Domain.Entities.Payrolls;

namespace Saga.Domain.Dtos.Payrolls;

/// <summary>
/// ashari.herman 2025-03-07 slipi jakarta
/// </summary>

public class AllowanceDto
{
    public Guid Key { get; set; } = Guid.Empty;
    public string Name { get; set; } = null!;
    public bool IsFixedAllowance { get; set; }
    public TaxVariable TaxVariable { get; set; }
    public string Description { get; set; } = string.Empty;

    public Allowance ConvertToEntity()
        => new()
        {
            Key = Key,
            Name = Name,
            IsFixedAllowance = IsFixedAllowance,
            TaxVariable = TaxVariable,
            Description = Description
        };
}

public class AllowanceSubDto
{
    public Guid Key { get; set; }
    public Guid AllowanceKey { get; set; }
    public string Name { get; set; } = null!;
    public AllowanceCategory Category { get; set; }
    public decimal Amount { get; set; }

    public AllowanceSub ConvertToEntity()
        => new()
        {
            Key = Key,
            AllowanceKey = AllowanceKey,
            Name = Name,
            Category = Category,
            Amount = Amount
        };

}
