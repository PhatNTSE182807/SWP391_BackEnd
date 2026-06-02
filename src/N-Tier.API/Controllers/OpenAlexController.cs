using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.OpenAlex;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenAlexController : ControllerBase
{
    private readonly IOpenAlexService _openAlexService;

    public OpenAlexController(IOpenAlexService openAlexService)
    {
        _openAlexService = openAlexService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchWorks([FromQuery] SearchWorksRequestModel request)
    {
        var result = await _openAlexService.SearchWorksAsync(request);
        return Ok(ApiResult<SearchWorksResponseModel>.Success(result));
    }
}
