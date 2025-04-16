using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class CutOffListItem
{
    public Guid Key { get; set; }
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
    public Company? Company { get; set; }
}

public class CutOffList
{
    public IEnumerable<CutOffListItem> CutOffs { get; set; } = new List<CutOffListItem>();
}

public class CutOffForm : CutOffListItem
{
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();

    public CutOffDto ConvertToCutOffDto()
    {
        return new CutOffDto
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
            DecEnd = this.DecEnd
        };
    }
}


