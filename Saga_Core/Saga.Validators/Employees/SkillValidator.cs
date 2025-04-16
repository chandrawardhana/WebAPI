using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class SkillValidator : AbstractValidator<SkillDto>
{
    public SkillValidator() 
    {
        RuleFor(b => b.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
                            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");
        RuleFor(b => b.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(b => b.Description).MaximumLength(200).WithMessage("Description must not exceed 200 characters");
    }
}
