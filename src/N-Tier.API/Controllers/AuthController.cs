using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Auth;
using N_Tier.Application.Services;
using Microsoft.AspNetCore.Http;

namespace N_Tier.API.Controllers;

[Tags("UserAuthentication")]
public class AuthController(IAuthService authService) : ApiController
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync(LoginRequestModel loginRequestModel)
    {
        return Ok(ApiResult<LoginResponseModel>.Success(await authService.LoginAsync(loginRequestModel)));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync(RegisterRequestModel registerRequestModel)
    {
        var result = await authService.RegisterAsync(registerRequestModel);
        return StatusCode(201, ApiResult<RegisterResponseModel>.Success(result));
    }

    [HttpPost("verify-registration")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyRegistrationAsync(VerifyRegistrationRequestModel model)
    {
        var result = await authService.VerifyRegistrationAsync(model);
        return Ok(ApiResult<bool>.Success(result));
    }

    [HttpPost("resend-confirmation-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendConfirmationCodeAsync(ResendConfirmationRequestModel model)
    {
        await authService.ResendConfirmationCodeAsync(model);
        return Ok(ApiResult<string>.Success("Verification code sent successfully"));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequestModel model)
    {
        await authService.ForgotPasswordAsync(model);
        return Ok(ApiResult<string>.Success("Password reset token sent successfully"));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequestModel model)
    {
        await authService.ResetPasswordAsync(model);
        return Ok(ApiResult<string>.Success("Password has been reset successfully"));
    }
}
