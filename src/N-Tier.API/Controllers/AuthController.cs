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
}
