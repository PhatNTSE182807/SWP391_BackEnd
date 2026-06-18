using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Author;
using N_Tier.Application.Services;

namespace N_Tier.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorController : ApiController
{
    private readonly IAuthorService _authorService;

    public AuthorController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    /// <summary>
    /// Get all authors from database
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _authorService.GetAllAuthorsAsync();
        return Ok(ApiResult<List<AuthorResponseModel>>.Success(result));
    }

    /// <summary>
    /// Get author by ID from database
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var result = await _authorService.GetAuthorByIdAsync(id);
        return Ok(ApiResult<AuthorResponseModel>.Success(result));
    }
}
