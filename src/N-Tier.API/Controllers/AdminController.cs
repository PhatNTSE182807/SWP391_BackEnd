using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Jobs;
using N_Tier.Application.Models;
using N_Tier.Application.Models.User;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // In production: [Authorize(Roles = "Admin")]
public class AdminController : ApiController
{
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ICacheService _cacheService;
    private readonly IUserService _userService;

    public AdminController(
        IBackgroundJobClient backgroundJobs,
        IElasticsearchService elasticsearchService,
        ICacheService cacheService,
        IUserService userService)
    {
        _backgroundJobs = backgroundJobs;
        _elasticsearchService = elasticsearchService;
        _cacheService = cacheService;
        _userService = userService;
    }

    // ========================================
    // ELASTICSEARCH & SEARCH MANAGEMENT
    // ========================================

    /// <summary>
    /// Trigger manual reindexing of all papers (Background job)
    /// </summary>
    [HttpPost("reindex")]
    public IActionResult TriggerReindex()
    {
        var jobId = _backgroundJobs.Enqueue<IReindexJob>(job => job.ExecuteAsync());
        return Ok(ApiResult<object>.Success(new { jobId, message = "Reindex job queued successfully" }));
    }

    /// <summary>
    /// Create Elasticsearch index
    /// </summary>
    [HttpPost("elasticsearch/create-index")]
    public async Task<IActionResult> CreateIndex()
    {
        var result = await _elasticsearchService.CreateIndexAsync();
        return Ok(ApiResult<bool>.Success(result));
    }

    /// <summary>
    /// Delete Elasticsearch index
    /// </summary>
    [HttpDelete("elasticsearch/delete-index")]
    public async Task<IActionResult> DeleteIndex()
    {
        var result = await _elasticsearchService.DeleteIndexAsync();
        return Ok(ApiResult<bool>.Success(result));
    }

    /// <summary>
    /// Clear all search cache
    /// </summary>
    [HttpPost("cache/clear")]
    public async Task<IActionResult> ClearCache()
    {
        var result = await _cacheService.RemoveByPatternAsync("search:papers:*");
        return Ok(ApiResult<object>.Success(new { cleared = result, message = "Cache cleared successfully" }));
    }

    // ========================================
    // USER MANAGEMENT (System Administrator only)
    // ========================================

    /// <summary>
    /// Get all users (username, email, phone, role, isActive)
    /// System Administrator only
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "System Administrator")]
    public async Task<IActionResult> GetAllUsersAsync()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResult<object>.Success(users));
    }

    /// <summary>
    /// Toggle user active/deactivated status
    /// Admin cannot deactivate themselves
    /// System Administrator only
    /// </summary>
    [HttpPut("users/{userId:guid}/deactivate")]
    [Authorize(Roles = "System Administrator")]
    public async Task<IActionResult> ToggleDeactivateUserAsync(Guid userId)
    {
        var result = await _userService.ToggleDeactivateUserAsync(userId);
        return Ok(ApiResult<UserResponseModel>.Success(result));
    }

    /// <summary>
    /// Delete user by userId
    /// Admin cannot delete themselves
    /// System Administrator only
    /// </summary>
    [HttpDelete("users/{userId:guid}")]
    [Authorize(Roles = "System Administrator")]
    public async Task<IActionResult> DeleteUserAsync(Guid userId)
    {
        await _userService.DeleteUserAsync(userId);
        return Ok(ApiResult<object>.Success(new { message = "User deleted successfully" }));
    }
}
