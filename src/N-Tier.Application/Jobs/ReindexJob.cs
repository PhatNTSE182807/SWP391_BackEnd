using Microsoft.Extensions.Logging;
using N_Tier.Application.Services;

namespace N_Tier.Application.Jobs;

public class ReindexJob : IReindexJob
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<ReindexJob> _logger;

    public ReindexJob(
        IElasticsearchService elasticsearchService,
        ILogger<ReindexJob> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting scheduled reindex job at {Time}", DateTime.UtcNow);
        
        try
        {
            var result = await _elasticsearchService.ReindexAllPapersAsync();
            
            if (result)
            {
                _logger.LogInformation("Scheduled reindex job completed successfully at {Time}", DateTime.UtcNow);
            }
            else
            {
                _logger.LogWarning("Scheduled reindex job completed with warnings at {Time}", DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled reindex job failed at {Time}", DateTime.UtcNow);
            throw;
        }
    }
}
