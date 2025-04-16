using FluentValidation;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Validators.Attendances;

public class ShiftDetailValidator : AbstractValidator<ShiftDetailDto>
{
    public ShiftDetailValidator()
    {
        RuleFor(r => r.Day).IsInEnum().WithMessage("Not valid Day");
        RuleFor(r => r.WorkName).NotNull().NotEmpty().WithMessage("Work Name cannot be null or empty")
                                .MaximumLength(50).WithMessage("Work Name must not exceed 50 characters");
        RuleFor(r => r.WorkType).IsInEnum().WithMessage("Not valid Work Type");
        RuleFor(r => r.In).MaximumLength(5).When(r => !string.IsNullOrEmpty(r.In))
                            .WithMessage("In must not exceed 5 characters if provided");
        RuleFor(r => r.Out).MaximumLength(5).When(r => !string.IsNullOrEmpty(r.Out))
                           .WithMessage("Out must not exceed 5 characters if provided");
        RuleFor(r => r.EarlyIn).MaximumLength(5).When(r => !string.IsNullOrEmpty(r.EarlyIn))
                                 .WithMessage("Early In must not exceed 5 characters if provided.");
        RuleFor(r => r.MaxOut).MaximumLength(5).When(r => !string.IsNullOrEmpty(r.MaxOut))
                                 .WithMessage("Early In must not exceed 5 characters if provided.");
        //RuleFor(r => r.LateTolerance).NotNull().NotEmpty().WithMessage("Late Tolerance cannot be zero or empty")
        //                    .GreaterThan(0).WithMessage("Late Tolerance must be greater than zero");

        //RuleSet("Create", () =>
        //{
        //    RuleFor(r => r.Day).IsInEnum().WithMessage("Not valid Day");
        //    RuleFor(r => r.WorkName).NotNull().NotEmpty().WithMessage("Work Name cannot be null or empty")
        //                            .MaximumLength(50).WithMessage("Work Name must not exceed 50 characters");
        //    RuleFor(r => r.WorkType).IsInEnum().WithMessage("Not valid Work Type");
        //    RuleFor(r => r.In).NotNull().NotEmpty().WithMessage("In cannot be null or empty")
        //                            .MaximumLength(5).WithMessage("In must not exceed 5 characters");
        //    RuleFor(r => r.Out).NotNull().NotEmpty().WithMessage("Out cannot be null or empty")
        //                            .MaximumLength(5).WithMessage("Out must not exceed 5 characters");
        //    RuleFor(r => r.EarlyIn).MaximumLength(5).When(r => !string.IsNullOrEmpty(r.EarlyIn))
        //                             .WithMessage("Early In must not exceed 5 characters if provided.");
        //    RuleFor(r => r.MaxOut).MaximumLength(5).When(r => !string.IsNullOrEmpty(r.MaxOut))
        //                             .WithMessage("Early In must not exceed 5 characters if provided.");
        //    RuleFor(r => r.LateTolerance).NotNull().NotEmpty().WithMessage("Late Tolerance cannot be zero or empty")
        //                        .GreaterThan(0).WithMessage("Late Tolerance must be greater than zero");
        //});

        //RuleSet("Update", () =>
        //{
        //    RuleFor(r => r.ShiftKey).NotNull().NotEmpty().WithMessage("Shift Key cannot be null or empty");
        //    RuleFor(r => r.Day).IsInEnum().WithMessage("Not valid Day");
        //    RuleFor(r => r.WorkName).NotNull().NotEmpty().WithMessage("Work Name cannot be null or empty")
        //                            .MaximumLength(50).WithMessage("Work Name must not exceed 50 characters");
        //    RuleFor(r => r.WorkType).IsInEnum().WithMessage("Not valid Work Type");
        //    RuleFor(r => r.In).NotNull().NotEmpty().WithMessage("In cannot be null or empty")
        //                            .MaximumLength(5).WithMessage("In must not exceed 5 characters");
        //    RuleFor(r => r.Out).NotNull().NotEmpty().WithMessage("Out cannot be null or empty")
        //                            .MaximumLength(5).WithMessage("Out must not exceed 5 characters");
        //    RuleFor(r => r.EarlyIn).MaximumLength(5).When(r => !string.IsNullOrEmpty(r.EarlyIn))
        //                             .WithMessage("Early In must not exceed 5 characters if provided.");
        //    RuleFor(r => r.MaxOut).MaximumLength(5).When(r => !string.IsNullOrEmpty(r.MaxOut))
        //                             .WithMessage("Early In must not exceed 5 characters if provided.");
        //    RuleFor(r => r.LateTolerance).NotNull().NotEmpty().WithMessage("Late Tolerance cannot be zero or empty")
        //                        .GreaterThan(0).WithMessage("Late Tolerance must be greater than zero");
        //});
    }
}
