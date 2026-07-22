using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.AdminDashboard;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Tags("AdminDashboard")]
[Route("api/admin/dashboard")]
[Authorize(Roles = "System Administrator")]
public class AdminDashboardController : ApiController
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOverviewAsync()
    {
        var result = await _adminDashboardService.GetOverviewAsync();
        return Ok(ApiResult<AdminDashboardOverviewDto>.Success(result));
    }

    [HttpGet("api-status")]
    public async Task<IActionResult> GetApiStatusesAsync()
    {
        var result = await _adminDashboardService.GetApiStatusesAsync();
        return Ok(ApiResult<List<ApiStatusDto>>.Success(result));
    }

    [HttpGet("api-calls")]
    public async Task<IActionResult> GetApiCallsAsync()
    {
        var result = await _adminDashboardService.GetApiCallsAsync();
        return Ok(ApiResult<ApiCallsOverviewDto>.Success(result));
    }
}
