using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeeAttendanceValidator : AbstractValidator<EmployeeAttendanceDto>
{
    public EmployeeAttendanceValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(x => x.FingerPrintID)
            .MaximumLength(20)
            .WithMessage("FingerPrintID cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.FingerPrintID));

            RuleFor(r => r.ShiftKey).NotNull().NotEmpty().WithMessage("Shift cannot be null or empty");
            RuleFor(r => r.ShiftScheduleKey).NotNull().NotEmpty().WithMessage("Shift Schedule cannot be null or empty");
            RuleFor(r => r.OvertimeMode).IsInEnum().WithMessage("Not valid Overtime Rate");
        });

        RuleSet("Update", () =>
        {
            RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee cannot be null or empty");
            RuleFor(x => x.FingerPrintID)
            .MaximumLength(20)
            .WithMessage("FingerPrintID cannot exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.FingerPrintID));
            RuleFor(r => r.ShiftKey).NotNull().NotEmpty().WithMessage("Shift cannot be null or empty");
            RuleFor(r => r.ShiftScheduleKey).NotNull().NotEmpty().WithMessage("Shift Schedule cannot be null or empty");
            RuleFor(r => r.OvertimeMode).IsInEnum().WithMessage("Not valid Overtime Rate");
        });
    }
}
