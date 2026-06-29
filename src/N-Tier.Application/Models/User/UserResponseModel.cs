using System;

namespace N_Tier.Application.Models.User;

public class UserResponseModel
{
    public Guid UserId { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public string Phonenumber { get; set; }

    public string RoleName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
