using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeeAttendanceDetailValidator : AbstractValidator<EmployeeAttendanceDetailDto>
{
    public EmployeeAttendanceDetailValidator()
    {
        RuleSet("Create", () =>
        {
            RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
            RuleFor(r => r.Quota).NotNull().NotEmpty().WithMessage("Quota cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Quota must be greater than zero");
            //RuleFor(r => r.Used).GreaterThan(0).WithMessage("Used must be greater than zero")
            //                    .When(x => x.Used.HasValue);
            RuleFor(r => r.Credit).GreaterThan(0).WithMessage("Credit must be greater than zero")
                                .When(x => x.Credit.HasValue);
            RuleFor(r => r.ExpiredAt).Must(BeAValidDate).WithMessage("ExpiredAt is required");
            RuleFor(r => r.Category).IsInEnum().WithMessage("Not valid Leave Category");
            RuleFor(r => r.Priority).NotNull().NotEmpty().WithMessage("Priority cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Priority must be greater than zero");
        });

        RuleSet("Update", () =>
        {
            RuleFor(r => r.EmployeeAttendanceKey).NotNull().NotEmpty().WithMessage("Employee Attendance Key cannot be null or empty");
            RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
            RuleFor(r => r.Quota).NotNull().NotEmpty().WithMessage("Quota cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Quota must be greater than zero");
            //RuleFor(r => r.Used).GreaterThan(0).WithMessage("Used must be greater than zero")
            //                    .When(x => x.Used.HasValue);
            RuleFor(r => r.Credit).GreaterThan(0).WithMessage("Credit must be greater than zero")
                                .When(x => x.Credit.HasValue);
            RuleFor(r => r.ExpiredAt).Must(BeAValidDate).WithMessage("ExpiredAt is required");
            RuleFor(r => r.Category).IsInEnum().WithMessage("Not valid Leave Category");
            RuleFor(r => r.Priority).NotNull().NotEmpty().WithMessage("Priority cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Priority must be greater than zero");
        });
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
