
using FluentValidation;
using Saga.Domain.Dtos.Payrolls;

namespace Saga.Validators.Payrolls;

public class AverageEffectiveRateValidator : AbstractValidator<AverageEffectiveRateDto>
{
    public AverageEffectiveRateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}
