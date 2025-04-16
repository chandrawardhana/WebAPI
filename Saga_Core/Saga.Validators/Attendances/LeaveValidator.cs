using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class LeaveValidator : AbstractValidator<LeaveDto>
{
    public LeaveValidator()
    {
        RuleFor(r => r.CompanyKey).NotNull().NotEmpty().WithMessage("Company cannot be null or empty");
        RuleFor(r => r.Code).NotNull().NotEmpty().WithMessage("Code cannot be null or empty")
                            .MaximumLength(10).WithMessage("Code must not exceed 10 characters");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage("Name cannot be null or empty")
                            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(r => r.MaxDays).NotNull().NotEmpty().WithMessage("Max Days cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Max Days must be greater than zero");
        RuleFor(r => r).Custom((leave, context) =>
        {
            if (leave.MinSubmission.HasValue && leave.MaxSubmission.HasValue)
            {
                if (Math.Abs(leave.MinSubmission.Value) > leave.MaxSubmission.Value)
                {
                    context.AddFailure("MaxSubmission", "Max Submission days must be greater than or equal to the absolute value of Min Submission days");
                }
            }
        });
        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
