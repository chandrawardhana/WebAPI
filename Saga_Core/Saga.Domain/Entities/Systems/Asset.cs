namespace Saga.Domain.Entities.Systems;

[Table("tbmasset", Schema = "System")]
public class Asset : AuditTrail
{
    [Required]
    [StringLength(100)]
    public string FileName { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string OriginalFileName { get; set; } = null!;
    [Required]
    [StringLength(10)]
    public string MimeType { get; set; } = null!;
    public DateTime UploadAt { get; set; }
}
