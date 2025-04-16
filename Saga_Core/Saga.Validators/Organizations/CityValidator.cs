using FluentValidation;
using Saga.Domain.Dtos.Organizations;

namespace Saga.Validators.Organizations;

public class CityValidator : AbstractValidator<CityDto>
{
    public CityValidator()
    {
        RuleFor(r => r.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
                            .MaximumLength(20).WithMessage("Code must not exceed 20 characters");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(r => r.Description).MaximumLength(200).WithMessage("Description must not exceed 200 characters");
        RuleFor(r => r.CountryKey).NotNull().NotEmpty().WithMessage("Country Key cannot be null or empty");
        RuleFor(r => r.ProvinceKey).NotNull().NotEmpty().WithMessage("Province Key cannot be null or empty");
    }
}