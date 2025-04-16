using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class ApproverValidator : AbstractValidator<ApproverDto>
{
    public ApproverValidator()
    {
        RuleFor(x => x.ApprovalConfigKey).NotNull().NotEmpty().WithMessage("Approval Config cannot be null or empty");
        RuleFor(x => x.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee cannot be null or empty");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(r => r.Level).NotNull().WithMessage("Level must not be null")
                             .GreaterThan(0).WithMessage("Level must be greater than zero");
        RuleFor(r => r.Action).NotNull().NotEmpty().WithMessage("Action cannot be null or empty")
            .MaximumLength(20).WithMessage("Action must not exceed 20 characters");
    }
}
