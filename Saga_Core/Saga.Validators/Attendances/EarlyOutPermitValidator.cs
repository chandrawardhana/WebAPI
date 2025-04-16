using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class EarlyOutPermitValidator : AbstractValidator<EarlyOutPermitDto>
{
    public EarlyOutPermitValidator()
    {
        RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee cannot be null or empty");
        RuleFor(r => r.ApprovalTransactionKey).NotNull().NotEmpty().WithMessage("Approval Transaction cannot be null or empty");
        RuleFor(r => r.Number).NotNull().NotEmpty().WithMessage("Number cannot be null or empty")
            .MaximumLength(18).WithMessage("Number must not exceed 18 characters");
        RuleFor(r => r.DateSubmission).Must(BeAValidDate).WithMessage("Hire Date is required");
        RuleFor(x => x.TimeOut)
                .NotEmpty()
                .WithMessage("Time out is required.")
                .Must(BeValidTime)
                .WithMessage("Invalid time out value.");
        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(r => r.ApprovalStatus).IsInEnum().WithMessage("Not valid Approval Status");
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

    private bool BeValidTime(TimeOnly? time)
    {
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(17, 0);
        return time >= startTime && time <= endTime;
    }
}
