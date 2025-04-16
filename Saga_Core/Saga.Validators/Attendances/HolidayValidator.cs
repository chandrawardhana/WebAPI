using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class HolidayValidator : AbstractValidator<HolidayDto>
{
    public HolidayValidator() 
    {
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(100).WithMessage("Name must not exceed 50 characters");
        RuleFor(r => r.Duration).NotNull().NotEmpty().WithMessage("Duration cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Duration must be greater than zero");
        RuleFor(r => r.Description).MaximumLength(200).WithMessage("Description must not exceed 200 characters");
        RuleFor(r => r.DateEvent).Must(BeAValidDate).WithMessage("Date Event is required");
    }

    private bool BeAValidDate(DateOnly date)
    {
        if (date == default(DateOnly))
            return false;
        return true;
    }

    private bool BeAValidDate(DateOnly? date)
    {
        if (date == default(DateOnly))
            return false;
        return true;
    }
}
