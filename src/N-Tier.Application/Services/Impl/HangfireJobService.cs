using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace N_Tier.Application.Services.Impl;

public class HangfireJobService : IHangfireJobService
{
    private readonly ISearchService _searchService;
    private readonly ILogger<HangfireJobService> _logger;

    public HangfireJobService(ISearchService searchService, ILogger<HangfireJobService> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public void ScheduleRecurringJobs()
    {
        // Reindex papers every 5 minutes
        RecurringJob.AddOrUpdate(
            "reindex-papers-every-5min",
            () => ReindexPapersAsync(),
            "*/5 * * * *" // Every 5 minutes
        );

        _logger.LogInformation("Recurring jobs scheduled successfully - Reindex every 5 minutes");
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ReindexPapersAsync()
    {
        _logger.LogInformation("Starting scheduled paper reindexing...");
        
        try
        {
            await _searchService.BulkIndexPapersAsync();
            _logger.LogInformation("Scheduled paper reindexing completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled paper reindexing");
            throw;
        }
    }

    public string EnqueueReindexJob()
    {
        var jobId = BackgroundJob.Enqueue(() => ReindexPapersAsync());
        _logger.LogInformation("Reindex job enqueued with ID: {JobId}", jobId);
        return jobId;
    }

    public string EnqueueRecreateIndexJob()
    {
        var jobId = BackgroundJob.Enqueue<ISearchService>(x => x.RecreateIndexAsync());
        _logger.LogInformation("Recreate index job enqueued with ID: {JobId}", jobId);
        return jobId;
    }
}
