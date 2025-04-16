using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeeSkillValidator : AbstractValidator<EmployeeSkillDto>
{
    public EmployeeSkillValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(r => r.SkillKey).NotNull().NotEmpty().WithMessage("Skill Key cannot be null or empty");
            RuleFor(r => r.Level).IsInEnum().WithMessage("Not valid level skill");
            RuleFor(r => r.IsCertificated).NotNull().NotEmpty().WithMessage("Is Certificated cannot be null or empty");
        });

        RuleSet("Update", () =>
        {
            RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee Key cannot be null or empty");
            RuleFor(r => r.SkillKey).NotNull().NotEmpty().WithMessage("Skill Key cannot be null or empty");
            RuleFor(r => r.Level).IsInEnum().WithMessage("Not valid level skill");
            RuleFor(r => r.IsCertificated).NotNull().NotEmpty().WithMessage("Is Certificated cannot be null or empty");
        });
    }
}
