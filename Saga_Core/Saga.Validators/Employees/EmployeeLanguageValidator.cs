using FluentValidation;
using Saga.Domain.Dtos.Employees;

namespace Saga.Validators.Employees;

public class EmployeeLanguageValidator : AbstractValidator<EmployeeLanguageDto>
{
    public EmployeeLanguageValidator() 
    {
        RuleSet("Create", () =>
        {
            RuleFor(r => r.LanguageKey).NotNull().NotEmpty().WithMessage("Language Key cannot be null or empty");
            RuleFor(r => r.SpeakLevel).IsInEnum().WithMessage("Not valid speak level");
            RuleFor(r => r.ListenLevel).IsInEnum().WithMessage("Not valid listen level");
        });

        RuleSet("Update", () =>
        {
            RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee Key cannot be null or empty");
            RuleFor(r => r.LanguageKey).NotNull().NotEmpty().WithMessage("Language Key cannot be null or empty");
            RuleFor(r => r.SpeakLevel).IsInEnum().WithMessage("Not valid speak level");
            RuleFor(r => r.ListenLevel).IsInEnum().WithMessage("Not valid listen level");
        });
    }
}
