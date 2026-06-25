using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
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
    private const string AuthorIndexName = "authors";

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
        await EnsureAliasExistsAsync(IndexName, false);

        var paperWithIncludes = await _context.Papers
            .Include(p => p.Journal)
            .Include(p => p.PaperAuthors).ThenInclude(pa => pa.Author)
            .Include(p => p.PaperKeywords).ThenInclude(pk => pk.Keyword)
            .Include(p => p.PaperTopics).ThenInclude(pt => pt.Topic)
                .ThenInclude(t => t.Subfield)
                    .ThenInclude(sf => sf.Field)
                        .ThenInclude(f => f.Domain)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaperId == paper.PaperId);

        if (paperWithIncludes == null)
        {
            _logger.LogWarning("Attempted to index paper {PaperId} but it was not found in database", paper.PaperId);
            return;
        }

        var document = MapToDocument(paperWithIncludes);
        var response = await _elasticClient.IndexAsync(document, idx => idx
            .Index(IndexName)
            .Id(paperWithIncludes.PaperId.ToString()));

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

        var aliasName = IndexName;
        var newIndexName = $"{aliasName}-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

        var existsNew = await _elasticClient.Indices.ExistsAsync(newIndexName);
        if (existsNew.Exists)
        {
            await _elasticClient.Indices.DeleteAsync(newIndexName);
        }

        var existsConcrete = await _elasticClient.Indices.ExistsAsync(aliasName);
        if (existsConcrete.Exists)
        {
            var aliasResponse = await _elasticClient.Indices.GetAliasAsync(g => g.Name(aliasName));
            if (!aliasResponse.IsValidResponse || aliasResponse.Aliases.Count == 0)
            {
                _logger.LogWarning("Concrete index {IndexName} exists instead of alias. Deleting it to make room for alias...", aliasName);
                await _elasticClient.Indices.DeleteAsync(aliasName);
            }
        }

        await CreateIndexAsync(newIndexName);

        var batchSize = 1000;
        var skip = 0;
        var totalIndexed = 0;

        while (true)
        {
            var papers = await _context.Papers
                .Include(p => p.Journal)
                .Include(p => p.PaperAuthors).ThenInclude(pa => pa.Author)
                .Include(p => p.PaperKeywords).ThenInclude(pk => pk.Keyword)
                .Include(p => p.PaperTopics).ThenInclude(pt => pt.Topic)
                    .ThenInclude(t => t.Subfield)
                        .ThenInclude(sf => sf.Field)
                            .ThenInclude(f => f.Domain)
                .AsNoTracking()
                .OrderBy(p => p.PaperId)
                .Skip(skip)
                .Take(batchSize)
                .ToListAsync();

            if (!papers.Any())
                break;

            var documents = papers.Select(MapToDocument).ToList();

            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index(newIndexName)
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

        var oldIndices = new List<string>();
        var getAliasResponse = await _elasticClient.Indices.GetAliasAsync(g => g.Name(aliasName));
        if (getAliasResponse.IsValidResponse)
        {
            oldIndices = getAliasResponse.Aliases.Keys.Select(k => k.ToString()).ToList();
        }

        var actions = new List<IndexUpdateAliasesAction>();
        actions.Add(IndexUpdateAliasesAction.Add(new AddAction
        {
            Alias = aliasName,
            Index = newIndexName
        }));

        foreach (var oldIndex in oldIndices)
        {
            actions.Add(IndexUpdateAliasesAction.Remove(new RemoveAction
            {
                Alias = aliasName,
                Index = oldIndex
            }));
        }

        var updateAliasResponse = await _elasticClient.Indices.UpdateAliasesAsync(new UpdateAliasesRequest
        {
            Actions = actions
        });

        if (!updateAliasResponse.IsValidResponse)
        {
            var error = updateAliasResponse.ElasticsearchServerError?.Error?.Reason ?? "Unknown error";
            _logger.LogError("Failed to update alias {AliasName} to new index {NewIndex}: {Error}", aliasName, newIndexName, error);
            throw new Exception($"Failed to update alias: {error}");
        }

        _logger.LogInformation("Successfully switched alias {AliasName} to point to index {NewIndexName}", aliasName, newIndexName);

        foreach (var oldIndex in oldIndices)
        {
            _logger.LogInformation("Deleting old index {OldIndex}...", oldIndex);
            var deleteResponse = await _elasticClient.Indices.DeleteAsync(oldIndex);
            if (!deleteResponse.IsValidResponse)
            {
                _logger.LogWarning("Failed to delete old index {OldIndex}: {Error}", oldIndex, deleteResponse.ElasticsearchServerError?.Error?.Reason);
            }
            else
            {
                _logger.LogInformation("Deleted old index {OldIndex} successfully", oldIndex);
            }
        }

        _logger.LogInformation("Bulk indexing completed. Total papers indexed: {Total}", totalIndexed);
    }

    public async Task DeleteIndexAsync()
    {
        _logger.LogInformation("Deleting alias and indices for {AliasName}...", IndexName);
        
        var getAliasResponse = await _elasticClient.Indices.GetAliasAsync(g => g.Name(IndexName));
        if (getAliasResponse.IsValidResponse)
        {
            foreach (var indexName in getAliasResponse.Aliases.Keys)
            {
                await _elasticClient.Indices.DeleteAsync(indexName);
            }
        }
        
        var existsConcrete = await _elasticClient.Indices.ExistsAsync(IndexName);
        if (existsConcrete.Exists)
        {
            await _elasticClient.Indices.DeleteAsync(IndexName);
        }
    }

    public async Task RecreateIndexAsync()
    {
        _logger.LogInformation("Recreating index {IndexName} with correct mapping and zero downtime...", IndexName);
        await BulkIndexPapersAsync();
        _logger.LogInformation("Index {IndexName} recreated successfully", IndexName);
    }

    private async Task CreateIndexAsync(string indexName)
    {
        var response = await _elasticClient.Indices.CreateAsync(indexName, c => c
            .Mappings(m => m
                .Properties<PaperDocument>(p => p
                    .Keyword(k => k.PaperId)
                    .Text(t => t.Title, td => td.Analyzer("standard"))
                    .Text(t => t.Abstract, td => td.Analyzer("standard"))
                    .IntegerNumber(i => i.PublicationYear)
                    .IntegerNumber(i => i.CitedByCount)
                    .Keyword(k => k.Language)
                    .Boolean(b => b.IsOpenAccess)
                    .Nested("journal", n => n
                        .Properties(jp => jp
                            .Keyword("journalId")
                            .Keyword("journalName")
                            .Boolean("isOpenAccess")
                        )
                    )
                    .Nested("authors", n => n
                        .Properties(ap => ap
                            .Keyword("authorId")
                            .Keyword("displayName")
                            .IntegerNumber("citedByCount")
                            .IntegerNumber("hIndex")
                        )
                    )
                    .Nested("keywords", n => n
                        .Properties(kp => kp
                            .Keyword("keywordId")
                            .Keyword("keywordName")
                        )
                    )
                    .Nested("topics", n => n
                        .Properties(tp => tp
                            .Keyword("topicId")
                            .Keyword("topicName")
                            .Keyword("subfieldId")
                            .Keyword("subfieldName")
                            .Keyword("fieldId")
                            .Keyword("fieldName")
                            .Keyword("domainId")
                            .Keyword("domainName")
                        )
                    )
                )
            )
        );

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to create index {IndexName}: {Error}",
                indexName, response.ElasticsearchServerError?.Error?.Reason);
            throw new Exception($"Failed to create index {indexName}: {response.ElasticsearchServerError?.Error?.Reason}");
        }

        _logger.LogInformation("Index {IndexName} created successfully", indexName);
    }

    private PaperDocument MapToDocument(Core.Entities.Paper paper)
    {
        var doc = new PaperDocument
        {
            PaperId = paper.PaperId,
            Title = paper.Title,
            Abstract = paper.Abstract,
            PublicationYear = paper.PublicationYear,
            CitedByCount = paper.CitedByCount,
            Language = paper.Language,
            IsOpenAccess = paper.IsOpenAccess
        };

        if (paper.Journal != null)
        {
            doc.Journal = new JournalDocument
            {
                JournalId = paper.Journal.JournalId,
                JournalName = paper.Journal.JournalName,
                IsOpenAccess = paper.Journal.IsOpenAccess
            };
        }

        if (paper.PaperAuthors != null && paper.PaperAuthors.Any())
        {
            doc.Authors = paper.PaperAuthors
                .Where(pa => pa.Author != null)
                .Select(pa => new AuthorDocument
                {
                    AuthorId = pa.Author.AuthorId,
                    DisplayName = pa.Author.DisplayName,
                    CitedByCount = pa.Author.CitedByCount,
                    HIndex = pa.Author.HIndex
                }).ToList();
        }

        if (paper.PaperKeywords != null && paper.PaperKeywords.Any())
        {
            doc.Keywords = paper.PaperKeywords
                .Where(pk => pk.Keyword != null)
                .Select(pk => new KeywordDocument
                {
                    KeywordId = pk.Keyword.KeywordId,
                    KeywordName = pk.Keyword.KeywordName
                }).ToList();
        }

        if (paper.PaperTopics != null && paper.PaperTopics.Any())
        {
            doc.Topics = paper.PaperTopics
                .Where(pt => pt.Topic != null)
                .Select(pt => new TopicDocument
                {
                    TopicId = pt.Topic.TopicId,
                    TopicName = pt.Topic.TopicName,
                    SubfieldId = pt.Topic.Subfield?.SubfieldId,
                    SubfieldName = pt.Topic.Subfield?.SubfieldName,
                    FieldId = pt.Topic.Subfield?.Field?.FieldId,
                    FieldName = pt.Topic.Subfield?.Field?.FieldName,
                    DomainId = pt.Topic.Subfield?.Field?.Domain?.DomainId,
                    DomainName = pt.Topic.Subfield?.Field?.Domain?.DomainName
                }).ToList();
        }

        return doc;
    }

    private string GenerateCacheKey(SearchPaperRequest request)
    {
        return $"search:papers:{request.Q}:{request.Page}:{request.Size}:{request.From}:{request.To}:{request.Language}:{request.IsOpenAccess}";
    }

    public async Task<SearchAuthorResponse> SearchAuthorsAsync(SearchAuthorRequest request)
    {
        var cacheKey = GenerateAuthorCacheKey(request);

        var cachedResult = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedResult))
        {
            _logger.LogInformation("Cache hit for author search query: {Query}", request.Q);
            return JsonConvert.DeserializeObject<SearchAuthorResponse>(cachedResult);
        }

        var mustQueries = new List<Query>();

        // Full-text search on displayName, fullName, affiliations, lastKnownInstitutions
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            mustQueries.Add(new MultiMatchQuery
            {
                Query = request.Q,
                Fields = new[] { "displayName^2", "fullName^2", "affiliations", "lastKnownInstitutions" }, 
                Fuzziness = new Fuzziness("AUTO"),
                Operator = Operator.Or
            });
        }

        // Works count filter
        if (request.MinWorksCount.HasValue || request.MaxWorksCount.HasValue)
        {
            mustQueries.Add(new NumberRangeQuery(new Field("worksCount"))
            {
                Gte = request.MinWorksCount,
                Lte = request.MaxWorksCount
            });
        }

        // Cited by count filter
        if (request.MinCitedByCount.HasValue || request.MaxCitedByCount.HasValue)
        {
            mustQueries.Add(new NumberRangeQuery(new Field("citedByCount"))
            {
                Gte = request.MinCitedByCount,
                Lte = request.MaxCitedByCount
            });
        }

        // H-Index filter
        if (request.MinHIndex.HasValue || request.MaxHIndex.HasValue)
        {
            mustQueries.Add(new NumberRangeQuery(new Field("hIndex"))
            {
                Gte = request.MinHIndex,
                Lte = request.MaxHIndex
            });
        }

        var from = (request.Page - 1) * request.Size;

        var searchResponse = await _elasticClient.SearchAsync<AuthorDocument>(s => s
            .Index(AuthorIndexName)
            .From(from)
            .Size(request.Size)
            .Query(q => q
                .Bool(b => b
                    .Must(mustQueries.ToArray())
                )
            )
            .Highlight(h => h
                .Fields(f => f
                    .Add("displayName", hf => hf
                        .PreTags(new[] { "<em>" })
                        .PostTags(new[] { "</em>" })
                    )
                    .Add("fullName", hf => hf
                        .PreTags(new[] { "<em>" })
                        .PostTags(new[] { "</em>" })
                    )
                    .Add("affiliations", hf => hf
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
            _logger.LogError("Elasticsearch author search error: {Error}", searchResponse.ElasticsearchServerError?.Error?.Reason);
            throw new Exception($"Author search failed: {searchResponse.ElasticsearchServerError?.Error?.Reason}");
        }

        var response = new SearchAuthorResponse
        {
            Total = searchResponse.Total,
            Page = request.Page,
            Size = request.Size,
            Results = searchResponse.Documents.Select((doc, index) =>
            {
                var hit = searchResponse.Hits.ElementAt(index);
                return new SearchAuthorResultItem
                {
                    AuthorId = doc.AuthorId,
                    DisplayName = doc.DisplayName,
                    FullName = doc.FullName,
                    Orcid = doc.Orcid,
                    WorksCount = doc.WorksCount,
                    CitedByCount = doc.CitedByCount,
                    HIndex = doc.HIndex,
                    Affiliations = doc.Affiliations,
                    LastKnownInstitutions = doc.LastKnownInstitutions,
                    Highlight = new SearchAuthorHighlight
                    {
                        DisplayName = hit.Highlight?.TryGetValue("displayName", out var nameHighlights) == true
                            ? nameHighlights.ToList()
                            : new List<string>(),
                        FullName = hit.Highlight?.TryGetValue("fullName", out var fullNameHighlights) == true
                            ? fullNameHighlights.ToList()
                            : new List<string>(),
                        Affiliations = hit.Highlight?.TryGetValue("affiliations", out var affHighlights) == true
                            ? affHighlights.ToList()
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

        _logger.LogInformation("Author search completed. Total results: {Total}", response.Total);
        return response;
    }

    public async Task IndexAuthorAsync(Core.Entities.Author author)
    {
        await EnsureAliasExistsAsync(AuthorIndexName, true);

        var document = MapToAuthorDocument(author);
        var response = await _elasticClient.IndexAsync(document, idx => idx.Index(AuthorIndexName));

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to index author {AuthorId}: {Error}",
                author.AuthorId, response.ElasticsearchServerError?.Error?.Reason);
            throw new Exception($"Failed to index author: {response.ElasticsearchServerError?.Error?.Reason}");
        }

        _logger.LogInformation("Successfully indexed author {AuthorId}", author.AuthorId);
    }

    public async Task BulkIndexAuthorsAsync()
    {
        _logger.LogInformation("Starting bulk indexing of authors...");

        var aliasName = AuthorIndexName;
        var newIndexName = $"{aliasName}-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

        var existsNew = await _elasticClient.Indices.ExistsAsync(newIndexName);
        if (existsNew.Exists)
        {
            await _elasticClient.Indices.DeleteAsync(newIndexName);
        }

        var existsConcrete = await _elasticClient.Indices.ExistsAsync(aliasName);
        if (existsConcrete.Exists)
        {
            var aliasResponse = await _elasticClient.Indices.GetAliasAsync(g => g.Name(aliasName));
            if (!aliasResponse.IsValidResponse || aliasResponse.Aliases.Count == 0)
            {
                _logger.LogWarning("Concrete index {IndexName} exists instead of alias. Deleting it to make room for alias...", aliasName);
                await _elasticClient.Indices.DeleteAsync(aliasName);
            }
        }

        await CreateAuthorIndexAsync(newIndexName);

        var batchSize = 1000;
        var skip = 0;
        var totalIndexed = 0;

        while (true)
        {
            var authors = await _context.Authors
                .AsNoTracking()
                .OrderBy(a => a.AuthorId)
                .Skip(skip)
                .Take(batchSize)
                .ToListAsync();

            if (!authors.Any())
                break;

            var documents = authors.Select(MapToAuthorDocument).ToList();

            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index(newIndexName)
                .IndexMany(documents)
            );

            if (!bulkResponse.IsValidResponse)
            {
                _logger.LogError("Bulk indexing of authors failed: {Error}",
                    bulkResponse.ElasticsearchServerError?.Error?.Reason);
                throw new Exception($"Bulk indexing of authors failed: {bulkResponse.ElasticsearchServerError?.Error?.Reason}");
            }

            totalIndexed += authors.Count;
            skip += batchSize;

            _logger.LogInformation("Indexed {Count} authors. Total: {Total}", authors.Count, totalIndexed);
        }

        var oldIndices = new List<string>();
        var getAliasResponse = await _elasticClient.Indices.GetAliasAsync(g => g.Name(aliasName));
        if (getAliasResponse.IsValidResponse)
        {
            oldIndices = getAliasResponse.Aliases.Keys.Select(k => k.ToString()).ToList();
        }

        var actions = new List<IndexUpdateAliasesAction>();
        actions.Add(IndexUpdateAliasesAction.Add(new AddAction
        {
            Alias = aliasName,
            Index = newIndexName
        }));

        foreach (var oldIndex in oldIndices)
        {
            actions.Add(IndexUpdateAliasesAction.Remove(new RemoveAction
            {
                Alias = aliasName,
                Index = oldIndex
            }));
        }

        var updateAliasResponse = await _elasticClient.Indices.UpdateAliasesAsync(new UpdateAliasesRequest
        {
            Actions = actions
        });

        if (!updateAliasResponse.IsValidResponse)
        {
            var error = updateAliasResponse.ElasticsearchServerError?.Error?.Reason ?? "Unknown error";
            _logger.LogError("Failed to update alias {AliasName} to new index {NewIndex}: {Error}", aliasName, newIndexName, error);
            throw new Exception($"Failed to update alias: {error}");
        }

        _logger.LogInformation("Successfully switched alias {AliasName} to point to index {NewIndexName}", aliasName, newIndexName);

        foreach (var oldIndex in oldIndices)
        {
            _logger.LogInformation("Deleting old index {OldIndex}...", oldIndex);
            var deleteResponse = await _elasticClient.Indices.DeleteAsync(oldIndex);
            if (!deleteResponse.IsValidResponse)
            {
                _logger.LogWarning("Failed to delete old index {OldIndex}: {Error}", oldIndex, deleteResponse.ElasticsearchServerError?.Error?.Reason);
            }
            else
            {
                _logger.LogInformation("Deleted old index {OldIndex} successfully", oldIndex);
            }
        }

        _logger.LogInformation("Bulk indexing of authors completed. Total authors indexed: {Total}", totalIndexed);
    }

    public async Task DeleteAuthorIndexAsync()
    {
        _logger.LogInformation("Deleting alias and indices for {AliasName}...", AuthorIndexName);
        
        var getAliasResponse = await _elasticClient.Indices.GetAliasAsync(g => g.Name(AuthorIndexName));
        if (getAliasResponse.IsValidResponse)
        {
            foreach (var indexName in getAliasResponse.Aliases.Keys)
            {
                await _elasticClient.Indices.DeleteAsync(indexName);
            }
        }
        
        var existsConcrete = await _elasticClient.Indices.ExistsAsync(AuthorIndexName);
        if (existsConcrete.Exists)
        {
            await _elasticClient.Indices.DeleteAsync(AuthorIndexName);
        }
    }

    public async Task RecreateAuthorIndexAsync()
    {
        _logger.LogInformation("Recreating index {IndexName} with correct mapping and zero downtime...", AuthorIndexName);
        await BulkIndexAuthorsAsync();
        _logger.LogInformation("Index {IndexName} recreated successfully", AuthorIndexName);
    }

    private async Task CreateAuthorIndexAsync(string indexName)
    {
        var response = await _elasticClient.Indices.CreateAsync(indexName, c => c
            .Mappings(m => m
                .Properties<AuthorDocument>(p => p
                    .Keyword(k => k.AuthorId)
                    .Text(t => t.DisplayName, td => td.Analyzer("standard"))
                    .Text(t => t.FullName, td => td.Analyzer("standard"))
                    .Keyword(k => k.Orcid)
                    .IntegerNumber(i => i.WorksCount)
                    .IntegerNumber(i => i.CitedByCount)
                    .IntegerNumber(i => i.HIndex)
                    .Text(t => t.Affiliations, td => td.Analyzer("standard"))
                    .Text(t => t.LastKnownInstitutions, td => td.Analyzer("standard"))
                )
            )
        );

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to create author index {IndexName}: {Error}",
                indexName, response.ElasticsearchServerError?.Error?.Reason);
            throw new Exception($"Failed to create author index {indexName}: {response.ElasticsearchServerError?.Error?.Reason}");
        }

        _logger.LogInformation("Index {IndexName} created successfully", indexName);
    }

    private AuthorDocument MapToAuthorDocument(Core.Entities.Author author)
    {
        return new AuthorDocument
        {
            AuthorId = author.AuthorId,
            DisplayName = author.DisplayName,
            FullName = author.FullName,
            Orcid = author.Orcid,
            WorksCount = author.WorksCount,
            CitedByCount = author.CitedByCount,
            HIndex = author.HIndex,
            Affiliations = author.Affiliations,
            LastKnownInstitutions = author.LastKnownInstitutions
        };
    }

    private string GenerateAuthorCacheKey(SearchAuthorRequest request)
    {
        return $"search:authors:{request.Q}:{request.Page}:{request.Size}:{request.MinWorksCount}:{request.MaxWorksCount}:{request.MinCitedByCount}:{request.MaxCitedByCount}:{request.MinHIndex}:{request.MaxHIndex}";
    }

    private async Task EnsureAliasExistsAsync(string aliasName, bool isAuthor)
    {
        var existsResponse = await _elasticClient.Indices.ExistsAsync(aliasName);
        if (!existsResponse.Exists)
        {
            _logger.LogInformation("Index/Alias {AliasName} does not exist. Initializing...", aliasName);
            var initialIndexName = $"{aliasName}-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
            if (isAuthor)
            {
                await CreateAuthorIndexAsync(initialIndexName);
            }
            else
            {
                await CreateIndexAsync(initialIndexName);
            }

            var updateAliasResponse = await _elasticClient.Indices.UpdateAliasesAsync(new UpdateAliasesRequest
            {
                Actions = new IndexUpdateAliasesAction[]
                {
                    IndexUpdateAliasesAction.Add(new AddAction
                    {
                        Alias = aliasName,
                        Index = initialIndexName
                    })
                }
            });

            if (!updateAliasResponse.IsValidResponse)
            {
                _logger.LogError("Failed to initialize alias {AliasName}: {Error}", aliasName, updateAliasResponse.ElasticsearchServerError?.Error?.Reason);
            }
        }
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
    public JournalDocument Journal { get; set; }
    public List<AuthorDocument> Authors { get; set; } = new();
    public List<KeywordDocument> Keywords { get; set; } = new();
    public List<TopicDocument> Topics { get; set; } = new();
}

public class JournalDocument
{
    public Guid? JournalId { get; set; }
    public string JournalName { get; set; }
    public bool? IsOpenAccess { get; set; }
}


public class KeywordDocument
{
    public Guid KeywordId { get; set; }
    public string KeywordName { get; set; }
}

public class TopicDocument
{
    public Guid TopicId { get; set; }
    public string TopicName { get; set; }
    public Guid? SubfieldId { get; set; }
    public string SubfieldName { get; set; }
    public Guid? FieldId { get; set; }
    public string FieldName { get; set; }
    public Guid? DomainId { get; set; }
    public string DomainName { get; set; }
}

public class AuthorDocument
{
    public Guid AuthorId { get; set; }
    public string DisplayName { get; set; }
    public string FullName { get; set; }
    public string Orcid { get; set; }
    public int? WorksCount { get; set; }
    public int? CitedByCount { get; set; }
    public int? HIndex { get; set; }
    public string Affiliations { get; set; }
    public string LastKnownInstitutions { get; set; }
}
