namespace N_Tier.Application.Models.Auth;

public class ResetPasswordRequestModel
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
