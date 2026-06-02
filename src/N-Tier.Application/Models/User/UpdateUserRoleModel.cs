namespace N_Tier.Application.Models.User;

public class UpdateUserRoleModel
{
    /// <summary>
    /// Tên role mới cần gán cho user (ví dụ: "Researcher", "Lecturer", "Student", "System Administrator")
    /// </summary>
    public string RoleName { get; set; }
}
