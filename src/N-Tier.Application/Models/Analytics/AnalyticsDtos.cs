using System.Collections.Generic;

namespace N_Tier.Application.Models.Analytics;

// GET /api/analytics/keyword-trends
public class KeywordTrendDto
{
    public string Keyword { get; set; }
    public List<YearlyCountDto> YearlyCounts { get; set; } = new();
}

public class YearlyCountDto
{
    public int Year { get; set; }
    public int Count { get; set; }
}

// GET /api/analytics/trending-topics
public class TrendingTopicDto
{
    public string TopicName { get; set; }
    public int PaperCount { get; set; }
    public double GrowthPercentage { get; set; }
    public string Trend { get; set; } // "up", "down", "stable"
}

// GET /api/analytics/dashboard
public class ResearcherDashboardDto
{
    public int BookmarkedPapers { get; set; }
    public int FollowedTopics { get; set; }
    public int NewPapersInFollowedTopics { get; set; }
    public List<FollowedTopicSummaryDto> TopFollowedTopics { get; set; } = new();
}

public class FollowedTopicSummaryDto
{
    public string TopicName { get; set; }
    public int RecentPaperCount { get; set; }
}
