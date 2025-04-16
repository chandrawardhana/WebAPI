using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class LeaveDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? Name { get; set; } = String.Empty;
    public int? MaxDays { get; set; } = 0;
    public int? MinSubmission { get; set; } = 0;
    public int? MaxSubmission { get; set; } = 0;
    public bool? IsByWeekDay { get; set; } = false;
    public bool? IsResidue { get; set; } = false;
    public string? Description { get; set; } = String.Empty;

    public Leave ConvertToEntity()
    {
        return new Leave
        {
            Key = this.Key ?? Guid.Empty,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            Code = this.Code ?? String.Empty,
            Name = this.Name ?? String.Empty,
            MaxDays = this.MaxDays ?? 0,
            MinSubmission = this.MinSubmission ?? 0,
            MaxSubmission = this.MaxSubmission ?? 0,
            IsByWeekDay = this.IsByWeekDay ?? false,
            IsResidue = this.IsResidue ?? false,
            Description = this.Description ?? String.Empty
        };
    }
}
