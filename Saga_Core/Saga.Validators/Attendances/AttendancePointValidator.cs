using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class AttendancePointValidator : AbstractValidator<AttendancePointDto>
{
    public AttendancePointValidator()
    {
        RuleFor(r => r.CompanyKey).NotNull().NotEmpty().WithMessage("Company cannot be null or empty");
        RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(10).WithMessage("Code cannot exceed 10 characters.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name cannot exceed than 50 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.Latitude)
            .NotNull().WithMessage("Latitude is required.")
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");
        RuleFor(x => x.Longitude)
            .NotNull().WithMessage("Longitude is required.")
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");
        RuleFor(x => x.RangeTolerance)
            .NotNull().WithMessage("Range Tolerance is required.")
            .GreaterThan(0).WithMessage("Range Tolerance must be a positive number.");
    }
}
