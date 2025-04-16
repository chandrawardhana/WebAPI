
using FluentValidation;
using Saga.Domain.Dtos.Payrolls;

namespace Saga.Validators.Payrolls;

public class PayrollTaxConfigValidator : AbstractValidator<PayrollTaxConfigDto>
{
    public PayrollTaxConfigValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}
