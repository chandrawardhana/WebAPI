using FluentValidation;
using Saga.Domain.Dtos.Organizations;

namespace Saga.Validators.Organizations;

public class TitleQualificationValidator : AbstractValidator<TitleQualificationDto>
{
    public TitleQualificationValidator()
    {
        RuleFor(r => r.TitleKey).NotNull().NotEmpty().WithMessage("Title Key cannot be null or empty");
        RuleFor(r => r.EducationKey).NotNull().NotEmpty().WithMessage("Education Key cannot be null or empty");
        RuleFor(r => r.PositionKey).NotNull().NotEmpty().WithMessage("Position Key cannot be null or empty");
        RuleFor(r => r.MinExperience).NotNull().NotEmpty().WithMessage("Min Experience cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Min Experience must be greater than zero");
    }
}
