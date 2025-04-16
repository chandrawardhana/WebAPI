using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class LatePermitValidator : AbstractValidator<LatePermitDto>
{
    public LatePermitValidator()
    {
        RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee cannot be null or empty");

        RuleFor(x => x.DateSubmission)
                .NotEmpty()
                .WithMessage("Date submission is required.")
                .Must(date => date <= DateTime.Now)
                .WithMessage("Date submission cannot be in the future.");

        RuleFor(x => x.TimeIn)
            .NotEmpty()
            .WithMessage("Time in is required.");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x)
            .Must(x => IsValidTimeIn(x.DateSubmission, x.TimeIn))
            .WithMessage("Time in must be within the same day as date submission.")
            .When(x => x.DateSubmission.HasValue && x.TimeIn.HasValue);

        RuleFor(r => r.ApprovalStatus).IsInEnum().WithMessage("Not valid Approval Status");

        RuleFor(r => r.Number).NotNull().NotEmpty().WithMessage("Number cannot be null or empty")
            .MaximumLength(18).WithMessage("Number must not exceed 18 characters");
        RuleFor(r => r.ApprovalTransactionKey).NotNull().NotEmpty().WithMessage("Approval Transaction cannot be null or empty");
    }

    private bool IsValidTimeIn(DateTime? dateSubmission, TimeOnly? timeIn)
    {
        if (!dateSubmission.HasValue || !timeIn.HasValue)
            return true;

        var timeInDateTime = dateSubmission.Value.Date.Add(timeIn.Value.ToTimeSpan());
        return timeInDateTime.Date == dateSubmission.Value.Date;
    }
}
