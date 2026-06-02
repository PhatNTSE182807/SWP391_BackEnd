using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using N_Tier.Application.Models.Search;
using N_Tier.DataAccess.Repositories;
using Nest;

namespace N_Tier.Application.Services.Impl;

public class ElasticsearchService : IElasticsearchService
{
    private readonly IElasticClient _client;
    private readonly ElasticsearchSettings _settings;
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly IPaperRepository _paperRepository;

    public ElasticsearchService(
        IElasticClient client,
        IOptions<ElasticsearchSettings> settings,
        ILogger<ElasticsearchService> logger,
        IPaperRepository paperRepository)
    {
        _client = client;
        _settings = settings.Value;
        _logger = logger;
        _paperRepository = paperRepository;
    }

    public async Task<bool> IndexPaperAsync(PaperDocument paper)
    {
        try
        {
            var response = await _client.IndexDocumentAsync(paper);
            if (!response.IsValid)
            {
                _logger.LogError("Failed to index paper {PaperId}: {Error}", 
                    paper.PaperId, response.OriginalException?.Message);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing paper {PaperId}", paper.PaperId);
            return false;
        }
    }

    public async Task<bool> IndexPapersAsync(IEnumerable<PaperDocument> papers)
    {
        try
        {
            var response = await _client.BulkAsync(b => b
                .Index(_settings.DefaultIndex)
                .IndexMany(papers));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to bulk index papers: {Error}", 
                    response.OriginalException?.Message);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk indexing papers");
            return false;
        }
    }

    public async Task<bool> UpdatePaperAsync(PaperDocument paper)
    {
        try
        {
            var response = await _client.UpdateAsync<PaperDocument>(paper.PaperId, u => u
                .Index(_settings.DefaultIndex)
                .Doc(paper)
                .DocAsUpsert());

            if (!response.IsValid)
            {
                _logger.LogError("Failed to update paper {PaperId}: {Error}", 
                    paper.PaperId, response.OriginalException?.Message);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating paper {PaperId}", paper.PaperId);
            return false;
        }
    }

    public async Task<bool> DeletePaperAsync(Guid paperId)
    {
        try
        {
            var response = await _client.DeleteAsync<PaperDocument>(paperId, d => d
                .Index(_settings.DefaultIndex));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete paper {PaperId}: {Error}", 
                    paperId, response.OriginalException?.Message);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting paper {PaperId}", paperId);
            return false;
        }
    }

    public async Task<SearchPaperResponseModel> SearchPapersAsync(SearchPaperRequestModel request)
    {
        try
        {
            var searchResponse = await _client.SearchAsync<PaperDocument>(s => s
                .Index(_settings.DefaultIndex)
                .From((request.Page - 1) * request.Size)
                .Size(request.Size)
                .Query(q => BuildQuery(q, request))
                .Highlight(h => h
                    .PreTags("<em>")
                    .PostTags("</em>")
                    .Fields(
                        f => f.Field(p => p.Title).FragmentSize(150).NumberOfFragments(1),
                        f => f.Field(p => p.Abstract).FragmentSize(200).NumberOfFragments(3)
                    )
                )
            );

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Search failed: {Error}", searchResponse.OriginalException?.Message);
                return new SearchPaperResponseModel();
            }

            var results = searchResponse.Hits.Select(hit => new SearchPaperResultModel
            {
                PaperId = hit.Source.PaperId,
                Title = hit.Source.Title,
                Abstract = hit.Source.Abstract,
                PublicationYear = hit.Source.PublicationYear,
                CitedByCount = hit.Source.CitedByCount,
                Highlight = new SearchHighlightModel
                {
                    Title = hit.Highlight?.ContainsKey("title") == true 
                        ? hit.Highlight["title"].ToList() 
                        : new List<string>(),
                    Abstract = hit.Highlight?.ContainsKey("abstract") == true 
                        ? hit.Highlight["abstract"].ToList() 
                        : new List<string>()
                }
            }).ToList();

            return new SearchPaperResponseModel
            {
                Total = (int)searchResponse.Total,
                Page = request.Page,
                Size = request.Size,
                Results = results
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching papers");
            return new SearchPaperResponseModel();
        }
    }

    private QueryContainer BuildQuery(QueryContainerDescriptor<PaperDocument> q, SearchPaperRequestModel request)
    {
        var queries = new List<Func<QueryContainerDescriptor<PaperDocument>, QueryContainer>>();

        // Full-text search on title and abstract
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            queries.Add(query => query
                .MultiMatch(mm => mm
                    .Query(request.Q)
                    .Fields(f => f
                        .Field(p => p.Title, 2.0) // Title has higher weight
                        .Field(p => p.Abstract)
                    )
                    .Type(TextQueryType.BestFields)
                    .Fuzziness(Fuzziness.Auto)
                )
            );
        }

        // Year range filter
        if (request.From.HasValue || request.To.HasValue)
        {
            queries.Add(query => query
                .Range(r => r
                    .Field(p => p.PublicationYear)
                    .GreaterThanOrEquals(request.From)
                    .LessThanOrEquals(request.To)
                )
            );
        }

        // Language filter
        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            queries.Add(query => query
                .Term(t => t
                    .Field(p => p.Language)
                    .Value(request.Language.ToLower())
                )
            );
        }

        // Open access filter
        if (request.IsOpenAccess.HasValue)
        {
            queries.Add(query => query
                .Term(t => t
                    .Field(p => p.IsOpenAccess)
                    .Value(request.IsOpenAccess.Value)
                )
            );
        }

        // If no queries, return match all
        if (queries.Count == 0)
        {
            return q.MatchAll();
        }

        // Combine all queries with AND logic
        return q.Bool(b => b.Must(queries.ToArray()));
    }

    public async Task<bool> CreateIndexAsync()
    {
        try
        {
            var indexExists = await _client.Indices.ExistsAsync(_settings.DefaultIndex);
            if (indexExists.Exists)
            {
                _logger.LogInformation("Index {IndexName} already exists", _settings.DefaultIndex);
                return true;
            }

            var response = await _client.Indices.CreateAsync(_settings.DefaultIndex, c => c
                .Map<PaperDocument>(m => m.AutoMap())
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0)
                )
            );

            if (!response.IsValid)
            {
                _logger.LogError("Failed to create index: {Error}", response.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Index {IndexName} created successfully", _settings.DefaultIndex);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index");
            return false;
        }
    }

    public async Task<bool> DeleteIndexAsync()
    {
        try
        {
            var response = await _client.Indices.DeleteAsync(_settings.DefaultIndex);
            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete index: {Error}", response.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Index {IndexName} deleted successfully", _settings.DefaultIndex);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting index");
            return false;
        }
    }

    public async Task<bool> ReindexAllPapersAsync()
    {
        try
        {
            _logger.LogInformation("Starting reindex of all papers");

            // Delete and recreate index
            await DeleteIndexAsync();
            await CreateIndexAsync();

            // Get all papers from database (predicate that matches all)
            var papers = await _paperRepository.GetAllAsync(p => true);
            
            var paperDocuments = papers.Select(p => new PaperDocument
            {
                PaperId = p.PaperId,
                Title = p.Title,
                Abstract = p.Abstract,
                PublicationYear = p.PublicationYear,
                Language = p.Language,
                CitedByCount = p.CitedByCount,
                IsOpenAccess = p.IsOpenAccess,
                Doi = p.Doi,
                PaperType = p.PaperType,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            var result = await IndexPapersAsync(paperDocuments);
            
            _logger.LogInformation("Reindex completed. Total papers indexed: {Count}", papers.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reindexing papers");
            return false;
        }
    }
}
