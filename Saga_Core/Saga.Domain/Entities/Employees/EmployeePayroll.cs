using Saga.Domain.ViewModels.Employees;

namespace Saga.Domain.Entities.Employees;

[Table("tbmemployeepayroll", Schema = "Employee")]
public class EmployeePayroll : AuditTrail
{
    [Required]
    public Guid EmployeeKey { get; set; }
    [StringLength(13)]
    public string HealthNationalityInsuranceNumber { get; set; } = String.Empty;
    public DateTime? HealthNationalityInsuranceRegistered { get; set; } = DateTime.Now;
    [StringLength(16)]
    public string LaborNationalityInsuranceNumber { get; set; } = String.Empty;
    public DateTime? LaborNationalityInsuranceRegistered { get; set; } = DateTime.Now;
    [MaxLength(20)]
    public string PensionNationalityInsuranceNumber { get; set; } = String.Empty;
    public DateTime? PensionNationalityInsuranceRegistered { get; set; } = DateTime.Now;
    [Required]
    [MaxLength(20)]
    public string TaxNumber { get; set; } = null!;
    [Required]
    public DateTime? TaxRegistered { get; set; } = DateTime.Now;
    [Required]
    [MaxLength(200)]
    public string TaxAddress { get; set; } = null!;
    [Required]
    public TaxStatus TaxStatus { get; set; }
    [Required]
    public Guid BankKey { get; set; }
    [Required]
    [StringLength(50)]
    public string BankAccountNumber { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string BankAccountName { get; set; } = null!;
    [Required]
    [StringLength(200)]
    public string BankAddress { get; set; } = null!;

    [NotMapped]
    public Employee? Employee { get; set; }
    [NotMapped]
    public Bank? Bank { get; set; }

    public EmployeePayrollForm ConvertToEmployeePayrollForm()
    {
        return new EmployeePayrollForm
        {
            Key = this.Key,
            EmployeeKey = this.EmployeeKey,
            HealthNationalityInsuranceNumber = this.HealthNationalityInsuranceNumber,
            HealthNationalityInsuranceRegistered = this.HealthNationalityInsuranceRegistered,
            LaborNationalityInsuranceNumber = this.LaborNationalityInsuranceNumber,
            LaborNationalityInsuranceRegistered = this.LaborNationalityInsuranceRegistered,
            PensionNationalityInsuranceNumber = this.PensionNationalityInsuranceNumber,
            PensionNationalityInsuranceRegistered = this.PensionNationalityInsuranceRegistered,
            TaxNumber = this.TaxNumber,
            TaxRegistered = this.TaxRegistered,
            TaxAddress = this.TaxAddress,
            TaxStatus = this.TaxStatus,
            BankKey = this.BankKey,
            BankAccountNumber = this.BankAccountNumber,
            BankAccountName = this.BankAccountName,
            BankAddress = this.BankAddress,
            Bank = this.Bank
        };
    }
}
