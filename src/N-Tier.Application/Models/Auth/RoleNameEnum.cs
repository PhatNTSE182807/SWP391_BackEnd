using System.Text.Json.Serialization;

namespace N_Tier.Application.Models.Auth;

/// <summary>
/// Danh sách roles có thể chọn khi đăng ký tài khoản
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoleNameEnum
{
    Researcher,

    Lecturer,

    Student
}
