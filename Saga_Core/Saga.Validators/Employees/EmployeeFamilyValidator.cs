using FluentValidation;
using Saga.Domain.Dtos.Employees;


namespace Saga.Validators.Employees;

public class EmployeeFamilyValidator : AbstractValidator<EmployeeFamilyDto>
{
    public EmployeeFamilyValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
            RuleFor(r => r.Gender).NotNull().NotEmpty().WithMessage("Gender cannot be null")
                .IsInEnum().WithMessage("Not valid gender");
            RuleFor(r => r.BoD).Must(BeAValidDate).WithMessage("Birth of Date is required");
            RuleFor(r => r.Relationship).NotNull().NotEmpty().WithMessage("Relationship cannot be null")
                .IsInEnum().WithMessage("Not valid relationship");
            RuleFor(r => r.Address).NotNull().NotEmpty().WithMessage("Address cannot be null or empty")
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
            RuleFor(r => r.PhoneNumber).Must(phoneNumber => string.IsNullOrEmpty(phoneNumber) || System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^(?:\+62|62|0)[2-9]\d{7,11}$"))
                                     .WithMessage("Phone Number must not exceed 20 digits if provided.");
        });

        RuleSet("Update", () =>
        {
            RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee Key cannot be null or empty");
            RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
            RuleFor(r => r.Gender).NotNull().NotEmpty().WithMessage("Gender cannot be null")
                .IsInEnum().WithMessage("Not valid gender");
            RuleFor(r => r.BoD).Must(BeAValidDate).WithMessage("Birth of Date is required");
            RuleFor(r => r.Relationship).NotNull().NotEmpty().WithMessage("Relationship cannot be null")
                .IsInEnum().WithMessage("Not valid relationship");
            RuleFor(r => r.Address).NotNull().NotEmpty().WithMessage("Address cannot be null or empty")
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
            RuleFor(r => r.PhoneNumber).Must(phoneNumber => string.IsNullOrEmpty(phoneNumber) || System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^(?:\+62|62|0)[2-9]\d{7,11}$"))
                                     .WithMessage("Phone Number must not exceed 20 digits if provided.");
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
