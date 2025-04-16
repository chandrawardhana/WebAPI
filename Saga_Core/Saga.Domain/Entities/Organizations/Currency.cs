namespace Saga.Domain.Entities.Organizations;

[Table("tbmcurrency", Schema = "Organization")]
public class Currency : AuditTrail
{
    [Required]
    [StringLength(3)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(5)]
    public string Symbol { get; set; } = null!;

    [Required]
    public CurrencySymbol SymbolPosition { get; set; }

    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;

}
