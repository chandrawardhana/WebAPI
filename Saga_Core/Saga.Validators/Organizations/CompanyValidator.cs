using FluentValidation;
using Saga.Domain.Dtos.Organizations;

namespace Saga.Validators.Organizations;

public class CompanyValidator : AbstractValidator<CompanyDto>
{
    public CompanyValidator()
    {
        RuleFor(r => r.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
                            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(r => r.CountryKey).NotNull().NotEmpty().WithMessage("Country cannot be null or empty");
        RuleFor(r => r.ProvinceKey).NotNull().NotEmpty().WithMessage("Province cannot be null or empty");
        RuleFor(r => r.CityKey).NotNull().NotEmpty().WithMessage("City cannot be null or empty");
        RuleFor(r => r.Address).NotNull().NotEmpty().WithMessage("Address cannot be null or empty")
                               .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
        RuleFor(r => r.PostalCode).NotNull().NotEmpty().WithMessage("Postal Code cannot be null or empty")
                               .MaximumLength(5).WithMessage("Postal Code must not exceed 5 characters")
                               .Must(x => int.TryParse(x, out int value) && value > 0).WithMessage("Invalid Number");
        RuleFor(r => r.Email).NotNull().NotEmpty().WithMessage("Email cannot be null or empty")
                             .EmailAddress().WithMessage("Not valid email address")
                             .MaximumLength(50).WithMessage("Email must not exceed 50 characters");
        RuleFor(r => r.PhoneNumber).NotNull().NotEmpty().WithMessage("Phone Number cannot be null or empty")
                               .Matches(@"^(?:\+62|62|0)[2-9]\d{7,11}$").WithMessage("Not Valid Phone Number")
                               .MaximumLength(20).WithMessage("Phone Number must not exceed 20 characters");
        RuleFor(r => r.Website).MaximumLength(100).WithMessage("Website must not exceed 100 characters");
        RuleFor(r => r.TaxId).NotNull().NotEmpty().WithMessage("Tax Id cannot be null or empty")
                                .MaximumLength(20).WithMessage("Tax Id must not exceed 20 characters");
        RuleFor(r => r.TaxName).NotNull().NotEmpty().WithMessage("Tax Name cannot be null or empty")
                                .MaximumLength(100).WithMessage("Tax Name must not exceed 100 characters");
        RuleFor(r => r.TaxAddress).NotNull().NotEmpty().WithMessage("Tax Address cannot be null or empty")
                                .MaximumLength(200).WithMessage("Tax Address must not exceed 200 characters");
        RuleFor(r => r.BankKey).NotNull().NotEmpty().WithMessage("Bank cannot be null or empty");
        RuleFor(r => r.BankAccountNumber).NotNull().NotEmpty().WithMessage("Bank Account Number cannot be null or empty")
                                .MaximumLength(50).WithMessage("Bank Account Number must not exceed 20 characters");
        RuleFor(r => r.BankAccountName).NotNull().NotEmpty().WithMessage("Bank Account Name cannot be null or empty")
                                .MaximumLength(100).WithMessage("Bank Account Name must not exceed 100 characters");
        RuleFor(r => r.BankAddress).NotNull().NotEmpty().WithMessage("Bank Address cannot be null or empty")
                                .MaximumLength(200).WithMessage("Bank Address must not exceed 200 characters");
    }
}
