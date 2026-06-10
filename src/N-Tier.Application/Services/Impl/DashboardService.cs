using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using N_Tier.Application.Models.Dashboard;
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

        var startDate = referenceDate.AddMonths(-lastXMonths);
        var startYear = startDate.Year;
        var startMonth = startDate.Month;

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
            for (int i = lastXMonths; i >= 0; i--)
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

    public async Task<IEnumerable<HotTopicDto>> GetHotTopicsAsync(int topCount = 5)
    {
        var maxDate = await _context.Papers.MaxAsync(p => (DateOnly?)p.PublicationDate);
        var referenceDate = maxDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var currentMonthStart = new DateOnly(referenceDate.Year, referenceDate.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        // Find topics with most publications overall to ensure we return enough topics
        var topTopicIds = await _context.PaperTopics
            .GroupBy(pt => pt.TopicId)
            .OrderByDescending(g => g.Count())
            .Take(topCount)
            .Select(g => g.Key)
            .ToListAsync();

        var hotTopics = new List<HotTopicDto>();

        foreach (var topicId in topTopicIds)
        {
            var topic = await _context.ResearchTopics.FindAsync(topicId);
            if (topic == null) continue;

            var currentMonthCount = await _context.PaperTopics
                .Include(pt => pt.Paper)
                .CountAsync(pt => pt.TopicId == topicId && pt.Paper.PublicationDate >= currentMonthStart);

            var previousMonthCount = await _context.PaperTopics
                .Include(pt => pt.Paper)
                .CountAsync(pt => pt.TopicId == topicId && pt.Paper.PublicationDate >= previousMonthStart && pt.Paper.PublicationDate < currentMonthStart);

            double growth = 0;
            if (previousMonthCount > 0)
            {
                growth = Math.Round((double)(currentMonthCount - previousMonthCount) / previousMonthCount * 100, 1);
            }
            else if (currentMonthCount > 0)
            {
                growth = 100.0; // From 0 to something is 100% growth
            }

            hotTopics.Add(new HotTopicDto
            {
                TopicName = topic.TopicName,
                PaperCount = currentMonthCount,
                GrowthPercentage = growth
            });
        }

        return hotTopics;
    }
}
