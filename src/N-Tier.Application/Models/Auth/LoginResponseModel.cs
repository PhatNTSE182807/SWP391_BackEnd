using System;

namespace N_Tier.Application.Models.Auth;

public class LoginResponseModel
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Phonenumber { get; set; }
    public string RoleName { get; set; }
    public string Token { get; set; }
}
