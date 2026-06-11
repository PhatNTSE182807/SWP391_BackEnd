using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using N_Tier.Application.Models.Search;
using N_Tier.DataAccess.Persistence;
using Newtonsoft.Json;

namespace N_Tier.Application.Services.Impl;

public class SearchService : ISearchService
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly IDistributedCache _cache;
    private readonly DatabaseContext _context;
    private readonly ILogger<SearchService> _logger;
    private const string IndexName = "papers";

    public SearchService(
        ElasticsearchClient elasticClient,
        IDistributedCache cache,
        DatabaseContext context,
        ILogger<SearchService> logger)
    {
        _elasticClient = elasticClient;
        _cache = cache;
        _context = context;
        _logger = logger;
    }

    public async Task<SearchPaperResponse> SearchPapersAsync(SearchPaperRequest request)
    {

        var cacheKey = GenerateCacheKey(request);

        var cachedResult = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedResult))
        {
            _logger.LogInformation("Cache hit for search query: {Query}", request.Q);
            return JsonConvert.DeserializeObject<SearchPaperResponse>(cachedResult);
        }

        var mustQueries = new List<Query>();

        // Full-text search on title and abstract
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            mustQueries.Add(new MultiMatchQuery
            {
                Query = request.Q,
                Fields = new[] { "title^2", "abstract" }, 
                Fuzziness = new Fuzziness("AUTO"),
                Operator = Operator.Or
            });
        }

        // Year range filter - now works with integer type
        if (request.From.HasValue || request.To.HasValue)
        {
            mustQueries.Add(new NumberRangeQuery(new Field("publicationYear"))
            {
                Gte = request.From,
                Lte = request.To
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            mustQueries.Add(new TermQuery(new Field("language.keyword"))
            {
                Value = request.Language
            });
        }

        if (request.IsOpenAccess.HasValue)
        {
            mustQueries.Add(new TermQuery(new Field("isOpenAccess"))
            {
                Value = request.IsOpenAccess.Value
            });
        }

        var from = (request.Page - 1) * request.Size;


        var searchResponse = await _elasticClient.SearchAsync<PaperDocument>(s => s
            .Index(IndexName)
            .From(from)
            .Size(request.Size)
            .Query(q => q
                .Bool(b => b
                    .Must(mustQueries.ToArray())
                )
            )
            .Highlight(h => h
                .Fields(f => f
                    .Add("title", hf => hf
                        .PreTags(new[] { "<em>" })
                        .PostTags(new[] { "</em>" })
                    )
                    .Add("abstract", hf => hf
                        .PreTags(new[] { "<em>" })
                        .PostTags(new[] { "</em>" })
                        .FragmentSize(150)
                        .NumberOfFragments(3)
                    )
                )
            )
            .Sort(sort => sort
                .Score(new ScoreSort { Order = SortOrder.Desc })
                .Field("citedByCount", new FieldSort { Order = SortOrder.Desc })
            )
        );

        if (!searchResponse.IsValidResponse)
        {
            _logger.LogError("Elasticsearch error: {Error}", searchResponse.ElasticsearchServerError?.Error?.Reason);
            throw new Exception($"Search failed: {searchResponse.ElasticsearchServerError?.Error?.Reason}");
        }

        var response = new SearchPaperResponse
        {
            Total = searchResponse.Total,
            Page = request.Page,
            Size = request.Size,
            Results = searchResponse.Documents.Select((doc, index) =>
            {
                var hit = searchResponse.Hits.ElementAt(index);
                return new SearchPaperResultItem
                {
                    PaperId = doc.PaperId,
                    Title = doc.Title,
                    Abstract = doc.Abstract,
                    PublicationYear = doc.PublicationYear,
                    CitedByCount = doc.CitedByCount,
                    Highlight = new SearchHighlight
                    {
                        Title = hit.Highlight?.TryGetValue("title", out var titleHighlights) == true
                            ? titleHighlights.ToList()
                            : new List<string>(),
                        Abstract = hit.Highlight?.TryGetValue("abstract", out var abstractHighlights) == true
                            ? abstractHighlights.ToList()
                            : new List<string>()
                    }
                };
            }).ToList()
        };

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        };
        await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(response), cacheOptions);

        _logger.LogInformation("Search completed. Total results: {Total}", response.Total);
        return response;
    }

    public async Task IndexPaperAsync(Core.Entities.Paper paper)
    {
        var document = MapToDocument(paper);
        var response = await _elasticClient.IndexAsync(document, idx => idx
            .Index(IndexName)
            .Id(paper.PaperId.ToString()));

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to index paper {PaperId}: {Error}",
                paper.PaperId, response.ElasticsearchServerError?.Error?.Reason);
            throw new Exception($"Failed to index paper: {response.ElasticsearchServerError?.Error?.Reason}");
        }

        _logger.LogInformation("Successfully indexed paper {PaperId}", paper.PaperId);
    }

    public async Task BulkIndexPapersAsync()
    {
        _logger.LogInformation("Starting bulk indexing of papers...");

        // Always recreate the index before bulk indexing so old duplicate docs are not kept
        var indexExists = await _elasticClient.Indices.ExistsAsync(IndexName);
        if (indexExists.Exists)
        {
            _logger.LogInformation("Deleting existing index {IndexName} before reindexing to avoid duplicate documents", IndexName);
            await DeleteIndexAsync();
        }

        await CreateIndexAsync();

        var batchSize = 1000;
        var skip = 0;
        var totalIndexed = 0;

        while (true)
        {
            var papers = await _context.Papers
                .AsNoTracking()
                .OrderBy(p => p.PaperId)
                .Skip(skip)
                .Take(batchSize)
                .ToListAsync();

            if (!papers.Any())
                break;

            var documents = papers.Select(MapToDocument).ToList();

            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index(IndexName)
                .Refresh(Refresh.WaitFor)
                .IndexMany(documents, (descriptor, doc) =>
                    descriptor.Id(doc.PaperId.ToString()))
            );

            if (!bulkResponse.IsValidResponse || bulkResponse.Errors)
            {
                var errorMessage = bulkResponse.ElasticsearchServerError?.Error?.Reason
                    ?? bulkResponse.Items?.FirstOrDefault(x => x.Error != null)?.Error?.Reason
                    ?? "Unknown bulk indexing error";

                _logger.LogError("Bulk indexing failed: {Error}", errorMessage);
                throw new Exception($"Bulk indexing failed: {errorMessage}");
            }

            totalIndexed += papers.Count;
            skip += batchSize;

            _logger.LogInformation("Indexed {Count} papers. Total: {Total}", papers.Count, totalIndexed);
        }

        _logger.LogInformation("Bulk indexing completed. Total papers indexed: {Total}", totalIndexed);
    }

    public async Task DeleteIndexAsync()
    {
        _logger.LogInformation("Deleting index {IndexName}...", IndexName);
        
        var response = await _elasticClient.Indices.DeleteAsync(IndexName);
        
        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to delete index {IndexName}: {Error}",
                IndexName, response.ElasticsearchServerError?.Error?.Reason);
            throw new Exception($"Failed to delete index: {response.ElasticsearchServerError?.Error?.Reason}");
        }
        
        _logger.LogInformation("Index {IndexName} deleted successfully", IndexName);
    }

    public async Task RecreateIndexAsync()
    {
        _logger.LogInformation("Recreating index {IndexName} with correct mapping...", IndexName);
        
        // Delete index if exists
        var indexExists = await _elasticClient.Indices.ExistsAsync(IndexName);
        if (indexExists.Exists)
        {
            await DeleteIndexAsync();
        }
        
        // Create new index with correct mapping
        await CreateIndexAsync();
        
        // Reindex all papers
        await BulkIndexPapersAsync();
        
        _logger.LogInformation("Index {IndexName} recreated successfully", IndexName);
    }

    private async Task CreateIndexAsync()
    {
        var response = await _elasticClient.Indices.CreateAsync(IndexName, c => c
            .Mappings(m => m
                .Properties<PaperDocument>(p => p
                    .Keyword(k => k.PaperId)
                    .Text(t => t.Title, td => td.Analyzer("standard"))
                    .Text(t => t.Abstract, td => td.Analyzer("standard"))
                    .IntegerNumber(i => i.PublicationYear)
                    .IntegerNumber(i => i.CitedByCount)
                    .Keyword(k => k.Language)
                    .Boolean(b => b.IsOpenAccess)
                )
            )
        );

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to create index: {Error}",
                response.ElasticsearchServerError?.Error?.Reason);
            throw new Exception($"Failed to create index: {response.ElasticsearchServerError?.Error?.Reason}");
        }

        _logger.LogInformation("Index {IndexName} created successfully", IndexName);
    }

    private PaperDocument MapToDocument(Core.Entities.Paper paper)
    {
        return new PaperDocument
        {
            PaperId = paper.PaperId,
            Title = paper.Title,
            Abstract = paper.Abstract,
            PublicationYear = paper.PublicationYear,
            CitedByCount = paper.CitedByCount,
            Language = paper.Language,
            IsOpenAccess = paper.IsOpenAccess
        };
    }

    private string GenerateCacheKey(SearchPaperRequest request)
    {
        return $"search:papers:{request.Q}:{request.Page}:{request.Size}:{request.From}:{request.To}:{request.Language}:{request.IsOpenAccess}";
    }
}

public class PaperDocument
{
    public Guid PaperId { get; set; }
    public string Title { get; set; }
    public string Abstract { get; set; }
    public int? PublicationYear { get; set; }
    public int? CitedByCount { get; set; }
    public string Language { get; set; }
    public bool? IsOpenAccess { get; set; }
}
