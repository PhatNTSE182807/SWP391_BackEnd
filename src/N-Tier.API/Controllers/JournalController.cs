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
        /// Get all journals
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _journalService.GetAllAsync();
            return Ok(ApiResult<List<JournalResponseModel>>.Success(result));
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
