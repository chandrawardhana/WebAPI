using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class CutOffValidator : AbstractValidator<CutOffDto>
{
    public CutOffValidator()
    {
        RuleFor(r => r.CompanyKey).NotNull().NotEmpty().WithMessage("Company cannot be null or empty");
        RuleFor(r => r.YearPeriod).GreaterThanOrEqualTo(1000).WithMessage("Year Period must be a 4-digit number.")
                                    .LessThanOrEqualTo(9999).WithMessage("Year Period must be a 4-digit number.");
        RuleFor(x => x.Description)
                .MaximumLength(200)
                .WithMessage("Description cannot exceed 200 characters.");

        RuleFor(x => x.JanStart)
            .NotEmpty().WithMessage("January Start Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("January Start Day must be between 1 and 31.");

        RuleFor(x => x.JanEnd)
            .NotEmpty().WithMessage("January End Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("January End Day must be between 1 and 31.")
            .GreaterThanOrEqualTo(x => x.JanStart)
            .WithMessage("January End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.FebStart)
            .NotEmpty().WithMessage("February Start Day cannot be null or empty")
            .InclusiveBetween(1, 29)
            .WithMessage("February Start Day must be between 1 and 29.");

        RuleFor(x => x.FebEnd)
            .NotEmpty().WithMessage("February End Day cannot be null or empty")
            .InclusiveBetween(1, 29)
            .WithMessage("February End Day must be between 1 and 29.")
            .GreaterThanOrEqualTo(x => x.FebStart)
            .WithMessage("February End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.MarStart)
            .NotEmpty().WithMessage("March Start Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("March Start Day must be between 1 and 31.");

        RuleFor(x => x.MarEnd)
            .NotEmpty().WithMessage("March End Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("March End Day must be between 1 and 31.")
            .GreaterThanOrEqualTo(x => x.MarStart)
            .WithMessage("March End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.AprStart)
            .NotEmpty().WithMessage("April Start Day cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("April Start Day must be between 1 and 30.");

        RuleFor(x => x.AprEnd)
            .NotEmpty().WithMessage("April End Day cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("April End Day must be between 1 and 30.")
            .GreaterThanOrEqualTo(x => x.AprStart)
            .WithMessage("April End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.MayStart)
            .NotEmpty().WithMessage("May Start Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("May Start Day must be between 1 and 31.");

        RuleFor(x => x.MayEnd)
            .NotEmpty().WithMessage("May End Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("May End Day must be between 1 and 31.")
            .GreaterThanOrEqualTo(x => x.MayStart)
            .WithMessage("May End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.JunStart)
            .NotEmpty().WithMessage("June Start Day cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("June Start Day must be between 1 and 30.");

        RuleFor(x => x.JunEnd)
            .NotEmpty().WithMessage("June End Day cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("June End Day must be between 1 and 30.")
            .GreaterThanOrEqualTo(x => x.JunStart)
            .WithMessage("June End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.JulStart)
            .NotEmpty().WithMessage("July Start Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("July Start Day must be between 1 and 31.");

        RuleFor(x => x.JulEnd)
            .NotEmpty().WithMessage("July End Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("July End Day must be between 1 and 31.")
            .GreaterThanOrEqualTo(x => x.JulStart)
            .WithMessage("July End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.AugStart)
            .NotEmpty().WithMessage("August Start Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("August Start Day must be between 1 and 31.");

        RuleFor(x => x.AugEnd)
            .NotEmpty().WithMessage("August End Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("August End Day must be between 1 and 31.")
            .GreaterThanOrEqualTo(x => x.AugStart)
            .WithMessage("August End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.SepStart)
            .NotEmpty().WithMessage("September Start Day cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("September Start Day must be between 1 and 30.");

        RuleFor(x => x.SepEnd)
            .NotEmpty().WithMessage("September End Day cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("September End Day must be between 1 and 30.")
            .GreaterThanOrEqualTo(x => x.SepStart)
            .WithMessage("September End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.OctStart)
            .NotEmpty().WithMessage("October Start Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("October Start Day must be between 1 and 31.");

        RuleFor(x => x.OctEnd)
            .NotEmpty().WithMessage("October End Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("October End Day must be between 1 and 31.")
            .GreaterThanOrEqualTo(x => x.OctStart)
            .WithMessage("October End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.NovStart)
            .NotEmpty().WithMessage("November Start Day cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("November Start Day must be between 1 and 30.");

        RuleFor(x => x.NovEnd)
            .NotEmpty().WithMessage("November End Day cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("November End Day must be between 1 and 30.")
            .GreaterThanOrEqualTo(x => x.NovStart)
            .WithMessage("November End Day must be greater than or equal to Start Day.");

        RuleFor(x => x.DecStart)
            .NotEmpty().WithMessage("December Start Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("December Start Day must be between 1 and 31.");

        RuleFor(x => x.DecEnd)
            .NotEmpty().WithMessage("December End Day cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("December End Day must be between 1 and 31.")
            .GreaterThanOrEqualTo(x => x.DecStart)
            .WithMessage("December End Day must be greater than or equal to Start Day.");
    }
}
