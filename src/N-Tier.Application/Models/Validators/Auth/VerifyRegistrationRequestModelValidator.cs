using FluentValidation;
using N_Tier.Application.Models.Auth;

namespace N_Tier.Application.Models.Validators.Auth;

public class VerifyRegistrationRequestModelValidator : AbstractValidator<VerifyRegistrationRequestModel>
{
    public VerifyRegistrationRequestModelValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email address is not valid");

        RuleFor(r => r.Code)
            .NotEmpty()
            .WithMessage("Verification code is required")
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must be exactly 6 digits");
    }
}
