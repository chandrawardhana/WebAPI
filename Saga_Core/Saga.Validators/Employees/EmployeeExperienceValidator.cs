using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeeExperienceValidator : AbstractValidator<EmployeeExperienceDto>
{
    public EmployeeExperienceValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(r => r.CompanyName).NotNull().NotEmpty().WithMessage("Company Name cannot be null or empty")
                                   .MaximumLength(200).WithMessage("Company Name must not exceed 200 characters");
            RuleFor(r => r.PositionKey).NotNull().NotEmpty().WithMessage("Position Key cannot be null or empty");
            RuleFor(r => r.YearStart).GreaterThanOrEqualTo(1000).WithMessage("Year Start must be a 4-digit number.")
                                     .LessThanOrEqualTo(9999).WithMessage("Year Start must be a 4-digit number.");
            RuleFor(r => r.YearEnd).GreaterThanOrEqualTo(1000).WithMessage("Year End must be a 4-digit number.")
                                     .LessThanOrEqualTo(9999).WithMessage("Year End must be a 4-digit number.");
        });

        RuleSet("Update", () =>
        {
            RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee Key cannot be null or empty");
            RuleFor(r => r.CompanyName).NotNull().NotEmpty().WithMessage("Company Name cannot be null or empty")
                                       .MaximumLength(200).WithMessage("Company Name must not exceed 200 characters");
            RuleFor(r => r.PositionKey).NotNull().NotEmpty().WithMessage("Position Key cannot be null or empty");
            RuleFor(r => r.YearStart).GreaterThanOrEqualTo(1000).WithMessage("Year Start must be a 4-digit number.")
                                     .LessThanOrEqualTo(9999).WithMessage("Year Start must be a 4-digit number.");
            RuleFor(r => r.YearEnd).GreaterThanOrEqualTo(1000).WithMessage("Year End must be a 4-digit number.")
                                     .LessThanOrEqualTo(9999).WithMessage("Year End must be a 4-digit number.");
        });
    }
}
