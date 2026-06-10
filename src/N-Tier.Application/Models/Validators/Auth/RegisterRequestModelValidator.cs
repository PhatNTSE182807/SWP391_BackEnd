using System.Text.RegularExpressions;
using FluentValidation;
using N_Tier.Application.Models.Auth;

namespace N_Tier.Application.Models.Validators.Auth;

public class RegisterRequestModelValidator : AbstractValidator<RegisterRequestModel>
{

    private const string VietnamPhoneRegex = @"^(0?)(3[2-9]|5[6|8|9]|7[0|6-9]|8[0-6|8|9]|9[0-4|6-9])[0-9]{7}$";

    public RegisterRequestModelValidator()
    {

        RuleFor(r => r.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(100)
            .WithMessage("Username must not exceed 100 characters");

        RuleFor(r => r.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email address is not valid")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters");

        RuleFor(r => r.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(VietnamPhoneRegex)
            .WithMessage("Phone number must be a valid Vietnamese phone number");

        RuleFor(r => r.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(256)
            .WithMessage("Password must not exceed 256 characters")
            .Must(password => !string.IsNullOrEmpty(password) && Regex.IsMatch(password, "[A-Z]"))
            .WithMessage("Password must contain at least one uppercase letter");

        RuleFor(r => r.RoleName)
            .IsInEnum()
            .WithMessage("Role is required and must be a valid role (Student, Lecturer, Researcher)");
    }
}
