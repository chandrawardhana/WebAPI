namespace Saga.Domain.Dtos.Employees;

public class EmployeePayrollDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public string? TaxNumber { get; set; } = String.Empty;
    public DateTime? TaxRegistered { get; set; } = DateTime.Now;
    public string? TaxAddress { get; set; } = String.Empty;
    public TaxStatus? TaxStatus { get; set; }
    public string? HealthNationalityInsuranceNumber { get; set; } = String.Empty;
    public DateTime? HealthNationalityInsuranceRegistered { get; set; } = DateTime.Now;
    public string? LaborNationalityInsuranceNumber { get; set; } = String.Empty;
    public DateTime? LaborNationalityInsuranceRegistered { get; set; } = DateTime.Now;
    public string? PensionNationalityInsuranceNumber { get; set; } = String.Empty;
    public DateTime? PensionNationalityInsuranceRegistered { get; set; } = DateTime.Now;
    public Guid? BankKey { get; set; } = Guid.Empty;
    public string? BankAccountNumber { get; set; } = String.Empty;
    public string? BankAccountName { get; set; } = String.Empty;
    public string? BankAddress { get; set; } = String.Empty;

    public EmployeePayroll ConvertToEntity()
    {
        return new EmployeePayroll
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey ?? Guid.Empty,
            TaxNumber = this.TaxNumber ?? String.Empty,
            TaxRegistered = this.TaxRegistered,
            TaxAddress = this.TaxAddress ?? String.Empty,
            TaxStatus = this.TaxStatus ?? Enums.TaxStatus.K0,
            HealthNationalityInsuranceNumber = this.HealthNationalityInsuranceNumber ?? String.Empty,
            HealthNationalityInsuranceRegistered = this.HealthNationalityInsuranceRegistered ?? DateTime.Now,
            LaborNationalityInsuranceNumber = this.LaborNationalityInsuranceNumber ?? String.Empty,
            LaborNationalityInsuranceRegistered = this.LaborNationalityInsuranceRegistered ?? DateTime.Now,
            PensionNationalityInsuranceNumber = this.PensionNationalityInsuranceNumber ?? String.Empty,
            PensionNationalityInsuranceRegistered = this.PensionNationalityInsuranceRegistered ?? DateTime.Now,
            BankKey = this.BankKey ?? Guid.Empty,
            BankAccountNumber = this.BankAccountNumber ?? String.Empty,
            BankAccountName = this.BankAccountName ?? String.Empty,
            BankAddress = this.BankAddress ?? String.Empty,
        };
    }
}
