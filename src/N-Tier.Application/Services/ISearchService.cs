using N_Tier.Application.Models.Search;

namespace N_Tier.Application.Services;

public interface ISearchService
{
    Task<SearchPaperResponseModel> SearchPapersAsync(SearchPaperRequestModel request);
}
