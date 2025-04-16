using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class ReligionValidator : AbstractValidator<ReligionDto>
{
    public ReligionValidator()
    {
        RuleFor(b => b.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
                            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");
        RuleFor(b => b.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
        RuleFor(b => b.Description).MaximumLength(200).WithMessage("Description must not exceed 200 characters");
    }
}
