
using Saga.Domain.Entities.Payrolls;

namespace Saga.Domain.Dtos.Payrolls;

public class BpjsConfigDto
{
    public Guid Key { get; set; } = Guid.Empty;
    public string Name { get; set; } = null!;
    public BpjsBaseOnCalculation BaseOnCalculation { get; set; } = BpjsBaseOnCalculation.BasicSalary;
    public decimal BaseOnFixedAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal MinAmount { get; set; }
    public string Description { get; set; } = string.Empty;

    public BpjsConfig ConvertToEntity()
        => new()
        {
            Key = Key,
            Name = Name,
            BaseOnCalculation = BaseOnCalculation,
            BaseOnFixedAmount = BaseOnFixedAmount,
            MaxAmount = MaxAmount,
            MinAmount = MinAmount,
            Description = Description
        };
}

public class BpjsSubConfigDto
{
    public Guid Key { get; set; }
    public Guid BpjsConfigKey { get; set; }
    public string Name { get; set; } = null!;
    public decimal Percentage { get; set; }

    public BpjsSubConfig ConvertToEntity()
        => new()
        {
            Key = Key,
            BpjsConfigKey = BpjsConfigKey,
            Name = Name,
            Percentage = Percentage,
        };

}