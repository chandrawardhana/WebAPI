using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeePayrollValidator : AbstractValidator<EmployeePayrollDto>
{
    public EmployeePayrollValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(r => r.TaxNumber).NotNull().NotEmpty().WithMessage("Tax Number cannot be null or empty")
                                .Matches("^[0-9]+$").WithMessage("Tax Number must contain only digits");
            RuleFor(r => r.TaxRegistered).NotNull().NotEmpty().WithMessage("Tax Registered cannot be null or empty")
                                .Must(BeAValidDate).WithMessage("Tax Registered is not a valid date.");
            RuleFor(r => r.TaxAddress).NotNull().NotEmpty().WithMessage("Tax Address cannot be null or empty")
                                .MaximumLength(200).WithMessage("Tax Address must not exceed 200 characters");
            RuleFor(r => r.TaxStatus).IsInEnum().WithMessage("Not valid tax status");
            RuleFor(r => r.HealthNationalityInsuranceNumber).Must(healthNumber => string.IsNullOrEmpty(healthNumber) || System.Text.RegularExpressions.Regex.IsMatch(healthNumber, @"^\d{13}$"))
                                     .WithMessage("Health Nationality Insurance Number must contain exactly 13 digits if provided.");
            RuleFor(r => r.HealthNationalityInsuranceRegistered).NotNull().WithMessage("Health Nationality Insurance Registered cannot be null")
                                                                 .Must(BeAValidDate)
                                                                 .When(r => r.HealthNationalityInsuranceRegistered.HasValue)
                                                                 .WithMessage("Health Nationality Insurance Registered is not a valid date.");
            RuleFor(r => r.LaborNationalityInsuranceNumber).Must(laborNumber => string.IsNullOrEmpty(laborNumber) || System.Text.RegularExpressions.Regex.IsMatch(laborNumber, @"^\d{16}$"))
                                     .WithMessage("Labor Nationality Insurance Number must contain exactly 16 digits if provided.");
            RuleFor(r => r.LaborNationalityInsuranceRegistered).NotNull().WithMessage("Labor Nationality Insurance Registered cannot be null")
                                                                 .Must(BeAValidDate)
                                                                 .When(r => r.LaborNationalityInsuranceRegistered.HasValue)
                                                                 .WithMessage("Labor Nationality Insurance Registered is not a valid date.");
            RuleFor(r => r.PensionNationalityInsuranceNumber).Must(pensionNumber => string.IsNullOrEmpty(pensionNumber) || System.Text.RegularExpressions.Regex.IsMatch(pensionNumber, @"^\d{20}$"))
                                     .WithMessage("Pension Nationality Insurance Number must contain exactly 20 digits if provided.");
            RuleFor(r => r.PensionNationalityInsuranceRegistered).NotNull().WithMessage("Pension Nationality Insurance Registered cannot be null")
                                                                 .Must(BeAValidDate)
                                                                 .When(r => r.PensionNationalityInsuranceRegistered.HasValue)
                                                                 .WithMessage("Pension Nationality Insurance Registered is not a valid date.");
            RuleFor(r => r.BankKey).NotNull().NotEmpty().WithMessage("Bank cannot be null or empty");
            RuleFor(r => r.BankAccountNumber).NotNull().NotEmpty().WithMessage("Bank Account Number cannot be null or empty")
                                    .MaximumLength(50).WithMessage("Bank Account Number must not exceed 20 characters");
            RuleFor(r => r.BankAccountName).NotNull().NotEmpty().WithMessage("Bank Account Name cannot be null or empty")
                                    .MaximumLength(100).WithMessage("Bank Account Name must not exceed 100 characters");
            RuleFor(r => r.BankAddress).NotNull().NotEmpty().WithMessage("Bank Address cannot be null or empty")
                                    .MaximumLength(200).WithMessage("Bank Address must not exceed 200 characters");
        });
    }

    private bool BeAValidDate(DateTime date)
    {
        if (date == default(DateTime))
            return false;
        return true;
    }

    private bool BeAValidDate(DateTime? date)
    {
        if (date == default(DateTime))
            return false;
        return true;
    }
}
