namespace N_Tier.Application.Models.User;

public class UpdateUserProfileModel
{
    public string Username { get; set; }
    
    public string Email { get; set; }
    
    public string Phonenumber { get; set; }
    
    public string OldPassword { get; set; }
    
    public string NewPassword { get; set; }
}
