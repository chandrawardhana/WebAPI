using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class ShiftValidator : AbstractValidator<ShiftDto>
{
    public ShiftValidator()
    {
        RuleFor(r => r.CompanyKey)
            .NotNull().WithMessage("Company cannot be null or empty")
            .NotEqual(Guid.Empty).WithMessage("Company cannot be null or empty")
            .NotEmpty().WithMessage("Company cannot be null or empty");

        RuleFor(r => r.ShiftGroupName)
            .NotNull().WithMessage("Shift Group Name cannot be null or empty")
            .NotEmpty().WithMessage("Shift Group Name cannot be null or empty")
            .MaximumLength(200).WithMessage("Shift Group Name must not exceed 200 characters");
        RuleFor(r => r.MaxLimit).GreaterThan(0).WithMessage("Max Limit must be greater than zero")
                                .When(x => x.MaxLimit.HasValue);
        RuleFor(r => r.Description).MaximumLength(200).When(r => !string.IsNullOrEmpty(r.Description))
                                     .WithMessage("Description must not exceed 200 characters if provided.");
    }
}
