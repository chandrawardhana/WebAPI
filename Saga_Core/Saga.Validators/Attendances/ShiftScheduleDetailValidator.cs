using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class ShiftScheduleDetailValidator : AbstractValidator<ShiftScheduleDetailDto>
{
    public ShiftScheduleDetailValidator() 
    {
        RuleFor(r => r.ShiftScheduleKey).NotNull().NotEmpty().WithMessage("Shift Schedule Key cannot be null or empty");
        RuleFor(r => r.ShiftDetailKey).NotNull().NotEmpty().WithMessage("Shift Detail Key cannot be null or empty");
        RuleFor(r => r.Date).Must(BeAValidDate).WithMessage("Date is required");
        RuleFor(r => r.ShiftName).NotNull().NotEmpty().WithMessage("Shift Name Key cannot be null or empty");
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
