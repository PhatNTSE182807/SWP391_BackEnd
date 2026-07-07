using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Author;
using N_Tier.DataAccess.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace N_Tier.Application.Services.Impl;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IDistributedCache _cache;

    public AuthorService(IAuthorRepository authorRepository, IDistributedCache cache)
    {
        _authorRepository = authorRepository;
        _cache = cache;
    }

    public async Task<List<AuthorResponseModel>> GetAllAuthorsAsync()
    {
        var authors = await _authorRepository.GetAllWithInclusionsAsync();
        return authors.Adapt<List<AuthorResponseModel>>();
    }

    public async Task<AuthorResponseModel> GetAuthorByIdAsync(Guid id)
    {
        var author = await _authorRepository.GetByIdAsync(id);
        return author.Adapt<AuthorResponseModel>();
    }

    public async Task<PagedResponse<AuthorResponseModel>> GetPaginatedAuthorsAsync(PagedRequest request)
    {
        var version = await GetCacheVersionAsync("authors");
        var cacheKey = $"authors:v:{version}:page:{request.Page}:size:{request.Size}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<PagedResponse<AuthorResponseModel>>(cachedData);
        }

        var (results, total) = await _authorRepository.GetPaginatedAsync(request.Page, request.Size);
        var mappedResults = results.Adapt<List<AuthorResponseModel>>();
        var response = new PagedResponse<AuthorResponseModel>(mappedResults, total, request.Page, request.Size);

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions);

        return response;
    }

    private async Task<string> GetCacheVersionAsync(string prefix)
    {
        var version = await _cache.GetStringAsync($"{prefix}:version");
        if (string.IsNullOrEmpty(version))
        {
            version = "1";
            await _cache.SetStringAsync($"{prefix}:version", version);
        }
        return version;
    }
}
