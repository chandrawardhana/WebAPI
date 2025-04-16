using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class LeaveSubmissionValidator : AbstractValidator<LeaveSubmissionDto>
{
    public LeaveSubmissionValidator()
    {
        RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee cannot be null or empty");
        RuleFor(r => r.LeaveKey).NotNull().NotEmpty().WithMessage("Leave cannot be null or empty");
        RuleFor(r => r.DateStart).Must(BeAValidDate).WithMessage("Date Start is required");
        RuleFor(r => r.DateEnd).Must(BeAValidDate).WithMessage("Date End is required");
        RuleFor(r => r.Duration).NotNull().NotEmpty().WithMessage("Duration cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Duration must be greater than zero");
        RuleFor(r => r.ApprovalStatus).IsInEnum().WithMessage("Not valid Approval Status");
        RuleFor(r => r.Description)
            .MaximumLength(200)
            .WithMessage("Reason cannot exceed 200 characters.")
            .When(r => !string.IsNullOrEmpty(r.Description));
        RuleFor(r => r.LeaveCode).NotNull().NotEmpty().WithMessage("Leave Code cannot be null or empty")
            .MaximumLength(10).WithMessage("Leave Code must not exceed 10 characters");
        RuleFor(r => r.Number).NotNull().NotEmpty().WithMessage("Number cannot be null or empty")
            .MaximumLength(18).WithMessage("Number must not exceed 18 characters");
        RuleFor(r => r.ApprovalTransactionKey).NotNull().NotEmpty().WithMessage("Approval Transaction cannot be null or empty");
        RuleFor(r => r)
            .Must(ValidateSubmissionDateRange)
            .WithMessage(r => $"Leave submission must be between {r.Leave?.MinSubmission} days and {r.Leave?.MaxSubmission} days before the leave start date");
    }

    private bool BeAValidDate(DateTime date)
    {
        if (date == default(DateTime))
            return false;
        return true;
    }

    private bool BeAValidDate(DateTime? date)
    {
        if (date == default(DateTime))
            return false;
        return true;
    }

    private bool ValidateSubmissionDateRange(LeaveSubmissionDto leaveSubmission)
    {
        if (leaveSubmission.Leave == null || !leaveSubmission.DateStart.HasValue)
            return false;

        var today = DateTime.Now.Date;
        var startDate = leaveSubmission.DateStart.Value.Date;
        var daysUntilStart = (startDate - today).Days;

        return daysUntilStart >= leaveSubmission.Leave.MinSubmission &&
               daysUntilStart <= leaveSubmission.Leave.MaxSubmission;
    }
}
