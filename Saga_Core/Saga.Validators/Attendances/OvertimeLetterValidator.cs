using FluentValidation;
using Saga.Domain.Dtos.Attendances;
using System;

namespace Saga.Validators.Attendances;

public class OvertimeLetterValidator : AbstractValidator<OvertimeLetterDto>
{
    public OvertimeLetterValidator()
    {
        RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee cannot be null or empty");
        RuleFor(r => r.ApprovalTransactionKey).NotNull().NotEmpty().WithMessage("Approval Transaction cannot be null or empty");
        RuleFor(r => r.Number).NotNull().NotEmpty().WithMessage("Number cannot be null or empty")
            .MaximumLength(18).WithMessage("Number must not exceed 18 characters");
        RuleFor(r => r.DateSubmission).Must(BeAValidDate).WithMessage("Hire Date is required");
                
        // Combined validation for both overtime fields and duration
        RuleFor(x => x)
            .Custom((overtime, context) => {
                // Only check duration if both times are provided
                if (overtime.OvertimeIn != default && overtime.OvertimeOut != default)
                {
                    if (!IsValidOvertimeDuration(overtime.OvertimeIn, overtime.OvertimeOut))
                    {
                        context.AddFailure("OvertimeOut", "Overtime duration cannot exceed 8 hours.");
                    }
                }
            });
        
        RuleFor(x => x.Description)
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
            
        RuleFor(r => r.ApprovalStatus).IsInEnum().WithMessage("Not valid Approval Status");
    }

    private bool BeAValidDate(DateOnly date)
    {
        if (date == default(DateOnly))
            return false;
        return true;
    }

    private bool BeAValidDate(DateOnly? date)
    {
        if (date == default(DateOnly))
            return false;
        return true;
    }

    private bool IsValidOvertimeDuration(TimeOnly overtimeIn, TimeOnly overtimeOut)
    {
        // Calculate duration considering day boundary crossing
        TimeSpan duration;
        
        // If overtime ends on the next day (overtimeOut < overtimeIn)
        if (overtimeOut < overtimeIn)
        {
            // Add 24 hours to overtimeOut to get the correct duration
            var nextDayOut = overtimeOut.Add(TimeSpan.FromHours(24));
            duration = nextDayOut - overtimeIn;
        }
        else
        {
            duration = overtimeOut - overtimeIn;
        }
        
        // Check if duration is within 8 hours
        return duration.TotalHours <= 8;
    }
}
