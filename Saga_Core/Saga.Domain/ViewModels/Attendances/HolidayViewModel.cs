using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.ViewModels.Organizations;

namespace Saga.Domain.ViewModels.Attendances;

public class HolidayListItem
{
    public Guid Key { get; set; }
    public string Name { get; set; } = null!;
    public int? Duration { get; set; } = 0;
    public string? Description { get; set; } = String.Empty;
    public Guid[]? CompanyKeys { get; set; }
    public IEnumerable<CompanyForm>? Companies { get; set; } = Enumerable.Empty<CompanyForm>();
    public DateOnly? DateEvent { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class HolidayList
{
    public IEnumerable<HolidayListItem> Holidays { get; set; } = new List<HolidayListItem>();
}

public class HolidayForm
{
    public Guid Key { get; set; }
    public string Name { get; set; } = null!;
    public int? Duration { get; set; } = 0;
    public string? Description { get; set; } = String.Empty;
    public Guid[]? CompanyKeys { get; set; } = Array.Empty<Guid>();
    //Existing company keys if any
    public Guid[]? ExistingCompanies { get; set; }

    public MultiSelectList CompanyTags { get; set; } = new MultiSelectList(new List<SelectListItem>());

    public DateOnly? DateEvent { get; set; }

    public IEnumerable<CompanyForm>? Companies { get; set; } = Enumerable.Empty<CompanyForm>();

    //Convert current instance to HolidayDto
    public HolidayDto ConvertToHolidayDto()
    {
        return new HolidayDto 
        { 
            Key = this.Key,
            Name = this.Name,
            Duration = this.Duration,
            Description = this.Description,
            CompanyKeys = this.CompanyKeys,
            ExistingCompanies = this.ExistingCompanies,
            DateEvent = this.DateEvent
        };
    }
}
