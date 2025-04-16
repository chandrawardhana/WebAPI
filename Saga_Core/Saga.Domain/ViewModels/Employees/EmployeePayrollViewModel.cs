using Saga.Domain.Dtos.Employees;
using System;

namespace Saga.Domain.ViewModels.Employees;

public class EmployeePayrollForm
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? EmployeeKey { get; set; } = Guid.Empty;
    public string? TaxNumber { get; set; } = String.Empty;
    public DateTime? TaxRegistered { get; set; } = DateTime.Now;
    public string? TaxAddress { get; set; } = String.Empty;
    public TaxStatus? TaxStatus { get; set; } = null;
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

    public Employee? Employee { get; set; }
    public Bank? Bank { get; set; }

    public EmployeePayrollDto ConvertToEmployeePayrollDto()
    {
        return new EmployeePayrollDto
        {
            Key = this.Key ?? Guid.Empty,
            EmployeeKey = this.EmployeeKey,
            TaxNumber = this.TaxNumber,
            TaxRegistered = this.TaxRegistered,
            TaxAddress = this.TaxAddress,
            TaxStatus = this.TaxStatus,
            HealthNationalityInsuranceNumber = this.HealthNationalityInsuranceNumber,
            HealthNationalityInsuranceRegistered = this.HealthNationalityInsuranceRegistered,
            LaborNationalityInsuranceNumber = this.LaborNationalityInsuranceNumber,
            LaborNationalityInsuranceRegistered = this.LaborNationalityInsuranceRegistered,
            PensionNationalityInsuranceNumber = this.PensionNationalityInsuranceNumber,
            PensionNationalityInsuranceRegistered = this.PensionNationalityInsuranceRegistered,
            BankKey = this.BankKey,
            BankAccountNumber = this.BankAccountNumber,
            BankAccountName = this.BankAccountName,
            BankAddress = this.BankAddress,
        };
    }
}
