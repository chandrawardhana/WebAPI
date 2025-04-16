using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class AttendanceValidator : AbstractValidator<AttendanceDto>
{
    public AttendanceValidator()
    {
        RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee Key cannot be null or empty");
        RuleFor(r => r.AttendanceDate).Must(BeAValidDate).WithMessage("Attendance Date is required");
        RuleFor(r => r.In).NotNull().NotEmpty().WithMessage("In cannot be null or empty");
        RuleFor(r => r.Out).NotNull().NotEmpty().WithMessage("Out cannot be null or empty");
        RuleFor(r => r.ShiftName).NotNull().NotEmpty().WithMessage("Shift Name cannot be null or empty")
                                 .MaximumLength(100).WithMessage("Shift Name cannot exceed 100 characters");
        RuleFor(r => r.Status).IsInEnum().WithMessage("Not valid Attendance Status");
        RuleFor(x => x.Description)
        .MaximumLength(200)
        .WithMessage("Description cannot exceed 200 characters.")
        .When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0)
            .WithMessage("Latitude must be between -90 and 90 degrees.")
            .When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0)
            .WithMessage("Longitude must be between -180 and 180 degrees.")
            .When(x => x.Longitude.HasValue);
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
