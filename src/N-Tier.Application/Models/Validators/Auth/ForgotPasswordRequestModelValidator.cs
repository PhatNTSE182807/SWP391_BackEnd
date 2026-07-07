using FluentValidation;
using N_Tier.Application.Models.Auth;

namespace N_Tier.Application.Models.Validators.Auth;

public class ForgotPasswordRequestModelValidator : AbstractValidator<ForgotPasswordRequestModel>
{
    public ForgotPasswordRequestModelValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email address is not valid");
    }
}
