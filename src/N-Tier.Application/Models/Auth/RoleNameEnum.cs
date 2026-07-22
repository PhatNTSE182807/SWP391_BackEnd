using System.Text.Json.Serialization;

namespace N_Tier.Application.Models.Auth;

/// <summary>
/// List of roles available for selection when registering an account
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoleNameEnum
{
    Researcher,

    Lecturer,

    Student
}
