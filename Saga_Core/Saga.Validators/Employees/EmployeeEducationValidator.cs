using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeeEducationValidator : AbstractValidator<EmployeeEducationDto>
{
    public EmployeeEducationValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(r => r.EducationKey).NotNull().NotEmpty().WithMessage("Education Key cannot be null or empty");
            RuleFor(r => r.IsCertificated).NotNull().NotEmpty().WithMessage("Is Certificated cannot be null or empty");
            RuleFor(r => r.GraduatedYear).GreaterThanOrEqualTo(1000).WithMessage("Graduated Year must be a 4-digit number.")
                                    .LessThanOrEqualTo(9999).WithMessage("Graduated Year must be a 4-digit number.");
        });

        RuleSet("Update", () =>
        {
            RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee Key cannot be null or empty");
            RuleFor(r => r.EducationKey).NotNull().NotEmpty().WithMessage("Education Key cannot be null or empty");
            RuleFor(r => r.IsCertificated).NotNull().NotEmpty().WithMessage("Is Certificated cannot be null or empty");
            RuleFor(r => r.GraduatedYear).GreaterThanOrEqualTo(1000).WithMessage("Graduated Year must be a 4-digit number.")
                                    .LessThanOrEqualTo(9999).WithMessage("Graduated Year must be a 4-digit number.");
        });
    }
}
