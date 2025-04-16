using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class ApprovalTransactionValidator : AbstractValidator<ApprovalTransactionDto>
{
    public ApprovalTransactionValidator()
    {
        RuleFor(x => x.EmployeeKey).NotNull().NotEmpty().WithMessage("Submitter is required.");
        RuleFor(r => r.ApprovalTransactionDate).Must(BeAValidDate).WithMessage("Approval Transaction Date is required");
        RuleFor(r => r.Category).IsInEnum().WithMessage("Not valid Submission Category");
        RuleFor(r => r.ApprovalStatus).IsInEnum().WithMessage("Not valid Approval Status");
        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
        RuleFor(x => x.RejectReason)
            .MaximumLength(200)
            .WithMessage("Reject Reason cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.RejectReason));
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
