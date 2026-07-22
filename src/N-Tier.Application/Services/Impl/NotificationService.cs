using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.Application.Services.Impl;

public class NotificationService : INotificationService
{
    private readonly DatabaseContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly bool _isMock;
    private static readonly object _lock = new();
    private static bool _isInitialized;

    public NotificationService(
        DatabaseContext context,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;

        var credentialPath = configuration["Firebase:CredentialFilePath"];
        
        if (string.IsNullOrEmpty(credentialPath))
        {
            _logger.LogWarning("Firebase credential path is not configured. NotificationService will run in MOCK mode.");
            _isMock = true;
            return;
        }

        // Handle relative paths from app base directory
        var fullPath = Path.IsPathRooted(credentialPath) 
            ? credentialPath 
            : Path.Combine(AppContext.BaseDirectory, credentialPath);

        if (!File.Exists(fullPath))
        {
            // Try fallback to active project root directory
            var fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), credentialPath);
            if (File.Exists(fallbackPath))
            {
                fullPath = fallbackPath;
            }
            else
            {
                _logger.LogWarning("Firebase credential file not found at '{Path}' or '{FallbackPath}'. NotificationService will run in MOCK mode.", fullPath, fallbackPath);
                _isMock = true;
                return;
            }
        }

        if (!_isInitialized)
        {
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    try
                    {
                        if (FirebaseApp.DefaultInstance == null)
                        {
                            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                            {
#pragma warning disable CS0618
                                FirebaseApp.Create(new AppOptions()
                                {
                                    Credential = GoogleCredential.FromStream(stream),
                                });
#pragma warning restore CS0618
                            }
                        }
                        _isInitialized = true;
                        _logger.LogInformation("FirebaseApp initialized successfully from key file: {Path}", fullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to initialize FirebaseApp. Falling back to MOCK mode.");
                        _isMock = true;
                    }
                }
            }
        }
    }

    public async Task SendNewPaperInFollowedTopicNotificationAsync(Guid paperId)
    {
        var paper = await _context.Papers
            .Include(p => p.PaperTopics)
                .ThenInclude(pt => pt.Topic)
            .FirstOrDefaultAsync(p => p.PaperId == paperId);

        if (paper == null)
        {
            _logger.LogWarning("SendNewPaperInFollowedTopicNotificationAsync: Paper not found with ID {PaperId}", paperId);
            return;
        }

        if (paper.PaperTopics == null || !paper.PaperTopics.Any())
        {
            _logger.LogInformation("Paper {PaperId} has no associated topics.", paperId);
            return;
        }

        foreach (var paperTopic in paper.PaperTopics)
        {
            var topic = paperTopic.Topic;
            if (topic == null) continue;

            var followingUsers = await _context.UserFollowingTopics
                .Where(uft => uft.TopicId == topic.TopicId)
                .Include(uft => uft.User)
                .ToListAsync();

            var tokens = followingUsers
                .Where(uft => uft.User != null && !string.IsNullOrWhiteSpace(uft.User.FcmToken))
                .Select(uft => uft.User.FcmToken)
                .Distinct()
                .ToList();

            if (!tokens.Any()) continue;

            var title = $"New Paper in Followed Topic: {topic.TopicName}";
            var body = $"\"{paper.Title}\" has been published in {topic.TopicName}.";
            var data = new Dictionary<string, string>
            {
                { "paperId", paperId.ToString() },
                { "topicId", topic.TopicId.ToString() },
                { "eventType", "NewPaperInFollowedTopic" }
            };

            await SendMulticastNotificationAsync(tokens, title, body, data);
        }
    }

    public async Task SendNewPaperInFollowedJournalNotificationAsync(Guid paperId)
    {
        var paper = await _context.Papers
            .Include(p => p.Journal)
            .FirstOrDefaultAsync(p => p.PaperId == paperId);

        if (paper == null)
        {
            _logger.LogWarning("SendNewPaperInFollowedJournalNotificationAsync: Paper not found with ID {PaperId}", paperId);
            return;
        }

        if (paper.Journal == null)
        {
            _logger.LogInformation("Paper {PaperId} has no associated journal.", paperId);
            return;
        }

        var journal = paper.Journal;
        var followingUsers = await _context.UserFollowingJournals
            .Where(ufj => ufj.JournalId == journal.JournalId)
            .Include(ufj => ufj.User)
            .ToListAsync();

        var tokens = followingUsers
            .Where(ufj => ufj.User != null && !string.IsNullOrWhiteSpace(ufj.User.FcmToken))
            .Select(ufj => ufj.User.FcmToken)
            .Distinct()
            .ToList();

        if (!tokens.Any())
        {
            _logger.LogInformation("No users follow journal {JournalId} for paper {PaperId}.", journal.JournalId, paperId);
            return;
        }

        var title = $"New Paper in Followed Journal: {journal.JournalName}";
        var body = $"\"{paper.Title}\" has been published in {journal.JournalName}.";
        var data = new Dictionary<string, string>
        {
            { "paperId", paperId.ToString() },
            { "journalId", journal.JournalId.ToString() },
            { "eventType", "NewPaperInFollowedJournal" }
        };

        await SendMulticastNotificationAsync(tokens, title, body, data);
    }

    public async Task SendBookmarkedPaperUpdatedNotificationAsync(Guid paperId)
    {
        var paper = await _context.Papers
            .FirstOrDefaultAsync(p => p.PaperId == paperId);

        if (paper == null)
        {
            _logger.LogWarning("SendBookmarkedPaperUpdatedNotificationAsync: Paper not found with ID {PaperId}", paperId);
            return;
        }

        var bookmarks = await _context.UserBookmarks
            .Where(ub => ub.PaperId == paperId)
            .Include(ub => ub.User)
            .ToListAsync();

        var tokens = bookmarks
            .Where(ub => ub.User != null && !string.IsNullOrWhiteSpace(ub.User.FcmToken))
            .Select(ub => ub.User.FcmToken)
            .Distinct()
            .ToList();

        if (!tokens.Any())
        {
            _logger.LogInformation("No bookmarks found for paper {PaperId}.", paperId);
            return;
        }

        var title = "Bookmarked Paper Updated";
        var body = $"\"{paper.Title}\" has been updated with new information.";
        var data = new Dictionary<string, string>
        {
            { "paperId", paperId.ToString() },
            { "eventType", "BookmarkedPaperUpdated" }
        };

        await SendMulticastNotificationAsync(tokens, title, body, data);
    }

    private async Task SendMulticastNotificationAsync(List<string> tokens, string title, string body, Dictionary<string, string> data)
    {
        var uniqueTokens = tokens.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
        if (!uniqueTokens.Any()) return;

        if (_isMock)
        {
            _logger.LogInformation(
                "MOCK FCM NOTIFICATION SENT:\n  Title: {Title}\n  Body: {Body}\n  Target count: {Count}\n  Tokens: {Tokens}\n  Data: {Data}", 
                title, 
                body, 
                uniqueTokens.Count, 
                string.Join(", ", uniqueTokens),
                string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}"))
            );
            return;
        }

        try
        {
#pragma warning disable CS0618
            var message = new MulticastMessage()
            {
                Tokens = uniqueTokens,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
#pragma warning restore CS0618
            _logger.LogInformation("FCM Send Completed. Success: {SuccessCount}, Failure: {FailureCount}", response.SuccessCount, response.FailureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending FCM multicast notification");
        }
    }
}
