using Saga.Domain.ViewModels.Attendances;
using Saga.Domain.ViewModels.Organizations;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmholiday", Schema = "Attendance")]
public class Holiday : AuditTrail
{
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    public int Duration { get; set; }
    public string? Description { get; set; } = string.Empty;

    [Column("Companies", TypeName = "text")]
    public Guid[]? CompanyKeys = Array.Empty<Guid>();

    [Required]
    public DateOnly DateEvent { get; set; }

    public HolidayForm ConvertToViewModelHolidayForm(IEnumerable<Company>? companies = null)
    {
        return new HolidayForm
        {
            Key = this.Key,
            Name = this.Name,
            Duration = this.Duration,
            Description = this.Description,
            ExistingCompanies = this.CompanyKeys ?? Array.Empty<Guid>(),
            DateEvent = this.DateEvent,
            Companies = companies != null
                ? companies
                    .Where(c => this.CompanyKeys.Contains(c.Key))
                    .Select(c => new CompanyForm
                    {
                        Key = c.Key,
                        Name = c.Name
                    })
                    .ToList()
                : Enumerable.Empty<CompanyForm>()
        };
    }

    public HolidayListItem ConvertToViewModelHolidayListItem(IEnumerable<Company>? companies = null)
    {
        return new HolidayListItem
        {
            Key = this.Key,
            Name = this.Name,
            Duration = this.Duration,
            Description = this.Description,
            DateEvent = this.DateEvent,
            CreatedAt = this.CreatedAt,
            CreatedBy = this.CreatedBy,
            Companies = companies != null
                ? companies
                    .Where(c => this.CompanyKeys.Contains(c.Key))
                    .Select(c => new CompanyForm
                    {
                        Key = c.Key,
                        Name = c.Name
                    })
                    .ToList()
                : Enumerable.Empty<CompanyForm>()
        };
    }
}
