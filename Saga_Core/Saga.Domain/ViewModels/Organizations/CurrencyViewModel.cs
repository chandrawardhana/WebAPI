namespace Saga.Domain.ViewModels.Organizations;

public class CurrencyList
{
    public IEnumerable<Currency> Currencies { get; set; } = Enumerable.Empty<Currency>();
}

public class CurrencyForm
{
    public Guid Key { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public CurrencySymbol SymbolPosition { get; set; }
    public string? Description { get; set; }
}
