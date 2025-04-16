using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class StandardWorkingValidator : AbstractValidator<StandardWorkingDto>
{
    public StandardWorkingValidator()
    {
        RuleFor(r => r.CompanyKey).NotNull().NotEmpty().WithMessage("Company cannot be null or empty");
        RuleFor(r => r.YearPeriod).GreaterThanOrEqualTo(1000).WithMessage("Year Period must be a 4-digit number.")
                                            .LessThanOrEqualTo(9999).WithMessage("Year Period must be a 4-digit number.");
        RuleFor(x => x.January)
            .NotEmpty().WithMessage("January cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("January must be between 1 and 31.");

        RuleFor(x => x.February)
            .NotEmpty().WithMessage("February cannot be null or empty")
            .InclusiveBetween(1, 29)
            .WithMessage("February must be between 1 and 29.");

        RuleFor(x => x.March)
            .NotEmpty().WithMessage("March cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("March must be between 1 and 31.");

        RuleFor(x => x.April)
            .NotEmpty().WithMessage("April cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("April must be between 1 and 30.");

        RuleFor(x => x.May)
            .NotEmpty().WithMessage("May cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("May must be between 1 and 31.");

        RuleFor(x => x.June)
            .NotEmpty().WithMessage("June cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("June must be between 1 and 30.");

        RuleFor(x => x.July)
            .NotEmpty().WithMessage("July cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("July must be between 1 and 31.");

        RuleFor(x => x.August)
                    .NotEmpty().WithMessage("August cannot be null or empty")
                    .InclusiveBetween(1, 31)
                    .WithMessage("August must be between 1 and 31.");

        RuleFor(x => x.September)
            .NotEmpty().WithMessage("September cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("September must be between 1 and 30.");

        RuleFor(x => x.October)
            .NotEmpty().WithMessage("October cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("October must be between 1 and 31.");

        RuleFor(x => x.November)
            .NotEmpty().WithMessage("November cannot be null or empty")
            .InclusiveBetween(1, 30)
            .WithMessage("November must be between 1 and 30.");

        RuleFor(x => x.December)
            .NotEmpty().WithMessage("December cannot be null or empty")
            .InclusiveBetween(1, 31)
            .WithMessage("December must be between 1 and 31.");

        RuleFor(r => r.Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters.")
            .When(r => !string.IsNullOrEmpty(r.Description));
    }
}
