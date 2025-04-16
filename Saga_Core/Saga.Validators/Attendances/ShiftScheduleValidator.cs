using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class ShiftScheduleValidator : AbstractValidator<ShiftScheduleDto>
{
    public ShiftScheduleValidator()
    {
        RuleFor(r => r.CompanyKey).NotNull().NotEmpty().WithMessage("Company cannot be null or empty");
        RuleFor(r => r.GroupName).NotNull().NotEmpty().WithMessage("Group Name cannot be null or empty")
                                    .MaximumLength(200).WithMessage("Group Name must not exceed 200 characters");
        RuleFor(r => r.YearPeriod).GreaterThanOrEqualTo(1000).WithMessage("Year Period must be a 4-digit number.")
                                    .LessThanOrEqualTo(9999).WithMessage("Year Period must be a 4-digit number.");
        RuleFor(r => r.MonthPeriod).IsInEnum().WithMessage("Not valid Month Name");
    }
}
