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
    public int PreviousPaperCount { get; set; }
    public double GrowthPercentage { get; set; }
    public string Trend { get; set; } // "up", "down", "stable"
    public int CurrentYear { get; set; }
    public int PreviousYear { get; set; }
    public int Years { get; set; }
}

// GET /api/analytics/topic-trends
public class TopicTrendDto
{
    public string TopicName { get; set; }
    public List<YearlyCountDto> YearlyCounts { get; set; } = new();
}

// GET /api/analytics/topics/compare
public class TopicComparisonDto
{
    public Guid TopicId { get; set; }
    public string TopicName { get; set; }
    public int PaperCount { get; set; }
    public int CitationCount { get; set; }
    public int JournalCount { get; set; }
    public int TopicHIndex { get; set; }
    public double GrowthPercentage { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public List<YearlyCountDto> YearlyCounts { get; set; } = new();
}

// GET /api/analytics/topics/available-for-compare
public class AvailableTopicForCompareDto
{
    public Guid TopicId { get; set; }
    public string TopicName { get; set; }
    public int PaperCount { get; set; }
    public int FirstYear { get; set; }
    public int LastYear { get; set; }
}

// GET /api/analytics/journals/tracker
public class JournalTrackerDto
{
    public Guid JournalId { get; set; }
    public string JournalName { get; set; }
    public string Publisher { get; set; }
    public string HomepageUrl { get; set; }
    public bool IsOpenAccess { get; set; }
    public int PaperCount { get; set; }
    public int CitationCount { get; set; }
    public double GrowthPercentage { get; set; }
    public int? LastPublicationYear { get; set; }
    public List<string> TopKeywords { get; set; } = new();
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
