using FluentValidation;
using Saga.Domain.Dtos.Employees;


namespace Saga.Validators.Employees;

public class EmployeePersonalValidator : AbstractValidator<EmployeePersonalDto>
{
    public EmployeePersonalValidator() 
    {
        RuleSet("Create", () =>
        {
            RuleFor(r => r.NationalityNumber).NotNull().NotEmpty().WithMessage("NationalityNumber cannot be null or empty")
                                .Matches(@"^\d{16}$").WithMessage("Nationality Number must contain exactly 16 digits");
            RuleFor(r => r.PlaceOfBirth).NotNull().NotEmpty().WithMessage("Place Of Birth cannot be null or empty")
                   .MaximumLength(50).WithMessage("Place Of Birth must not exceed 50 characters");
            RuleFor(r => r.DateOfBirth).Must(BeAValidDate).WithMessage("Date of Birth is required");
            RuleFor(r => r.Gender).IsInEnum().WithMessage("Not valid gender");
            RuleFor(r => r.ReligionKey).NotNull().NotEmpty().WithMessage("Religion Key cannot be null or empty");
            RuleFor(r => r.MaritalStatus).IsInEnum().WithMessage("Not valid marital status");
            RuleFor(r => r.Address).NotNull().NotEmpty().WithMessage("Address cannot be null or empty")
                                .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
            RuleFor(r => r.CountryKey).NotNull().NotEmpty().WithMessage("Country Key cannot be null or empty");
            RuleFor(r => r.ProvinceKey).NotNull().NotEmpty().WithMessage("Province Key cannot be null or empty");
            RuleFor(r => r.CityKey).NotNull().NotEmpty().WithMessage("City Key cannot be null or empty");
            RuleFor(r => r.PostalCode).NotNull().NotEmpty().WithMessage("Postal Code cannot be null or empty")
                                    .Matches(@"^\d{5}$").WithMessage("Postal Code must contain exactly 5 digits");
            RuleFor(r => r.CurrentAddress).MaximumLength(200).When(r => !string.IsNullOrEmpty(r.CurrentAddress))
                                     .WithMessage("Current Address must not exceed 200 characters if provided.");
            RuleFor(r => r.CurrentPostalCode).Must(currentPostalCode => string.IsNullOrEmpty(currentPostalCode) || System.Text.RegularExpressions.Regex.IsMatch(currentPostalCode, @"^\d{5}$"))
                                     .WithMessage("Current Postal Code must contain exactly 5 digits if provided.");
            RuleFor(r => r.PhoneNumber).NotNull().NotEmpty().WithMessage("Phone Number cannot be null or empty")
                                   .Matches(@"^(?:\+62|62|0)[2-9]\d{7,11}$").WithMessage("Not Valid Phone Number")
                                   .MaximumLength(20).WithMessage("Phone Number must not exceed 20 characters");
            RuleFor(r => r.Email).NotNull().NotEmpty().WithMessage("Email cannot be null or empty")
                                 .EmailAddress().WithMessage("Not valid email address")
                                 .MaximumLength(100).WithMessage("Email must not exceed 100 characters");
            RuleFor(r => r.SocialMedia).MaximumLength(100).When(r => !string.IsNullOrEmpty(r.SocialMedia))
                                     .WithMessage("SocialMedia must not exceed 100 characters if provided.");
            RuleFor(r => r.EthnicKey).NotNull().NotEmpty().WithMessage("Ethnic Key cannot be null or empty");
            RuleFor(r => r.Weight).NotNull().NotEmpty().WithMessage("Weight cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Weight must be greater than zero");
            RuleFor(r => r.Height).NotNull().NotEmpty().WithMessage("Height cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Height must be greater than zero");
            RuleFor(r => r.Blood).IsInEnum().WithMessage("Not valid Blood Type");
            RuleFor(r => r.CitizenNumber).NotNull().NotEmpty().WithMessage("Citizen Number cannot be null or empty")
                                .Matches(@"^\d{16}$").WithMessage("Citizen Number must contain exactly 16 digits");
            RuleFor(r => r.CitizenRegistered).Must(BeAValidDateOnly).WithMessage("Citizen Registered cannot be null or empty");
            RuleFor(r => r.NationalityKey).NotNull().NotEmpty().WithMessage("Nationality cannot be null or empty");
        });

        RuleSet("Update", () =>
        {
            RuleFor(r => r.EmployeeKey).NotNull().NotEmpty().WithMessage("Employee Key cannot be null or empty");
            RuleFor(r => r.NationalityNumber).NotNull().NotEmpty().WithMessage("NationalityNumber cannot be null or empty")
                                    .Matches(@"^\d{16}$").WithMessage("Nationality Number must contain exactly 16 digits");
            RuleFor(r => r.NationalityRegistered).NotNull().NotEmpty().WithMessage("Nationality Registered cannot be null or empty")
                    .Must(BeAValidDate).WithMessage("Nationality Registered is not a valid date.");
            RuleFor(r => r.PlaceOfBirth).NotNull().NotEmpty().WithMessage("Place Of Birth cannot be null or empty")
                   .MaximumLength(50).WithMessage("Place Of Birth must not exceed 50 characters");
            RuleFor(r => r.DateOfBirth).Must(BeAValidDate).WithMessage("Date of Birth is required");
            RuleFor(r => r.Gender).IsInEnum().WithMessage("Not valid gender");
            RuleFor(r => r.ReligionKey).NotNull().NotEmpty().WithMessage("Religion Key cannot be null or empty");
            RuleFor(r => r.MaritalStatus).IsInEnum().WithMessage("Not valid marital status");
            RuleFor(r => r.Address).NotNull().NotEmpty().WithMessage("Address cannot be null or empty")
                                .MaximumLength(200).WithMessage("Address must not exceed 200 characters");
            RuleFor(r => r.CountryKey).NotNull().NotEmpty().WithMessage("Country Key cannot be null or empty");
            RuleFor(r => r.ProvinceKey).NotNull().NotEmpty().WithMessage("Province Key cannot be null or empty");
            RuleFor(r => r.CityKey).NotNull().NotEmpty().WithMessage("City Key cannot be null or empty");
            RuleFor(r => r.PostalCode).NotNull().NotEmpty().WithMessage("Postal Code cannot be null or empty")
                                    .Matches(@"^\d{5}$").WithMessage("Postal Code must contain exactly 5 digits");
            RuleFor(r => r.CurrentAddress).MaximumLength(200).When(r => !string.IsNullOrEmpty(r.CurrentAddress))
                                     .WithMessage("Current Address must not exceed 200 characters if provided.");
            RuleFor(r => r.CurrentPostalCode).Must(currentPostalCode => string.IsNullOrEmpty(currentPostalCode) || System.Text.RegularExpressions.Regex.IsMatch(currentPostalCode, @"^\d{5}$"))
                                     .WithMessage("Current Postal Code must contain exactly 5 digits if provided.");
            RuleFor(r => r.PhoneNumber).NotNull().NotEmpty().WithMessage("Phone Number cannot be null or empty")
                                   .Matches(@"^(?:\+62|62|0)[2-9]\d{7,11}$").WithMessage("Not Valid Phone Number")
                                   .MaximumLength(20).WithMessage("Phone Number must not exceed 20 characters");
            RuleFor(r => r.Email).NotNull().NotEmpty().WithMessage("Email cannot be null or empty")
                                 .EmailAddress().WithMessage("Not valid email address")
                                 .MaximumLength(100).WithMessage("Email must not exceed 100 characters");
            RuleFor(r => r.SocialMedia).MaximumLength(100).When(r => !string.IsNullOrEmpty(r.SocialMedia))
                                     .WithMessage("SocialMedia must not exceed 100 characters if provided.");
            RuleFor(r => r.EthnicKey).NotNull().NotEmpty().WithMessage("Ethnic Key cannot be null or empty");
            RuleFor(r => r.Weight).NotNull().NotEmpty().WithMessage("Weight cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Weight must be greater than zero");
            RuleFor(r => r.Height).NotNull().NotEmpty().WithMessage("Height cannot be zero or empty")
                                .GreaterThan(0).WithMessage("Height must be greater than zero");
            RuleFor(r => r.Blood).IsInEnum().WithMessage("Not valid Blood Type");
            RuleFor(r => r.CitizenNumber).NotNull().NotEmpty().WithMessage("Citizen Number cannot be null or empty")
                                .Matches(@"^\d{16}$").WithMessage("Citizen Number must contain exactly 16 digits");
            RuleFor(r => r.CitizenRegistered).Must(BeAValidDateOnly).WithMessage("Citizen Registered cannot be null or empty");
            RuleFor(r => r.NationalityKey).NotNull().NotEmpty().WithMessage("Nationality cannot be null or empty");
        });
    }

    private bool BeAValidDate(DateTime date)
    {
        if (date == default(DateTime))
            return false;
        return true;
    }

    private bool BeAValidDate(DateTime? date)
    {
        if (date == default(DateTime))
            return false;
        return true;
    }

    private bool BeAValidDateOnly(DateOnly date)
    {
        if (date == default(DateOnly))
            return false;
        return true;
    }

    private bool BeAValidDateOnly(DateOnly? date)
    {
        if (date == default(DateOnly))
            return false;
        return true;
    }
}
