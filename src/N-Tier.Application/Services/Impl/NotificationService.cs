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
using N_Tier.Application.Models.Notification;
using N_Tier.Core.Entities;
using N_Tier.DataAccess.Persistence;
using N_Tier.Shared.Services;

namespace N_Tier.Application.Services.Impl;

public class NotificationService : INotificationService
{
    private readonly DatabaseContext _context;
    private readonly IClaimService _claimService;
    private readonly ILogger<NotificationService> _logger;
    private readonly bool _isMock;
    private static readonly object _lock = new();
    private static bool _isInitialized;

    public NotificationService(
        DatabaseContext context,
        IClaimService claimService,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _claimService = claimService;
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

            if (!followingUsers.Any()) continue;

            var title = $"New Paper in Followed Topic: {topic.TopicName}";
            var body = $"\"{paper.Title}\" has been published in {topic.TopicName}.";
            var eventType = "NewPaperInFollowedTopic";

            // Persist notifications in DB for all target users
            var now = DateTime.UtcNow.AddHours(7);
            var notificationEntities = followingUsers.Select(uft => new Core.Entities.Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = uft.UserId,
                Title = title,
                Body = body,
                EventType = eventType,
                PaperId = paperId,
                TopicId = topic.TopicId,
                IsRead = false,
                CreatedAt = now
            }).ToList();

            _context.Notifications.AddRange(notificationEntities);
            await _context.SaveChangesAsync();

            // Send FCM push notifications to devices with FCM Token
            var tokens = followingUsers
                .Where(uft => uft.User != null && !string.IsNullOrWhiteSpace(uft.User.FcmToken))
                .Select(uft => uft.User.FcmToken)
                .Distinct()
                .ToList();

            if (tokens.Any())
            {
                var data = new Dictionary<string, string>
                {
                    { "paperId", paperId.ToString() },
                    { "topicId", topic.TopicId.ToString() },
                    { "eventType", eventType }
                };

                await SendMulticastNotificationAsync(tokens, title, body, data);
            }
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

        if (!followingUsers.Any())
        {
            _logger.LogInformation("No users follow journal {JournalId} for paper {PaperId}.", journal.JournalId, paperId);
            return;
        }

        var title = $"New Paper in Followed Journal: {journal.JournalName}";
        var body = $"\"{paper.Title}\" has been published in {journal.JournalName}.";
        var eventType = "NewPaperInFollowedJournal";

        // Persist notifications in DB for target users
        var now = DateTime.UtcNow.AddHours(7);
        var notificationEntities = followingUsers.Select(ufj => new Core.Entities.Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = ufj.UserId,
            Title = title,
            Body = body,
            EventType = eventType,
            PaperId = paperId,
            JournalId = journal.JournalId,
            IsRead = false,
            CreatedAt = now
        }).ToList();

        _context.Notifications.AddRange(notificationEntities);
        await _context.SaveChangesAsync();

        // Send FCM push notifications
        var tokens = followingUsers
            .Where(ufj => ufj.User != null && !string.IsNullOrWhiteSpace(ufj.User.FcmToken))
            .Select(ufj => ufj.User.FcmToken)
            .Distinct()
            .ToList();

        if (tokens.Any())
        {
            var data = new Dictionary<string, string>
            {
                { "paperId", paperId.ToString() },
                { "journalId", journal.JournalId.ToString() },
                { "eventType", eventType }
            };

            await SendMulticastNotificationAsync(tokens, title, body, data);
        }
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

        if (!bookmarks.Any())
        {
            _logger.LogInformation("No bookmarks found for paper {PaperId}.", paperId);
            return;
        }

        var title = "Bookmarked Paper Updated";
        var body = $"\"{paper.Title}\" has been updated with new information.";
        var eventType = "BookmarkedPaperUpdated";

        // Persist notifications in DB for target users
        var now = DateTime.UtcNow.AddHours(7);
        var notificationEntities = bookmarks.Select(ub => new Core.Entities.Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = ub.UserId,
            Title = title,
            Body = body,
            EventType = eventType,
            PaperId = paperId,
            IsRead = false,
            CreatedAt = now
        }).ToList();

        _context.Notifications.AddRange(notificationEntities);
        await _context.SaveChangesAsync();

        // Send FCM push notifications
        var tokens = bookmarks
            .Where(ub => ub.User != null && !string.IsNullOrWhiteSpace(ub.User.FcmToken))
            .Select(ub => ub.User.FcmToken)
            .Distinct()
            .ToList();

        if (tokens.Any())
        {
            var data = new Dictionary<string, string>
            {
                { "paperId", paperId.ToString() },
                { "eventType", eventType }
            };

            await SendMulticastNotificationAsync(tokens, title, body, data);
        }
    }

    public async Task<List<NotificationResponseModel>> GetUserNotificationsAsync()
    {
        var userIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(n => new NotificationResponseModel
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId,
            Title = n.Title,
            Body = n.Body,
            EventType = n.EventType,
            PaperId = n.PaperId,
            TopicId = n.TopicId,
            JournalId = n.JournalId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        var userIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var userIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync()
    {
        var userIdStr = _claimService.GetUserId();
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unreadNotifications.Any())
        {
            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
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
                Notification = new FirebaseAdmin.Messaging.Notification()
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
