using FluentValidation;
using Saga.Domain.Dtos.Organizations;

namespace Saga.Validators.Organizations;

public class CountryValidator : AbstractValidator<CountryDto>
{
    public CountryValidator()
    {
        RuleFor(r => r.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(r => r.Description).MaximumLength(200).WithMessage("Description must not exceed 200 characters");
    }
}
