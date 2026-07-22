using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Application.Exceptions;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Keyword;
using N_Tier.DataAccess.Repositories;
using N_Tier.Shared.Helpers;

namespace N_Tier.Application.Services.Impl;

public class KeywordService : IKeywordService
{
    private readonly IKeywordRepository _keywordRepository;

    public KeywordService(IKeywordRepository keywordRepository)
    {
        _keywordRepository = keywordRepository;
    }

    public async Task<PagedResponse<KeywordResponseModel>> GetPaginatedKeywordsAsync(KeywordPagedRequest request)
    {
        var (results, total) = await _keywordRepository.GetPaginatedKeywordsAsync(request.SearchTerm, request.Page, request.Size);

        var mappedResults = results.Select(k => new KeywordResponseModel
        {
            KeywordId = k.KeywordId,
            KeywordName = k.KeywordName,
            NormalizedName = k.NormalizedName,
            WorksCount = k.WorksCount,
            CitedByCount = k.CitedByCount,
            WorksApiUrl = k.WorksApiUrl,
            SourceCreatedDate = k.SourceCreatedDate,
            SourceUpdatedDate = k.SourceUpdatedDate,
            CreatedAt = k.CreatedAt,
            UpdatedAt = k.UpdatedAt
        }).ToList();

        return new PagedResponse<KeywordResponseModel>(mappedResults, total, request.Page, request.Size);
    }

    public async Task<KeywordResponseModel> GetKeywordByIdAsync(Guid keywordId)
    {
        var keyword = await _keywordRepository.GetFirstAsync(k => k.KeywordId == keywordId);
        if (keyword == null)
            throw new NotFoundException($"Keyword with id {keywordId} not found");

        return new KeywordResponseModel
        {
            KeywordId = keyword.KeywordId,
            KeywordName = keyword.KeywordName,
            NormalizedName = keyword.NormalizedName,
            WorksCount = keyword.WorksCount,
            CitedByCount = keyword.CitedByCount,
            WorksApiUrl = keyword.WorksApiUrl,
            SourceCreatedDate = keyword.SourceCreatedDate,
            SourceUpdatedDate = keyword.SourceUpdatedDate,
            CreatedAt = keyword.CreatedAt,
            UpdatedAt = keyword.UpdatedAt
        };
    }
}
