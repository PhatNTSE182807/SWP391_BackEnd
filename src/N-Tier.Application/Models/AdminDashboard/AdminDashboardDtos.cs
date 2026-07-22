namespace N_Tier.Application.Models.AdminDashboard;

public class AdminDashboardOverviewDto
{
    public AdminDashboardSummaryDto Summary { get; set; } = new();
    public List<RoleCountDto> UserRoleBreakdown { get; set; } = new();
    public List<UserGrowthDataPointDto> UserGrowth { get; set; } = new();
    public List<AdminRecentUserDto> RecentUsers { get; set; } = new();
    public List<ApiStatusDto> ApiStatuses { get; set; } = new();
    public ApiCallsOverviewDto ApiCalls { get; set; } = new();
}

public class AdminDashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int DeactivatedUsers { get; set; }
    public int Researchers { get; set; }
    public int Students { get; set; }
    public int Lecturers { get; set; }
    public int SystemAdministrators { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewResearchersThisMonth { get; set; }
    public int ApisConnected { get; set; }
    public int DegradedApis { get; set; }
}

public class RoleCountDto
{
    public string RoleName { get; set; }
    public int Count { get; set; }
}

public class UserGrowthDataPointDto
{
    public string Month { get; set; }
    public int TotalUsers { get; set; }
    public int Researchers { get; set; }
    public int Students { get; set; }
    public int Lecturers { get; set; }
    public int SystemAdministrators { get; set; }
}

public class AdminRecentUserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string RoleName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? RegisteredAt { get; set; }
}

public class ApiStatusDto
{
    public string Name { get; set; }
    public string BaseUrl { get; set; }
    public string Status { get; set; }
    public int? LatencyMs { get; set; }
    public DateTime CheckedAt { get; set; }
    public string Message { get; set; }
}

public class ApiCallsOverviewDto
{
    public bool TrackingEnabled { get; set; }
    public string Message { get; set; }
    public List<ApiCallDataPointDto> DataPoints { get; set; } = new();
}

public class ApiCallDataPointDto
{
    public string Key { get; set; }
    public int Value { get; set; }
}
