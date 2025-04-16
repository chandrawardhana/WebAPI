
using Saga.Domain.Entities.Payrolls;

namespace Saga.Domain.Dtos.Payrolls;

public class AverageEffectiveRateDto
{
    public Guid Key { get; set; } 
    public string Name { get; set; } = null!;
    public TaxStatus[] TaxStatuses { get; set; } = [];
    public string Description { get; set; } = string.Empty;

    public AverageEffectiveRateDetail[] Details { get; set; } = [];

    public AverageEffectiveRate ConvertToEntity()
    {
        var entity = new AverageEffectiveRate();
        entity.Key = Key;
        entity.Name = Name;
        entity.Description = Description;
        entity.TaxStatuses = TaxStatuses;
        entity.Details = Details;
        return entity;
    }
}
