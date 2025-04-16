using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeeHobbyValidator : AbstractValidator<EmployeeHobbyDto>
{
    public EmployeeHobbyValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(r => r.HobbyKey).NotNull().NotEmpty().WithMessage("Hobby Key cannot be null or empty");
            RuleFor(r => r.Level).IsInEnum().WithMessage("Not valid hobby skill");
        });

        RuleSet("Update", () =>
        {
            RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee Key cannot be null or empty");
            RuleFor(r => r.HobbyKey).NotNull().NotEmpty().WithMessage("Hobby Key cannot be null or empty");
            RuleFor(r => r.Level).IsInEnum().WithMessage("Not valid hobby skill");
        });
    }
}
