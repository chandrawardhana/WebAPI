using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class StandardWorkingDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public int? YearPeriod { get; set; } = 0;
    public int? January { get; set; } = 0;
    public int? February { get; set; } = 0;
    public int? March { get; set; } = 0;
    public int? April { get; set; } = 0;
    public int? May { get; set; } = 0;
    public int? June { get; set; } = 0;
    public int? July { get; set; } = 0;
    public int? August { get; set; } = 0;
    public int? September { get; set; } = 0;
    public int? October { get; set; } = 0;
    public int? November { get; set; } = 0;
    public int? December { get; set; } = 0;
    public string? Description { get; set; } = string.Empty;

    public StandardWorking ConvertToEntity()
    {
        return new StandardWorking
        {
            Key = this.Key ?? Guid.Empty,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            YearPeriod = this.YearPeriod ?? 0,
            January = this.January ?? 0,
            February = this.February ?? 0,
            March = this.March ?? 0,
            April = this.April ?? 0,
            May = this.May ?? 0,
            June = this.June ?? 0,
            July = this.July ?? 0,
            August = this.August ?? 0,
            September = this.September ?? 0,
            October = this.October ?? 0,
            November = this.November ?? 0,
            December = this.December ?? 0,
            Description = this.Description ?? string.Empty
        };
    }
}
