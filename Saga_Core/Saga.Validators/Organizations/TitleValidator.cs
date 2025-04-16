using FluentValidation;
using Saga.Domain.Dtos.Organizations;

namespace Saga.Validators.Organizations;

public class TitleValidator : AbstractValidator<TitleDto>
{
    public TitleValidator() 
    {
        RuleFor(r => r.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
        RuleFor(r => r.Description).MaximumLength(200).WithMessage("Description must not exceed 200 characters");
        RuleFor(r => r.CompanyKey).NotNull().NotEmpty().WithMessage("Company Key cannot be null or empty");
    }
}
