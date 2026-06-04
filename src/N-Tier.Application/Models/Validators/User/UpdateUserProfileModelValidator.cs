using System.Text.RegularExpressions;
using FluentValidation;
using N_Tier.Application.Models.User;

namespace N_Tier.Application.Models.Validators.User;

public class UpdateUserProfileModelValidator : AbstractValidator<UpdateUserProfileModel>
{
    private const string VietnamPhoneRegex = @"^(0?)(3[2-9]|5[6|8|9]|7[0|6-9]|8[0-6|8|9]|9[0-4|6-9])[0-9]{7}$";

    public UpdateUserProfileModelValidator()
    {
        // ── Username ──────────────────────────────────────────────────────
        RuleFor(r => r.Username)
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(100)
            .WithMessage("Username must not exceed 100 characters")
            .When(r => !string.IsNullOrEmpty(r.Username));

        // ── Email ─────────────────────────────────────────────────────────
        RuleFor(r => r.Email)
            .EmailAddress()
            .WithMessage("Email address is not valid")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters")
            .When(r => !string.IsNullOrEmpty(r.Email));

        // ── Phone number ──────────────────────────────────────────────────
        RuleFor(r => r.Phonenumber)
            .Matches(VietnamPhoneRegex)
            .WithMessage("Phone number must be a valid Vietnamese phone number ")
            .When(r => !string.IsNullOrEmpty(r.Phonenumber));

        // ── Password ──────────────────────────────────────────────────────
        RuleFor(r => r.NewPassword)
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(256)
            .WithMessage("Password must not exceed 256 characters")
            .Must(password => !string.IsNullOrEmpty(password) && Regex.IsMatch(password, "[A-Z]"))
            .WithMessage("Password must contain at least one uppercase letter")
            .When(r => !string.IsNullOrEmpty(r.NewPassword));
    }
}
