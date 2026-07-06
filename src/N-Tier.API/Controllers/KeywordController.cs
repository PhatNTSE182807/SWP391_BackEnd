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

    public KeywordController(DatabaseContext context)
    {
        _context = context;
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
}
