using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmstandardworking", Schema = "Attendance")]
public class StandardWorking : AuditTrail
{
    [Required]
    public Guid CompanyKey { get; set; }
    [Required]
    public int YearPeriod { get; set; }
    [Required]
    public int January { get; set; }
    [Required]
    public int February { get; set; }
    [Required]
    public int March { get; set; }
    [Required]
    public int April { get; set; }
    [Required]
    public int May { get; set; }
    [Required]
    public int June { get; set; }
    [Required]
    public int July { get; set; }
    [Required]
    public int August { get; set; }
    [Required]
    public int September { get; set; }
    [Required]
    public int October { get; set; }
    [Required]
    public int November { get; set; }
    [Required]
    public int December { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; } = String.Empty;

    [NotMapped]
    public Company? Company { get; set; }

    public StandardWorkingForm ConvertToViewModelStandardWorking()
    {
        return new StandardWorkingForm
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            YearPeriod = this.YearPeriod,
            January = this.January,
            February = this.February,
            March = this.March,
            April = this.April,
            May = this.May,
            June = this.June,
            July = this.July,
            August = this.August,
            September = this.September,
            October = this.October,
            November = this.November,
            December = this.December,
            Description = this.Description,
        };
    }
}
