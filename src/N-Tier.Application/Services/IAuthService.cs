using System.Threading.Tasks;
using N_Tier.Application.Models.Auth;

namespace N_Tier.Application.Services;

public interface IAuthService
{
    Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequestModel);
    Task<RegisterResponseModel> RegisterAsync(RegisterRequestModel registerRequestModel);
}
