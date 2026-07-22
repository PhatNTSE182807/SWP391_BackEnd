using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Topic;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TopicController : ApiController
{
    private readonly ITopicService _topicService;

    public TopicController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    /// <summary>
    /// Get all topics from database (paginated)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] TopicPagedRequest request)
    {
        var result = await _topicService.GetPaginatedTopicsAsync(request);
        return Ok(ApiResult<PagedResponse<TopicResponseModel>>.Success(result));
    }

    /// <summary>
    /// Get topic by ID from database
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await _topicService.GetTopicByIdAsync(id);
        return Ok(ApiResult<TopicResponseModel>.Success(result));
    }
}
