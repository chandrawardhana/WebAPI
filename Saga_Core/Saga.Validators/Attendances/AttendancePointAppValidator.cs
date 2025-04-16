using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class AttendancePointAppValidator : AbstractValidator<AttendancePointAppDto>
{
    public AttendancePointAppValidator()
    {
        RuleFor(x => x.EmployeeKey)
                .NotEmpty()
                .WithMessage("Employee is required.");

        RuleFor(x => x.Latitude)
            .NotEmpty()
            .WithMessage("Latitude is required.")
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.Longitude)
            .NotEmpty()
            .WithMessage("Longitude is required.")
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180 degrees.");

        RuleFor(x => x.InOutMode)
            .IsInEnum()
            .WithMessage("Invalid In/Out Mode value.");

        RuleFor(x => x.AbsenceTime)
            .NotEmpty()
            .WithMessage("Absence Time is required.")
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("Absence Time cannot be in the future.");
    }
}
