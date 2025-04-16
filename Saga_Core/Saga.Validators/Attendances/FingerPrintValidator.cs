using FluentValidation;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Enums;

namespace Saga.Validators.Attendances;

public class FingerPrintValidator : AbstractValidator<FingerPrintDto>
{
    public FingerPrintValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code cannot be null or empty")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name cannor be null or empty")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Invalid Connection Method");

        RuleFor(x => x.IPAddress)
            .NotEmpty().WithMessage("IP Address cannot be null or empty")
            .MaximumLength(50).WithMessage("IP Address cannot exceed 50 characters");

        RuleFor(x => x.Port)
            .GreaterThan(0).WithMessage("Port must be a positive number");

        RuleFor(r => r.CommKey).MaximumLength(100).When(r => !string.IsNullOrEmpty(r.CommKey))
                                     .WithMessage("Comm Key must not exceed 100 characters if provided.");
        RuleFor(r => r.Comm).MaximumLength(50).When(r => !string.IsNullOrEmpty(r.Comm))
                                     .WithMessage("Comm must not exceed 50 characters if provided.");
        RuleFor(x => x.Baudrate)
            .Must(baudrate =>
                baudrate == null ||
                (baudrate >= 9600 && baudrate <= 115200)
            ).WithMessage("Baudrate must be between 9600 and 115200 when specified");

        RuleFor(x => x.RetrieveScheduleTimes)
            .Must(times =>
                times == null ||
                (times.Length > 0 && times.Length <= 10)
            ).WithMessage("RetrieveScheduleTimes must have between 1 and 10 time slots")
            .When(x => x.RetrieveScheduleTimes != null)
            .Must(times =>
                times == null ||
                times.All(time =>
                    time >= TimeSpan.Zero &&
                    time < TimeSpan.FromDays(1)
                )
            ).WithMessage("Each schedule time must be between 00:00:00 and 23:59:59");

        RuleFor(x => x.SerialNumber)
        .NotEmpty().When(x => x.Method == ConnectionMethod.Web)
        .WithMessage("Serial Number is required when Connection Method is Web")
        .MaximumLength(15).When(r => !string.IsNullOrEmpty(r.SerialNumber))
        .WithMessage("Serial Number must not exceed 15 characters if provided");
    }
}
