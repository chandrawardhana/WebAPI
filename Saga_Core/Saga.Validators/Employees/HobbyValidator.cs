using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class HobbyValidator : AbstractValidator<HobbyDto>
{
    public HobbyValidator()
    {
        RuleFor(r => r.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
        RuleFor(r => r.Description).MaximumLength(200).WithMessage("Description must not exceed 200 characters");
    }
}
