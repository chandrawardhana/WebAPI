using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class ApprovalStampValidator : AbstractValidator<ApprovalStampDto>
{
    public ApprovalStampValidator()
    {
        RuleFor(r => r.ApprovalTransactionKey).NotNull().NotEmpty().WithMessage("Approval Transaction cannot be null or empty");
        RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Approver cannot be null or empty");
        RuleFor(r => r.Level).NotNull().WithMessage("Level must not be null")
                             .GreaterThan(0).WithMessage("Level must be greater than zero");
        RuleFor(r => r.Status).IsInEnum().WithMessage("Not valid Approval Status");
        RuleFor(x => x.RejectReason)
        .MaximumLength(200)
        .WithMessage("Reject Reason cannot exceed 200 characters.")
        .When(x => !string.IsNullOrEmpty(x.RejectReason));
        RuleFor(r => r.DateStamp).Must(BeAValidDate).WithMessage("Date Stamp is required");
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
}
