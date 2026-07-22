namespace N_Tier.Application.Models.User;

public class UpdateUserRoleModel
{
    /// <summary>
    /// The new role name to assign to the user (e.g., "Researcher", "Lecturer", "Student", "System Administrator")
    /// </summary>
    public string RoleName { get; set; }
}
