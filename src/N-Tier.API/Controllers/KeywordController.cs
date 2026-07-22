using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using N_Tier.Application.Models;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.API.Controllers;

[Tags("Keywords")]
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class KeywordController : ApiController
{
    private readonly DatabaseContext _context;
    private readonly N_Tier.Application.Services.IKeywordService _keywordService;

    public KeywordController(DatabaseContext context, N_Tier.Application.Services.IKeywordService keywordService)
    {
        _context = context;
        _keywordService = keywordService;
    }

    /// <summary>
    /// Returns keyword suggestions for autocomplete.
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestionsAsync([FromQuery] string q = "", [FromQuery] int size = 10)
    {
        if (size <= 0 || size > 50)
            return BadRequest(ApiResult<List<string>>.Failure(new[] { "size must be between 1 and 50." }));

        var query = q?.Trim().ToLower();

        var keywordsQuery = _context.Keywords
            .AsNoTracking()
            .Where(k => !string.IsNullOrWhiteSpace(k.KeywordName));

        if (!string.IsNullOrWhiteSpace(query))
        {
            keywordsQuery = keywordsQuery.Where(k =>
                k.NormalizedName.Contains(query) ||
                k.KeywordName.ToLower().Contains(query));
        }

        var keywords = await keywordsQuery
            .OrderByDescending(k => k.WorksCount ?? 0)
            .ThenBy(k => k.KeywordName)
            .Select(k => k.KeywordName)
            .Distinct()
            .Take(size)
            .ToListAsync();

        return Ok(ApiResult<List<string>>.Success(keywords));
    }

    /// <summary>
    /// Returns a paginated list of keywords.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetKeywordsAsync([FromQuery] N_Tier.Application.Models.Keyword.KeywordPagedRequest request)
    {
        var result = await _keywordService.GetPaginatedKeywordsAsync(request);
        return Ok(ApiResult<N_Tier.Application.Models.PagedResponse<N_Tier.Application.Models.Keyword.KeywordResponseModel>>.Success(result));
    }

    /// <summary>
    /// Returns a specific keyword's details.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetKeywordByIdAsync([FromRoute] System.Guid id)
    {
        var result = await _keywordService.GetKeywordByIdAsync(id);
        return Ok(ApiResult<N_Tier.Application.Models.Keyword.KeywordResponseModel>.Success(result));
    }
}
