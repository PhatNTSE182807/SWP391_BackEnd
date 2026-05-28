using System;

namespace N_Tier.Application.Models.Auth;

public class RegisterResponseModel
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string RoleName { get; set; }
}
