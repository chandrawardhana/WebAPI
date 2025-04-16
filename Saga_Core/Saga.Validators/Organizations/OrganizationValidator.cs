using FluentValidation;
using Saga.Domain.Dtos.Organizations;

namespace Saga.Validators.Organizations;

public class OrganizationValidator : AbstractValidator<OrganizationDto>
{
    public OrganizationValidator()
    {
        RuleFor(b => b.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
                            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");
        RuleFor(b => b.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}
