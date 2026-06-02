using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.SemanticScholar;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SemanticScholarController : ControllerBase
{
    private readonly ISemanticScholarService _semanticScholarService;

    public SemanticScholarController(ISemanticScholarService semanticScholarService)
    {
        _semanticScholarService = semanticScholarService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchWorks([FromQuery] SemanticScholarSearchRequestModel request)
    {
        var result = await _semanticScholarService.SearchWorksAsync(request);
        return Ok(ApiResult<SemanticScholarSearchResponseModel>.Success(result));
    }
}
