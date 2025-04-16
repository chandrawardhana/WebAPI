
using FluentValidation;
using Saga.Domain.Dtos.Payrolls;

namespace Saga.Validators.Payrolls;

/// <summary>
/// ashari.herman 2025-03-12 slipi jakarta
/// </summary>

public class BpjsConfigValidator : AbstractValidator<BpjsConfigDto>
{
    public BpjsConfigValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}

public class BpjsSubConfigValidator : AbstractValidator<BpjsSubConfigDto>
{
    public BpjsSubConfigValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}

