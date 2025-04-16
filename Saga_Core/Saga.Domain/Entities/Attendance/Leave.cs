using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmleave", Schema = "Attendance")]
public class Leave : AuditTrail
{
    [Required]
    public Guid CompanyKey { get; set; }
    [Required]
    [StringLength(10)]
    public string Code { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
    [Required]
    public int MaxDays { get; set; }
    [Required]
    public int MinSubmission { get; set; }
    [Required]
    public int MaxSubmission { get; set; }
    public bool? IsByWeekDay { get; set; } = false;
    public bool? IsResidue { get; set; } = false;
    [MaxLength(200)]
    public string? Description { get; set; } = string.Empty;

    [NotMapped]
    public Company? Company { get; set; }
    
    public LeaveForm ConvertToViewModelLeave()
    {
        return new LeaveForm
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            Code = this.Code,
            Name = this.Name,
            MaxDays = this.MaxDays,
            MinSubmission = this.MinSubmission,
            MaxSubmission = this.MaxSubmission,
            IsByWeekDay = this.IsByWeekDay,
            IsResidue = this.IsResidue,
            Description = this.Description,
            Company = this.Company,
        };
    }

    public LeaveListItem ConvertToViewModelLeaveListItem()
    {
        return new LeaveListItem
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            Code = this.Code,
            Name = this.Name,
            MaxDays = this.MaxDays,
            MinSubmission = this.MinSubmission,
            MaxSubmission = this.MaxSubmission,
            IsByWeekDay = this.IsByWeekDay,
            IsResidue = this.IsResidue,
            Company = this.Company
        };
    }
}
