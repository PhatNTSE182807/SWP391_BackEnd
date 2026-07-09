namespace N_Tier.Application.Models.Auth;

public class VerifyRegistrationRequestModel
{
    public string Email { get; set; }
    public string Code { get; set; }
}
