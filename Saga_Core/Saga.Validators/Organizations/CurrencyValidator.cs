using FluentValidation;
using Saga.Domain.Dtos.Organizations;

namespace Saga.Validators.Organizations;

public class CurrencyValidator : AbstractValidator<CurrencyDto>
{
    public CurrencyValidator()
    {
        RuleFor(r => r.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
                    .MaximumLength(3).WithMessage("Code must not exceed 3 characters");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
        RuleFor(r => r.Symbol).NotNull().NotEmpty().WithMessage("Symbol cannot be null or empty")
            .MaximumLength(5).WithMessage("Symbol must not exceed 5 characters");
        RuleFor(r => r.SymbolPosition).NotNull().NotEmpty().WithMessage("Symbol Position cannot be null")
            .IsInEnum().WithMessage("Not valid symbol position");
        RuleFor(r => r.Description).MaximumLength(200).WithMessage("Description must not exceed 200 characters");
    }
}
