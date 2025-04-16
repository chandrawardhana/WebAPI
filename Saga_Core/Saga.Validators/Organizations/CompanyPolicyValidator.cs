using FluentValidation;
using Saga.Domain.Dtos.Organizations;

namespace Saga.Validators.Organizations;

public class CompanyPolicyValidator : AbstractValidator<CompanyPolicyDto>
{
    public CompanyPolicyValidator()
    {
        RuleFor(r => r.CompanyKey).NotNull().NotEmpty().WithMessage("Company Key cannot be null or empty");
        RuleFor(r => r.OrganizationKey).NotNull().NotEmpty().WithMessage("Organization Key cannot be null");
        RuleFor(r => r.EffectiveDate).NotNull().NotEmpty().WithMessage("Effective Date cannot be null")
                                     .Must(BeAValidDate).WithMessage("Not valid Date");
    }

    private bool BeAValidDate(DateTime date)
    {
        return !date.Equals(default(DateTime));
    }
}
