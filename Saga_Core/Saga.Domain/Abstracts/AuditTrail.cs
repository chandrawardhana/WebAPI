namespace Saga.Domain.Abstracts;

public abstract class AuditTrail : IAuditTrail
{
    [Key]
    public Guid Key { get; set; }
    public DateTime? CreatedAt { get; set; }
    [MaxLength(40)]
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [MaxLength(40)]
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    [MaxLength(40)]
    public string? DeletedBy { get; set; }
}
