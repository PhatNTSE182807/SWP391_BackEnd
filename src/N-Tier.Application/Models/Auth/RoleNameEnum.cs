namespace N_Tier.Application.Models.Auth;

/// <summary>
/// Danh sách roles có thể chọn khi đăng ký tài khoản
/// </summary>
public enum RoleNameEnum
{
    [System.Runtime.Serialization.EnumMember(Value = "System Administrator")]
    SystemAdministrator,

    Researcher,

    Lecturer,

    Student
}
