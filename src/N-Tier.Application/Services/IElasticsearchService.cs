using N_Tier.Application.Models.Search;

namespace N_Tier.Application.Services;

public interface IElasticsearchService
{
    Task<bool> IndexPaperAsync(PaperDocument paper);
    Task<bool> IndexPapersAsync(IEnumerable<PaperDocument> papers);
    Task<bool> UpdatePaperAsync(PaperDocument paper);
    Task<bool> DeletePaperAsync(Guid paperId);
    Task<SearchPaperResponseModel> SearchPapersAsync(SearchPaperRequestModel request);
    Task<bool> CreateIndexAsync();
    Task<bool> DeleteIndexAsync();
    Task<bool> ReindexAllPapersAsync();
}
