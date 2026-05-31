using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Paper;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaperController : ApiController
    {
        private readonly IPaperService _paperService;

        public PaperController(IPaperService paperService)
        {
            _paperService = paperService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _paperService.GetAllPapersAsync();
            return Ok(ApiResult<List<PaperResponseModel>>.Success(result));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _paperService.GetPaperByIdAsync(id);
            return Ok(ApiResult<PaperResponseModel>.Success(result));
        }

        [HttpGet("author/{authorId:guid}")]
        public async Task<IActionResult> GetByAuthorIdAsync(Guid authorId)
        {
            var result = await _paperService.GetPaperbyAuthorId(authorId);
            return Ok(ApiResult<List<PaperResponseModel>>.Success(result));
        }
    }
}
