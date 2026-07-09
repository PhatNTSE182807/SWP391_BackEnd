using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Journal;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JournalController : ApiController
    {
        private readonly IJournalService _journalService;

        public JournalController(IJournalService journalService)
        {
            _journalService = journalService;
        }

        /// <summary>
        /// Get all journals (paginated)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] JournalPagedRequest request)
        {
            var result = await _journalService.GetPaginatedJournalsAsync(request);
            return Ok(ApiResult<PagedResponse<JournalResponseModel>>.Success(result));
        }

        /// <summary>
        /// Get journal by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _journalService.GetByIdAsync(id);
            return Ok(ApiResult<JournalResponseModel>.Success(result));
        }
    }
}
