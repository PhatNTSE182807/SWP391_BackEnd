using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Application.Models.Dashboard;
using N_Tier.Application.Models.Analytics;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.Application.Services.Impl;

public class DashboardService : IDashboardService
{
    private readonly DatabaseContext _context;

    public DashboardService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid userId)
    {
        var bookmarkedPapers = await _context.UserBookmarks
            .CountAsync(b => b.UserId == userId);

        var followedTopics = await _context.UserFollowingTopics
            .CountAsync(f => f.UserId == userId);

        // Calculate new papers since last week as a simple metric
        var lastWeek = DateTime.UtcNow.AddDays(-7);
        var newPapers = await _context.Papers
            .CountAsync(p => p.CreatedAt >= lastWeek);

        return new DashboardSummaryDto
        {
            BookmarkedPapers = bookmarkedPapers,
            FollowedTopics = followedTopics,
            JournalAlerts = 0, // Placeholder as per UI
            NewPapers = newPapers
        };
    }

    public async Task<IEnumerable<PublicationTrendDto>> GetPublicationTrendsAsync(int lastXMonths = 6)
    {
        var maxDate = await _context.Papers.MaxAsync(p => (DateOnly?)p.PublicationDate);
        var referenceDate = maxDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var startDate = referenceDate.AddMonths(-lastXMonths + 1);

        // Get top 3 topics overall to show in trends
        var topTopics = await _context.PaperTopics
            .GroupBy(pt => pt.TopicId)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToListAsync();

        var trends = new List<PublicationTrendDto>();

        foreach (var topicId in topTopics)
        {
            var topic = await _context.ResearchTopics.FindAsync(topicId);
            if (topic == null) continue;

            // Grouping by PublicationYear and month (extracted from PublicationDate if available)
            // Note: Since PublicationDate might be null or format varies, we will rely on PublicationDate
            // EF Core translated grouping by Month/Year can be tricky, so we do it in memory for the filtered set
            var papersInTopic = await _context.PaperTopics
                .Include(pt => pt.Paper)
                .Where(pt => pt.TopicId == topicId && pt.Paper.PublicationDate != null && pt.Paper.PublicationDate >= startDate)
                .Select(pt => pt.Paper.PublicationDate)
                .ToListAsync();

            var monthlyCounts = papersInTopic
                .Where(d => d.HasValue)
                .Select(d => d.Value)
                .GroupBy(d => new { d.Year, d.Month })
                .Select(g => new MonthlyCountDto
                {
                    Year = g.Key.Year,
                    MonthNumber = g.Key.Month,
                    Month = new DateOnly(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                    Count = g.Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.MonthNumber)
                .ToList();

            // Fill missing months
            var completeMonthlyCounts = new List<MonthlyCountDto>();
            for (int i = lastXMonths - 1; i >= 0; i--)
            {
                var d = referenceDate.AddMonths(-i);
                var existing = monthlyCounts.FirstOrDefault(m => m.Year == d.Year && m.MonthNumber == d.Month);
                if (existing != null)
                {
                    completeMonthlyCounts.Add(existing);
                }
                else
                {
                    completeMonthlyCounts.Add(new MonthlyCountDto
                    {
                        Year = d.Year,
                        MonthNumber = d.Month,
                        Month = new DateOnly(d.Year, d.Month, 1).ToString("MMM"),
                        Count = 0
                    });
                }
            }

            trends.Add(new PublicationTrendDto
            {
                TopicName = topic.TopicName,
                MonthlyCounts = completeMonthlyCounts
            });
        }

        return trends;
    }

    public async Task<IEnumerable<HotTopicDto>> GetHotTopicsAsync(int? startYear, int? endYear)
    {
        var maxYearInDb = await _context.Papers
            .Where(p => p.PublicationYear != null)
            .MaxAsync(p => (int?)p.PublicationYear) ?? DateTime.UtcNow.Year;

        var end = endYear ?? maxYearInDb;
        var start = startYear ?? (end - 9); // Default to 10-year window (Lấy 10 năm)
        var step = 2; // Bước nhảy là 2 năm
        var topCount = 10; // Lấy top 10

        // Generate target years with the specified step
        var targetYears = new List<int>();
        for (int y = end; y >= start; y -= step)
        {
            targetYears.Add(y);
        }
        targetYears.Reverse(); // e.g. 2020, 2022, 2024, 2026

        // Find the top topics by total paper count within the chosen year range (start to end)
        var topTopics = await _context.PaperTopics
            .Where(pt => pt.Paper.PublicationYear != null 
                      && pt.Paper.PublicationYear >= start 
                      && pt.Paper.PublicationYear <= end)
            .GroupBy(pt => pt.TopicId)
            .OrderByDescending(g => g.Count())
            .Take(topCount)
            .Select(g => new { TopicId = g.Key, TotalCount = g.Count() })
            .ToListAsync();

        if (!topTopics.Any())
            return Enumerable.Empty<HotTopicDto>();

        var topicIds = topTopics.Select(t => t.TopicId).ToList();
        var overallTotalCount = topTopics.Sum(t => t.TotalCount);

        // Get count details in target years plus the step offset (start - step + 1 to end)
        var queryStartYear = start - step + 1;
        var paperTopicsQuery = await _context.PaperTopics
            .Where(pt => topicIds.Contains(pt.TopicId)
                      && pt.Paper.PublicationYear != null
                      && pt.Paper.PublicationYear >= queryStartYear
                      && pt.Paper.PublicationYear <= end)
            .Select(pt => new
            {
                pt.TopicId,
                pt.Topic.TopicName,
                PublicationYear = pt.Paper.PublicationYear!.Value
            })
            .ToListAsync();

        var trends = new List<HotTopicDto>();

        foreach (var topTopic in topTopics)
        {
            var topicName = paperTopicsQuery
                .FirstOrDefault(pt => pt.TopicId == topTopic.TopicId)?.TopicName
                ?? (await _context.ResearchTopics.FindAsync(topTopic.TopicId))?.TopicName;

            if (topicName == null) continue;

            var yearlyCounts = new List<YearlyCountDto>();
            foreach (var year in targetYears)
            {
                var startInt = year - step + 1;
                var count = paperTopicsQuery
                    .Count(pt => pt.TopicId == topTopic.TopicId 
                              && pt.PublicationYear >= startInt 
                              && pt.PublicationYear <= year);

                yearlyCounts.Add(new YearlyCountDto
                {
                    Year = year,
                    Count = count
                });
            }

            // Calculate Growth Percentage between end (latest year) and start (starting year)
            var currentYearCount = paperTopicsQuery
                .Count(pt => pt.TopicId == topTopic.TopicId && pt.PublicationYear == end);

            var startYearCount = paperTopicsQuery
                .Count(pt => pt.TopicId == topTopic.TopicId && pt.PublicationYear == start);

            double growth = 0;
            if (startYearCount > 0)
            {
                growth = Math.Round((double)(currentYearCount - startYearCount) / startYearCount * 100, 1);
            }
            else if (currentYearCount > 0)
            {
                growth = 100.0;
            }

            // Calculate TotalPercentage: topic's total publications in selected range / overall sum of top topics in range
            double totalPercentage = overallTotalCount > 0
                ? Math.Round((double)topTopic.TotalCount / overallTotalCount * 100, 1)
                : 0;

            trends.Add(new HotTopicDto
            {
                TopicName = topicName,
                PaperCount = currentYearCount,
                GrowthPercentage = growth,
                TotalPercentage = totalPercentage,
                YearlyCounts = yearlyCounts
            });
        }

        return trends;
    }
}
