using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class CutOffDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public int? YearPeriod { get; set; } = 0;
    public string? Description { get; set; } = String.Empty;
    public int? JanStart { get; set; } = 0;
    public int? JanEnd { get; set; } = 0;
    public int? FebStart { get; set; } = 0;
    public int? FebEnd { get; set; } = 0;
    public int? MarStart { get; set; } = 0;
    public int? MarEnd { get; set; } = 0;
    public int? AprStart { get; set; } = 0;
    public int? AprEnd { get; set; } = 0;
    public int? MayStart { get; set; } = 0;
    public int? MayEnd { get; set; } = 0;
    public int? JunStart { get; set; } = 0;
    public int? JunEnd { get; set; } = 0;
    public int? JulStart { get; set; } = 0;
    public int? JulEnd { get; set; } = 0;
    public int? AugStart { get; set; } = 0;
    public int? AugEnd { get; set; } = 0;
    public int? SepStart { get; set; } = 0;
    public int? SepEnd { get; set; } = 0;
    public int? OctStart { get; set; } = 0;
    public int? OctEnd { get; set; } = 0;
    public int? NovStart { get; set; } = 0;
    public int? NovEnd { get; set; } = 0;
    public int? DecStart { get; set; } = 0;
    public int? DecEnd { get; set; } = 0;

    public CutOff ConvertToEntity()
    {
        return new CutOff
        {
            Key = this.Key ?? Guid.Empty,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            YearPeriod = this.YearPeriod ?? 0,
            Description = this.Description,
            JanStart = this.JanStart ?? 0,
            JanEnd = this.JanEnd ?? 0,
            FebStart = this.FebStart ?? 0,
            FebEnd = this.FebEnd ?? 0,
            MarStart = this.MarStart ?? 0,
            MarEnd = this.MarEnd ?? 0,
            AprStart = this.AprStart ?? 0,
            AprEnd = this.AprEnd ?? 0,
            MayStart = this.MayStart ?? 0,
            MayEnd = this.MayEnd ?? 0,
            JunStart = this.JunStart ?? 0,
            JunEnd = this.JunEnd ?? 0,
            JulStart = this.JulStart ?? 0,
            JulEnd = this.JulEnd ?? 0,
            AugStart = this.AugStart ?? 0,
            AugEnd = this.AugEnd ?? 0,
            SepStart = this.SepStart ?? 0,
            SepEnd = this.SepEnd ?? 0,
            OctStart = this.OctStart ?? 0,
            OctEnd = this.OctEnd ?? 0,
            NovStart = this.NovStart ?? 0,
            NovEnd = this.NovEnd ?? 0,
            DecStart = this.DecStart ?? 0,
            DecEnd = this.DecEnd ?? 0
        };
    }
}
