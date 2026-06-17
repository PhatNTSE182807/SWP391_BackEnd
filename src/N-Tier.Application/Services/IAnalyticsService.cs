using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.Analytics;

namespace N_Tier.Application.Services;

public interface IAnalyticsService
{
    // Research Trends
    Task<List<ChartDataPoint>> GetPaperCountByYearAsync();
    Task<List<ChartDataPoint>> GetCitationsByYearAsync();
    Task<List<ChartDataPoint>> GetTopTopicsAsync(int size);
    Task<List<ChartDataPoint>> GetTopDomainsAsync(int size);
    Task<List<SeriesDataDto>> GetKeywordTrendOverTimeAsync(List<string> keywords);

    // Author Statistics
    Task<List<ChartDataPoint>> GetTopAuthorsByCitationsAsync(int size);
    Task<List<ChartDataPoint>> GetTopAuthorsByHIndexAsync(int size);
    Task<NetworkGraphDto> GetAuthorCollaborationNetworkAsync(int size);

    // Journal Statistics
    Task<List<ChartDataPoint>> GetTopJournalsByPaperCountAsync(int size);
    Task<List<ChartDataPoint>> GetTopJournalsByCitationsAsync(int size);
    Task<List<ChartDataPoint>> GetOpenAccessRatioAsync();

    // Keyword Statistics
    Task<List<ChartDataPoint>> GetKeywordCloudAsync(int size);
    Task<List<SeriesDataDto>> GetTopKeywordsByYearAsync(int size);
    Task<NetworkGraphDto> GetKeywordCoOccurrenceNetworkAsync(int size);
}
