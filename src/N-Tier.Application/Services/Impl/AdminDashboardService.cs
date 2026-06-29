using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N_Tier.Application.Models.AdminDashboard;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.Application.Services.Impl;

public class AdminDashboardService : IAdminDashboardService
{
    private static readonly HttpClient HealthCheckClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    private readonly DatabaseContext _context;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(DatabaseContext context, ILogger<AdminDashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AdminDashboardOverviewDto> GetOverviewAsync()
    {
        var users = await _context.CoreUsers
            .Include(u => u.Role)
            .Where(u => !u.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var apiStatuses = await GetApiStatusesAsync();
        var now = DateTime.UtcNow.AddHours(7);
        var startOfWeek = now.AddDays(-7);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        return new AdminDashboardOverviewDto
        {
            Summary = new AdminDashboardSummaryDto
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                DeactivatedUsers = users.Count(u => !u.IsActive),
                Researchers = CountRole(users, "Researcher"),
                Students = CountRole(users, "Student"),
                Lecturers = CountRole(users, "Lecturer"),
                SystemAdministrators = CountRole(users, "System Administrator"),
                NewUsersThisWeek = users.Count(u => u.CreatedAt >= startOfWeek),
                NewResearchersThisMonth = users.Count(u =>
                    u.CreatedAt >= startOfMonth &&
                    string.Equals(u.Role?.RoleName, "Researcher", StringComparison.OrdinalIgnoreCase)),
                ApisConnected = await CountConfiguredApisAsync(apiStatuses.Count),
                DegradedApis = apiStatuses.Count(a => a.Status != "Operational")
            },
            UserRoleBreakdown = users
                .GroupBy(u => u.Role?.RoleName ?? "Unknown")
                .OrderBy(g => g.Key)
                .Select(g => new RoleCountDto
                {
                    RoleName = g.Key,
                    Count = g.Count()
                })
                .ToList(),
            UserGrowth = BuildUserGrowth(users, now, 6),
            RecentUsers = users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new AdminRecentUserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    RoleName = u.Role?.RoleName,
                    IsActive = u.IsActive,
                    RegisteredAt = u.CreatedAt
                })
                .ToList(),
            ApiStatuses = apiStatuses,
            ApiCalls = await GetApiCallsAsync()
        };
    }

    public async Task<List<ApiStatusDto>> GetApiStatusesAsync()
    {
        var checks = new[]
        {
            new ApiHealthCheck("OpenAlex API", "https://api.openalex.org", "https://api.openalex.org/works?per-page=1&mailto=phuocse@fpt.edu.vn"),
            new ApiHealthCheck("Crossref API", "https://api.crossref.org", "https://api.crossref.org/works?rows=1"),
            new ApiHealthCheck("Semantic Scholar API", "https://api.semanticscholar.org", "https://api.semanticscholar.org/graph/v1/paper/search?query=computer%20science&limit=1&fields=title")
        };

        var results = await Task.WhenAll(checks.Select(CheckApiAsync));
        return results.ToList();
    }

    public Task<ApiCallsOverviewDto> GetApiCallsAsync()
    {
        return Task.FromResult(new ApiCallsOverviewDto
        {
            TrackingEnabled = false,
            Message = "API call tracking is not enabled yet. Add request/call logging to populate this chart.",
            DataPoints = new List<ApiCallDataPointDto>()
        });
    }

    private static int CountRole(List<User> users, string roleName)
    {
        return users.Count(u => string.Equals(u.Role?.RoleName, roleName, StringComparison.OrdinalIgnoreCase));
    }

    private static List<UserGrowthDataPointDto> BuildUserGrowth(List<User> users, DateTime referenceDate, int months)
    {
        var dataPoints = new List<UserGrowthDataPointDto>();
        var firstMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1).AddMonths(-(months - 1));

        for (var i = 0; i < months; i++)
        {
            var monthStart = firstMonth.AddMonths(i);
            var nextMonth = monthStart.AddMonths(1);
            var usersInMonth = users
                .Where(u => u.CreatedAt >= monthStart && u.CreatedAt < nextMonth)
                .ToList();

            dataPoints.Add(new UserGrowthDataPointDto
            {
                Month = monthStart.ToString("MMM"),
                TotalUsers = usersInMonth.Count,
                Researchers = CountRole(usersInMonth, "Researcher"),
                Students = CountRole(usersInMonth, "Student"),
                Lecturers = CountRole(usersInMonth, "Lecturer"),
                SystemAdministrators = CountRole(usersInMonth, "System Administrator")
            });
        }

        return dataPoints;
    }

    private async Task<int> CountConfiguredApisAsync(int fallbackCount)
    {
        try
        {
            return await _context.Set<ApiSource>()
                .AsNoTracking()
                .CountAsync(a => a.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Could not read ApiSource records for admin dashboard. Falling back to live health-check count.");
            return fallbackCount;
        }
    }

    private async Task<ApiStatusDto> CheckApiAsync(ApiHealthCheck check)
    {
        var checkedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, check.HealthUrl);
            request.Headers.UserAgent.ParseAdd("N-Tier-Admin-Dashboard/1.0");

            using var response = await HealthCheckClient.SendAsync(request);
            stopwatch.Stop();

            var latencyMs = (int)stopwatch.ElapsedMilliseconds;
            var status = response.IsSuccessStatusCode
                ? latencyMs <= 1000 ? "Operational" : "Degraded"
                : "Down";

            return new ApiStatusDto
            {
                Name = check.Name,
                BaseUrl = check.BaseUrl,
                Status = status,
                LatencyMs = latencyMs,
                CheckedAt = checkedAt,
                Message = response.IsSuccessStatusCode
                    ? "Health check succeeded."
                    : $"Health check returned {(int)response.StatusCode}."
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ApiStatusDto
            {
                Name = check.Name,
                BaseUrl = check.BaseUrl,
                Status = "Down",
                LatencyMs = null,
                CheckedAt = checkedAt,
                Message = ex.Message
            };
        }
    }

    private sealed record ApiHealthCheck(string Name, string BaseUrl, string HealthUrl);
}
