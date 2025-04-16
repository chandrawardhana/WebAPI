
using FluentValidation;
using Saga.Domain.Dtos.Payrolls;

namespace Saga.Validators.Payrolls;

/// <summary>
/// ashari.herman 2025-03-07 slipi jakarta
/// </summary>

public class PayslipTemplateValidator : AbstractValidator<PayslipTemplateDto>
{
    public PayslipTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}


