using System;
using System.Threading.Tasks;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Keyword;

namespace N_Tier.Application.Services;

public interface IKeywordService
{
    Task<PagedResponse<KeywordResponseModel>> GetPaginatedKeywordsAsync(KeywordPagedRequest request);
    Task<KeywordResponseModel> GetKeywordByIdAsync(Guid keywordId);
}
