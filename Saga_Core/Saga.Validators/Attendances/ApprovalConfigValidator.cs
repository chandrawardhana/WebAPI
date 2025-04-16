using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class ApprovalConfigValidator : AbstractValidator<ApprovalConfigDto>
{
    public  ApprovalConfigValidator()
    {
        RuleFor(x => x.OrganizationKey).NotNull().NotEmpty().WithMessage("Organization cannot be null or empty");
        RuleFor(x => x.Description)
        .MaximumLength(200)
        .WithMessage("Description cannot exceed 200 characters.")
        .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
