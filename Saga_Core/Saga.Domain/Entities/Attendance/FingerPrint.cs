using Saga.Domain.ViewModels.Attendances;

namespace Saga.Domain.Entities.Attendance;

[Table("tbmfingerprint", Schema = "Attendance")]
public class FingerPrint : AuditTrail
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
    public ConnectionMethod Method { get; set; }

    [Required]
    [StringLength(50)]
    public string IPAddress { get; set; } = null!;

    [Required]
    public int Port { get; set; }

    [StringLength(100)]
    public string? CommKey { get; set; } = String.Empty;

    [StringLength(50)]
    public string? Comm { get; set; } = String.Empty;

    public int? Baudrate { get; set; } = 0;

    [StringLength(200)]
    public string? Description { get; set; } = String.Empty;

    public TimeSpan[]? RetrieveScheduleTimes { get; set; }

    [StringLength(15)]
    public string? SerialNumber { get; set; } = String.Empty;

    [NotMapped]
    public Company? Company { get; set; }

    [NotMapped]
    public bool Status { get; set; }

    public FingerPrintListItem ConvertToFingerPrintListItemViewModel()
    {
        return new FingerPrintListItem
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            Code = this.Code,
            Name = this.Name,
            Method = this.Method,
            IPAddress = this.IPAddress,
            Company = this.Company,
            RetrieveScheduleTimes = this.RetrieveScheduleTimes ?? Array.Empty<TimeSpan>(),
            SerialNumber = this.SerialNumber
        };
    }

    public FingerPrintForm ConvertToFingerPrintFormViewModel()
    {
        return new FingerPrintForm
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            Code = this.Code,
            Name = this.Name,
            Method = this.Method,
            IPAddress = this.IPAddress,
            Port = this.Port,
            CommKey = this.CommKey,
            Comm = this.Comm,
            Baudrate = this.Baudrate,
            Description = this.Description,
            Company = this.Company,
            RetrieveScheduleTimes = this.RetrieveScheduleTimes ?? Array.Empty<TimeSpan>(),
            SerialNumber = this.SerialNumber
        };
    }
}
