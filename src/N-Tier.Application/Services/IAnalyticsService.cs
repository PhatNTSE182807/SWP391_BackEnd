using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using N_Tier.Application.Models.Analytics;

namespace N_Tier.Application.Services;

public interface IAnalyticsService
{
    /// <summary>
    /// Returns yearly paper counts for a given keyword over a specified number of years.
    /// </summary>
    Task<KeywordTrendDto> GetKeywordTrendsAsync(string keyword, int years = 5);

    /// <summary>
    /// Returns yearly paper counts for a given topic name over a specified number of years.
    /// </summary>
    Task<TopicTrendDto> GetTopicTrendsAsync(string topic, int years = 5);

    /// <summary>
    /// Returns Top 10 topics with highest publication growth in the latest available period.
    /// </summary>
    Task<IEnumerable<TrendingTopicDto>> GetTrendingTopicsAsync(int topCount = 10);

    /// <summary>
    /// Returns personalized researcher dashboard stats.
    /// </summary>
    Task<ResearcherDashboardDto> GetResearcherDashboardAsync(Guid userId);
}
