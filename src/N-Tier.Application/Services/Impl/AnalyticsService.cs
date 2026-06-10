using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Application.Models.Analytics;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.Application.Services.Impl;

public class AnalyticsService : IAnalyticsService
{
    private readonly DatabaseContext _context;

    public AnalyticsService(DatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<KeywordTrendDto> GetKeywordTrendsAsync(string keyword, int years = 5)
    {
        // Find keyword by name (case-insensitive via normalized_name column)
        var normalizedKeyword = keyword.Trim().ToLower();

        var keywordEntity = await _context.Keywords
            .FirstOrDefaultAsync(k => k.NormalizedName == normalizedKeyword);

        if (keywordEntity == null)
        {
            // Return empty result if keyword not found
            return new KeywordTrendDto
            {
                Keyword = keyword,
                YearlyCounts = new List<YearlyCountDto>()
            };
        }

        // Find the reference year from the latest paper in DB
        var maxYear = await _context.Papers
            .Where(p => p.PublicationYear != null)
            .MaxAsync(p => (int?)p.PublicationYear) ?? DateTime.UtcNow.Year;

        var startYear = maxYear - years + 1;

        // Get paper counts per year for this keyword
        var papersWithKeyword = await _context.PaperKeywords
            .Include(pk => pk.Paper)
            .Where(pk => pk.KeywordId == keywordEntity.KeywordId
                      && pk.Paper.PublicationYear != null
                      && pk.Paper.PublicationYear >= startYear
                      && pk.Paper.PublicationYear <= maxYear)
            .Select(pk => pk.Paper.PublicationYear!.Value)
            .ToListAsync();

        var yearlyCounts = papersWithKeyword
            .GroupBy(y => y)
            .Select(g => new YearlyCountDto { Year = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Year, x => x.Count);

        // Fill missing years with 0
        var result = new List<YearlyCountDto>();
        for (int y = startYear; y <= maxYear; y++)
        {
            result.Add(new YearlyCountDto
            {
                Year = y,
                Count = yearlyCounts.TryGetValue(y, out var count) ? count : 0
            });
        }

        return new KeywordTrendDto
        {
            Keyword = keywordEntity.KeywordName,
            YearlyCounts = result
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TrendingTopicDto>> GetTrendingTopicsAsync(int topCount = 10)
    {
        // Use the latest available year as reference
        var maxYear = await _context.Papers
            .Where(p => p.PublicationYear != null)
            .MaxAsync(p => (int?)p.PublicationYear) ?? DateTime.UtcNow.Year;

        var previousYear = maxYear - 1;

        // Get all topic IDs that appear overall (not limited to current period)
        var allTopicIds = await _context.PaperTopics
            .GroupBy(pt => pt.TopicId)
            .OrderByDescending(g => g.Count())
            .Take(topCount * 3) // Wider pool to ensure enough after filtering
            .Select(g => g.Key)
            .ToListAsync();

        var trendingTopics = new List<TrendingTopicDto>();

        foreach (var topicId in allTopicIds)
        {
            var topic = await _context.ResearchTopics.FindAsync(topicId);
            if (topic == null) continue;

            var currentYearCount = await _context.PaperTopics
                .Include(pt => pt.Paper)
                .CountAsync(pt => pt.TopicId == topicId && pt.Paper.PublicationYear == maxYear);

            var previousYearCount = await _context.PaperTopics
                .Include(pt => pt.Paper)
                .CountAsync(pt => pt.TopicId == topicId && pt.Paper.PublicationYear == previousYear);

            double growth = 0;
            string trend;

            if (previousYearCount > 0)
            {
                growth = Math.Round((double)(currentYearCount - previousYearCount) / previousYearCount * 100, 1);
            }
            else if (currentYearCount > 0)
            {
                growth = 100.0;
            }

            if (growth > 5) trend = "up";
            else if (growth < -5) trend = "down";
            else trend = "stable";

            trendingTopics.Add(new TrendingTopicDto
            {
                TopicName = topic.TopicName,
                PaperCount = currentYearCount,
                GrowthPercentage = growth,
                Trend = trend
            });
        }

        return trendingTopics
            .OrderByDescending(t => t.GrowthPercentage)
            .Take(topCount);
    }

    /// <inheritdoc />
    public async Task<ResearcherDashboardDto> GetResearcherDashboardAsync(Guid userId)
    {
        var bookmarkedPapers = await _context.UserBookmarks
            .CountAsync(b => b.UserId == userId);

        var followedTopicIds = await _context.UserFollowingTopics
            .Where(f => f.UserId == userId)
            .Select(f => f.TopicId)
            .ToListAsync();

        var followedTopicsCount = followedTopicIds.Count;

        // Use the latest year in database as reference for "new" papers
        var maxYear = await _context.Papers
            .Where(p => p.PublicationYear != null)
            .MaxAsync(p => (int?)p.PublicationYear) ?? DateTime.UtcNow.Year;

        // Count new papers this year in followed topics
        var newPapersInFollowedTopics = followedTopicIds.Count > 0
            ? await _context.PaperTopics
                .Include(pt => pt.Paper)
                .CountAsync(pt => followedTopicIds.Contains(pt.TopicId)
                               && pt.Paper.PublicationYear == maxYear)
            : 0;

        // Top 5 followed topics with their recent paper counts
        var topFollowedTopics = new List<FollowedTopicSummaryDto>();
        foreach (var topicId in followedTopicIds.Take(5))
        {
            var topic = await _context.ResearchTopics.FindAsync(topicId);
            if (topic == null) continue;

            var recentCount = await _context.PaperTopics
                .Include(pt => pt.Paper)
                .CountAsync(pt => pt.TopicId == topicId && pt.Paper.PublicationYear == maxYear);

            topFollowedTopics.Add(new FollowedTopicSummaryDto
            {
                TopicName = topic.TopicName,
                RecentPaperCount = recentCount
            });
        }

        return new ResearcherDashboardDto
        {
            BookmarkedPapers = bookmarkedPapers,
            FollowedTopics = followedTopicsCount,
            NewPapersInFollowedTopics = newPapersInFollowedTopics,
            TopFollowedTopics = topFollowedTopics
        };
    }
}
