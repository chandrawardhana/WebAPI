using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeeTransferValidator : AbstractValidator<EmployeeTransferDto>
{
    public EmployeeTransferValidator()
    {
        RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee Key cannot be null or empty");
        RuleFor(r => r.TransferCategory).NotNull().NotEmpty().WithMessage("Transfer Categor cannot be null or empty")
                                        .IsInEnum().WithMessage("Not valid transfer category");
        RuleFor(r => r.EffectiveDate).NotNull().WithMessage("Effective Date cannot be null")
                                    .Must(BeAValidDate).WithMessage("Nationality Registered is not a valid date.");
        RuleFor(r => r.NewCompanyKey).NotNull().NotEmpty().WithMessage("New Company Key cannot be null or empty");
        RuleFor(r => r.NewOrganizationKey).NotNull().NotEmpty().WithMessage("New Organization Key cannot be null or empty");
        RuleFor(r => r.NewPositionKey).NotNull().NotEmpty().WithMessage("New Position Key cannot be null or empty");
        RuleFor(r => r.NewTitleKey).NotNull().NotEmpty().WithMessage("New Title Key cannot be null or empty");
        RuleFor(r => r.NewBranchKey).NotNull().NotEmpty().WithMessage("New Branch Key cannot be null or empty");
        RuleFor(r => r.TransferStatus).NotNull().NotEmpty().WithMessage("TransferStatus cannot be null or empty")
                                      .IsInEnum().WithMessage("Not valid transfer status");
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

public class CancelEmployeeTransferValidator : AbstractValidator<CancelEmployeeTransferDto>
{
    public CancelEmployeeTransferValidator()
    {
        RuleFor(x => x.EmployeeTransferKeys).NotEmpty().WithMessage("At least one transfer must be selected for cancellation.");

        RuleFor(r => r.CancelledReason).NotNull().NotEmpty().WithMessage("Cancelled Reason cannot be null or empty")
                                        .MaximumLength(300).WithMessage("Cancelled Reason must not exceed 300 characters"); ;
    }
}
