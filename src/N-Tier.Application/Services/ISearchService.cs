using System.Threading.Tasks;
using N_Tier.Application.Models.Search;

namespace N_Tier.Application.Services;

public interface ISearchService
{
    Task<SearchPaperResponse> SearchPapersAsync(SearchPaperRequest request);
    Task IndexPaperAsync(Core.Entities.Paper paper);
    Task BulkIndexPapersAsync();
    Task DeleteIndexAsync();
    Task RecreateIndexAsync();
}
