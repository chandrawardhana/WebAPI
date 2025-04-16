using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class OvertimeRateDetailValidator : AbstractValidator<OvertimeRateDetailDto>
{
    public OvertimeRateDetailValidator()
    {
        RuleFor(r => r.Level).NotNull().NotEmpty().WithMessage("Level cannot be zero or empty")
                            .GreaterThan(0).WithMessage("Level must be greater than zero");
        RuleFor(r => r.Hours).NotNull().WithMessage("Hours cannot be zero or empty")
                            .GreaterThan(0).WithMessage("Hours must be greater than zero");
        RuleFor(r => r.Multiply).NotNull().WithMessage("Multiply cannot be zero or empty")
                            .GreaterThanOrEqualTo(1.5f).WithMessage("Multiply value cannot be lower than 1.5");
    }
}
