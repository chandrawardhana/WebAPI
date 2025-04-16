namespace Saga.Domain.Dtos.Organizations;

public class CurrencyDto
{
    public Guid? Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public CurrencySymbol SymbolPosition { get; set; }
    public string? Description { get; set; }
}
