using System.Text.RegularExpressions;
using FluentValidation;
using N_Tier.Application.Models.Auth;

namespace N_Tier.Application.Models.Validators.Auth;

public class ResetPasswordRequestModelValidator : AbstractValidator<ResetPasswordRequestModel>
{
    public ResetPasswordRequestModelValidator()
    {
        RuleFor(r => r.Token)
            .NotEmpty()
            .WithMessage("Reset token is required");

        RuleFor(r => r.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(256)
            .WithMessage("Password must not exceed 256 characters")
            .Must(password => !string.IsNullOrEmpty(password) && Regex.IsMatch(password, "[A-Z]"))
            .WithMessage("Password must contain at least one uppercase letter");
    }
}
