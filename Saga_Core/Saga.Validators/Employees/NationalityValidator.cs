using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class NationalityValidator : AbstractValidator<NationalityDto>
{
    public NationalityValidator()
    {
        RuleFor(r => r.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(x => x.Description)
        .MaximumLength(200)
        .WithMessage("Description cannot exceed 200 characters.")
        .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
