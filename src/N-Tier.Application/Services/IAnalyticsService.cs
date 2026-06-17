using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.Analytics;

namespace N_Tier.Application.Services;

public interface IAnalyticsService
{
    // Research Trends (Elasticsearch)
    Task<List<ChartDataPoint>> GetPaperCountByYearAsync();
    Task<List<ChartDataPoint>> GetCitationsByYearAsync();
    Task<List<ChartDataPoint>> GetTopTopicsAsync(int size);
    Task<List<ChartDataPoint>> GetTopDomainsAsync(int size);
    Task<List<SeriesDataDto>> GetKeywordTrendOverTimeAsync(List<string> keywords);

    // Author Statistics (Elasticsearch)
    Task<List<ChartDataPoint>> GetTopAuthorsByCitationsAsync(int size);
    Task<List<ChartDataPoint>> GetTopAuthorsByHIndexAsync(int size);
    Task<NetworkGraphDto> GetAuthorCollaborationNetworkAsync(int size);

    // Journal Statistics (Elasticsearch)
    Task<List<ChartDataPoint>> GetTopJournalsByPaperCountAsync(int size);
    Task<List<ChartDataPoint>> GetTopJournalsByCitationsAsync(int size);
    Task<List<ChartDataPoint>> GetOpenAccessRatioAsync();

    // Keyword Statistics (Elasticsearch)
    Task<List<ChartDataPoint>> GetKeywordCloudAsync(int size);
    Task<List<SeriesDataDto>> GetTopKeywordsByYearAsync(int size);
    Task<NetworkGraphDto> GetKeywordCoOccurrenceNetworkAsync(int size);

    // Keyword & Topic Trends (EF Core)
    Task<KeywordTrendDto> GetKeywordTrendsAsync(string keyword, int years = 5);
    Task<TopicTrendDto> GetTopicTrendsAsync(string topic, int years = 5);
    Task<IEnumerable<TrendingTopicDto>> GetTrendingTopicsAsync(int topCount = 10);

    // Researcher Dashboard (EF Core)
    Task<ResearcherDashboardDto> GetResearcherDashboardAsync(Guid userId);
}
