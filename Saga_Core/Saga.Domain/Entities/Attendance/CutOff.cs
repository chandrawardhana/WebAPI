using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmcutoff", Schema = "Attendance")]
public class CutOff : AuditTrail
{
    [Required]
    public Guid CompanyKey { get; set; }
    [Required]
    public int YearPeriod { get; set; }
    [StringLength(200)]
    public string? Description { get; set; } = string.Empty;
    [Required]
    public int JanStart { get; set; }
    [Required]
    public int JanEnd { get; set; }
    [Required]
    public int FebStart { get; set; }
    [Required]
    public int FebEnd { get; set; }
    [Required]
    public int MarStart { get; set; }
    [Required]
    public int MarEnd { get; set; }
    [Required]
    public int AprStart { get; set; }
    [Required]
    public int AprEnd { get; set; }
    [Required]
    public int MayStart { get; set; }
    [Required]
    public int MayEnd { get; set; }
    [Required]
    public int JunStart { get;set; }
    [Required]
    public int JunEnd { get; set; }
    [Required]
    public int JulStart { get; set; }
    [Required]
    public int JulEnd { get; set; }
    [Required]
    public int AugStart { get; set; }
    [Required]
    public int AugEnd { get; set; }
    [Required]
    public int SepStart { get; set; }
    [Required]
    public int SepEnd { get; set; }
    [Required]
    public int OctStart { get; set; }
    [Required]
    public int OctEnd { get; set; }
    [Required]
    public int NovStart { get; set; }
    [Required]
    public int NovEnd { get; set; }
    [Required]
    public int DecStart { get; set; }
    [Required]
    public int DecEnd { get; set; }

    [NotMapped]
    public Company? Company { get; set; }

    public CutOffForm ConvertToViewModelCutOff()
    {
        return new CutOffForm 
        { 
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            YearPeriod = this.YearPeriod,
            Description = this.Description,
            JanStart = this.JanStart,
            JanEnd = this.JanEnd,
            FebStart = this.FebStart,
            FebEnd = this.FebEnd,
            MarStart = this.MarStart,
            MarEnd = this.MarEnd,
            AprStart = this.AprStart,
            AprEnd = this.AprEnd,
            MayStart = this.MayStart,
            MayEnd = this.MayEnd,
            JunStart = this.JunStart,
            JunEnd = this.JunEnd,
            JulStart = this.JulStart,
            JulEnd = this.JulEnd,
            AugStart = this.AugStart,
            AugEnd = this.AugEnd,
            SepStart = this.SepStart,
            SepEnd = this.SepEnd,
            OctStart = this.OctStart,
            OctEnd = this.OctEnd,
            NovStart = this.NovStart,
            NovEnd = this.NovEnd,
            DecStart = this.DecStart,
            DecEnd = this.DecEnd,
            Company = this.Company
        };
    }
}
