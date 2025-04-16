using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class OvertimeRateValidator : AbstractValidator<OvertimeRateDto>
{
    public OvertimeRateValidator()
    {
        RuleFor(r => r.CompanyKey).NotNull().NotEmpty().WithMessage("Company cannot be null or empty");
        RuleFor(r => r.GroupName).NotNull().NotEmpty().WithMessage("Group Name cannot be null or empty")
                                    .MaximumLength(200).WithMessage("Group Name must not exceed 200 characters");
        RuleFor(r => r.BaseOnDay).IsInEnum().WithMessage("Not valid Base On Day");
        RuleFor(r => r.MaxHour).NotNull().NotEmpty().WithMessage("Max Hour cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Max Hour must be greater than zero");
        RuleFor(r => r.OvertimeRateDetails).Must(x => x.Count <= 5).WithMessage("Formula cannot exceed 5 rows");
    }
}
