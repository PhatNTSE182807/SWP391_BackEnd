using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Crossref;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrossrefController : ControllerBase
{
    private readonly ICrossrefService _crossrefService;

    public CrossrefController(ICrossrefService crossrefService)
    {
        _crossrefService = crossrefService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchWorks([FromQuery] CrossrefSearchRequestModel request)
    {
        var result = await _crossrefService.SearchWorksAsync(request);
        return Ok(ApiResult<CrossrefSearchResponseModel>.Success(result));
    }
}
