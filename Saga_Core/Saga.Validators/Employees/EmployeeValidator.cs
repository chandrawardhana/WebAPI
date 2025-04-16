using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeeValidator : AbstractValidator<EmployeeDto>
{
    public EmployeeValidator() 
    {
        RuleFor(r => r.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
                            .MaximumLength(30).WithMessage("Code must not exceed 30 characters");
        RuleFor(r => r.FirstName).NotNull().NotEmpty().WithMessage("FirstName cannot be null or empty")
                            .MaximumLength(50).WithMessage("FirstName must not exceed 50 characters");
        RuleFor(r => r.LastName).MaximumLength(50).WithMessage("LastName must not exceed 50 characters");
        RuleFor(r => r.CompanyKey).NotNull().NotEmpty().WithMessage("Company cannot be null or empty");
        RuleFor(r => r.OrganizationKey).NotNull().NotEmpty().WithMessage("Organization cannot be null or empty");
        RuleFor(r => r.PositionKey).NotNull().NotEmpty().WithMessage("Position cannot be null or empty");
        RuleFor(r => r.TitleKey).NotNull().NotEmpty().WithMessage("Title cannot be null or empty");
        RuleFor(r => r.BranchKey).NotNull().NotEmpty().WithMessage("Branch cannot be null or empty");
        RuleFor(r => r.GradeKey).NotNull().NotEmpty().WithMessage("Grade cannot be null or empty");
        RuleFor(r => r.HireDate).Must(BeAValidDate).WithMessage("Hire Date is required");
        RuleFor(r => r.Status).IsInEnum().WithMessage("Not valid employee status");
        RuleFor(r => r.ResignDate)
            .NotNull().NotEmpty().WithMessage("Resign Date is required when status is Resign")
            .Must(BeAValidDate).WithMessage("Resign Date is not a valid date")
            .When(r => r.Status == Domain.Enums.EmployeeStatus.Resign);

        // Add additional validation to ensure ResignDate is after HireDate when present
        RuleFor(r => r.ResignDate)
            .Must((employee, resignDate) => !resignDate.HasValue || resignDate.Value > employee.HireDate)
            .When(r => r.ResignDate.HasValue && r.HireDate.HasValue)
            .WithMessage("Resign Date must be after Hire Date");

        RuleFor(r => r.CorporateEmail).EmailAddress().WithMessage("Not valid email address")
                                      .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
                                      .When(r => !string.IsNullOrEmpty(r.CorporateEmail));
        RuleFor(r => r.PhoneExtension).Matches(@"^(?:\+62|62|0)[2-9]\d{7,11}$").WithMessage("Not Valid Phone Number")
                                   .MaximumLength(20).WithMessage("Phone Number must not exceed 20 characters")
                                   .When(r => !string.IsNullOrEmpty(r.PhoneExtension));
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
