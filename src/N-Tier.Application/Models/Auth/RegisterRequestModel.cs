namespace N_Tier.Application.Models.Auth;

public class RegisterRequestModel
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public RoleNameEnum RoleName { get; set; }
}
