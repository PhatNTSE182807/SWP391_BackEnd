using N_Tier.Application.Models.AdminDashboard;

namespace N_Tier.Application.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardOverviewDto> GetOverviewAsync();
    Task<List<ApiStatusDto>> GetApiStatusesAsync();
    Task<ApiCallsOverviewDto> GetApiCallsAsync();
}
