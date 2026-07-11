using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N_Tier.Application.Models.Analytics;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.Application.Services.Impl;

public class AnalyticsService : IAnalyticsService
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly ILogger<AnalyticsService> _logger;
    private readonly DatabaseContext _context;
    private const string IndexName = "papers";

    public AnalyticsService(ElasticsearchClient elasticClient, ILogger<AnalyticsService> logger, DatabaseContext context)
    {
        _elasticClient = elasticClient;
        _logger = logger;
        _context = context;
    }

    #region Research Trends (Elasticsearch)

    public async Task<List<ChartDataPoint>> GetPaperCountByYearAsync()
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("by_year", agg => agg
                        .Terms(t => t
                            .Field("publicationYear")
                            .Size(100)
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetPaperCountByYearAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var terms = response.Aggregations.GetLongTerms("by_year");
            if (terms == null) return new List<ChartDataPoint>();

            return terms.Buckets
                .Select(b => new ChartDataPoint
                {
                    Key = b.Key.ToString(),
                    Value = b.DocCount
                })
                .OrderBy(x => x.Key)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Analytics] ES Error in GetPaperCountByYearAsync");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<List<ChartDataPoint>> GetCitationsByYearAsync()
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("by_year", agg => agg
                        .Terms(t => t
                            .Field("publicationYear")
                            .Size(100)
                        )
                        .Aggregations(sub => sub
                            .Add("total_citations", subAgg => subAgg
                                .Sum(sum => sum.Field("citedByCount"))
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetCitationsByYearAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var terms = response.Aggregations.GetLongTerms("by_year");
            if (terms == null) return new List<ChartDataPoint>();

            var result = new List<ChartDataPoint>();
            foreach (var bucket in terms.Buckets)
            {
                var sumAgg = bucket.Aggregations.GetSum("total_citations");
                result.Add(new ChartDataPoint
                {
                    Key = bucket.Key.ToString(),
                    Value = sumAgg?.Value ?? 0
                });
            }

            return result.OrderBy(x => x.Key).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetCitationsByYearAsync while querying Elasticsearch");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<List<ChartDataPoint>> GetTopTopicsAsync(int size)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("nested_topics", agg => agg
                        .Nested(n => n.Path("topics"))
                        .Aggregations(sub => sub
                            .Add("top_topics", subAgg => subAgg
                                .Terms(t => t
                                    .Field("topics.topicName")
                                    .Size(size)
                                )
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetTopTopicsAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var nested = response.Aggregations.GetNested("nested_topics");
            if (nested == null) return new List<ChartDataPoint>();

            var terms = nested.Aggregations.GetStringTerms("top_topics");
            if (terms == null) return new List<ChartDataPoint>();

            return terms.Buckets
                .Select(b => new ChartDataPoint
                {
                    Key = b.Key.ToString(),
                    Value = b.DocCount
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Analytics] ES Error in GetTopTopicsAsync");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<List<ChartDataPoint>> GetTopDomainsAsync(int size)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("nested_topics", agg => agg
                        .Nested(n => n.Path("topics"))
                        .Aggregations(sub => sub
                            .Add("top_domains", subAgg => subAgg
                                .Terms(t => t
                                    .Field("topics.domainName")
                                    .Size(size)
                                )
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetTopDomainsAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var nested = response.Aggregations.GetNested("nested_topics");
            if (nested == null) return new List<ChartDataPoint>();

            var terms = nested.Aggregations.GetStringTerms("top_domains");
            if (terms == null) return new List<ChartDataPoint>();

            return terms.Buckets
                .Select(b => new ChartDataPoint
                {
                    Key = b.Key.ToString(),
                    Value = b.DocCount
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetTopDomainsAsync while querying Elasticsearch");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<List<SeriesDataDto>> GetKeywordTrendOverTimeAsync(List<string> keywords)
    {
        try
        {
            if (keywords == null || !keywords.Any())
            {
                // If no keywords provided, find top 5 keywords overall first
                var topKeywords = await GetKeywordCloudAsync(5);
                keywords = topKeywords.Select(k => k.Key).ToList();
            }

            if (!keywords.Any()) return new List<SeriesDataDto>();

            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("by_year", agg => agg
                        .Terms(t => t
                            .Field("publicationYear")
                            .Size(100)
                        )
                        .Aggregations(sub => sub
                            .Add("nested_keywords", subAgg => subAgg
                                .Nested(n => n.Path("keywords"))
                                .Aggregations(subSub => subSub
                                    .Add("keyword_buckets", t2 => t2
                                        .Terms(t3 => t3
                                            .Field("keywords.keywordName.keyword")
                                            .Size(1000)
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetKeywordTrendOverTimeAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<SeriesDataDto>();
            }

            var terms = response.Aggregations.GetLongTerms("by_year");
            if (terms == null) return new List<SeriesDataDto>();

            var result = keywords.ToDictionary(k => k, k => new List<ChartDataPoint>());

            foreach (var yearBucket in terms.Buckets)
            {
                var yearStr = yearBucket.Key.ToString();
                var nested = yearBucket.Aggregations.GetNested("nested_keywords");
                var kwTerms = nested?.Aggregations.GetStringTerms("keyword_buckets");

                if (kwTerms != null)
                {
                    foreach (var kw in keywords)
                    {
                        var kwBucket = kwTerms.Buckets.FirstOrDefault(b => string.Equals(b.Key.ToString(), kw, StringComparison.OrdinalIgnoreCase));
                        var count = kwBucket?.DocCount ?? 0;
                        result[kw].Add(new ChartDataPoint
                        {
                            Key = yearStr,
                            Value = count
                        });
                    }
                }
                else
                {
                    foreach (var kw in keywords)
                    {
                        result[kw].Add(new ChartDataPoint { Key = yearStr, Value = 0 });
                    }
                }
            }

            return result.Select(kv => new SeriesDataDto
            {
                SeriesName = kv.Key,
                DataPoints = kv.Value.OrderBy(dp => dp.Key).ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetKeywordTrendOverTimeAsync while querying Elasticsearch");
            return new List<SeriesDataDto>();
        }
    }

    #endregion

    #region Author Statistics (Elasticsearch)

    public async Task<List<ChartDataPoint>> GetTopAuthorsByCitationsAsync(int size)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("nested_authors", agg => agg
                        .Nested(n => n.Path("authors"))
                        .Aggregations(sub => sub
                            .Add("author_buckets", t => t
                                .Terms(terms => terms
                                    .Field("authors.displayName.keyword")
                                    .Size(size)
                                    .Order(new[] { KeyValuePair.Create<Field, SortOrder>("sum_citations", SortOrder.Desc) })
                                )
                                .Aggregations(subSub => subSub
                                    .Add("sum_citations", sumAgg => sumAgg.Sum(sum => sum.Field("authors.citedByCount")))
                                )
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetTopAuthorsByCitationsAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var nested = response.Aggregations.GetNested("nested_authors");
            if (nested == null) return new List<ChartDataPoint>();

            var terms = nested.Aggregations.GetStringTerms("author_buckets");
            if (terms == null) return new List<ChartDataPoint>();

            return terms.Buckets
                .Select(b => {
                    var sumVal = b.Aggregations.GetSum("sum_citations")?.Value ?? 0;
                    return new ChartDataPoint
                    {
                        Key = b.Key.ToString(),
                        Value = sumVal
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetTopAuthorsByCitationsAsync while querying Elasticsearch");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<List<ChartDataPoint>> GetTopAuthorsByHIndexAsync(int size)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("nested_authors", agg => agg
                        .Nested(n => n.Path("authors"))
                        .Aggregations(sub => sub
                            .Add("author_buckets", t => t
                                .Terms(terms => terms
                                    .Field("authors.displayName.keyword")
                                    .Size(size)
                                    .Order(new[] { KeyValuePair.Create<Field, SortOrder>("max_hindex", SortOrder.Desc) })
                                )
                                .Aggregations(subSub => subSub
                                    .Add("max_hindex", maxAgg => maxAgg.Max(max => max.Field("authors.hIndex")))
                                )
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetTopAuthorsByHIndexAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var nested = response.Aggregations.GetNested("nested_authors");
            if (nested == null) return new List<ChartDataPoint>();

            var terms = nested.Aggregations.GetStringTerms("author_buckets");
            if (terms == null) return new List<ChartDataPoint>();

            return terms.Buckets
                .Select(b => {
                    var maxVal = b.Aggregations.GetMax("max_hindex")?.Value ?? 0;
                    return new ChartDataPoint
                    {
                        Key = b.Key.ToString(),
                        Value = maxVal
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetTopAuthorsByHIndexAsync while querying Elasticsearch");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<NetworkGraphDto> GetAuthorCollaborationNetworkAsync(int size)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("nested_authors", agg => agg
                        .Nested(n => n.Path("authors"))
                        .Aggregations(sub => sub
                            .Add("author_buckets", t => t
                                .Terms(terms => terms
                                    .Field("authors.displayName.keyword")
                                    .Size(size)
                                )
                                .Aggregations(subSub => subSub
                                    .Add("nested_coauthors", nc => nc
                                        .Nested(n2 => n2.Path("authors"))
                                        .Aggregations(subSubSub => subSubSub
                                            .Add("coauthor_buckets", t2 => t2
                                                .Terms(terms2 => terms2
                                                    .Field("authors.displayName.keyword")
                                                    .Size(10)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );

            var graph = new NetworkGraphDto();
            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetAuthorCollaborationNetworkAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return graph;
            }

            var nested = response.Aggregations.GetNested("nested_authors");
            var terms = nested?.Aggregations.GetStringTerms("author_buckets");
            if (terms == null) return graph;

            var nodeMap = new Dictionary<string, GraphNode>();
            var edgeMap = new Dictionary<string, GraphEdge>();

            foreach (var bucket in terms.Buckets)
            {
                var authorName = bucket.Key.ToString();
                if (!nodeMap.ContainsKey(authorName))
                {
                    nodeMap[authorName] = new GraphNode
                    {
                        Id = authorName,
                        Label = authorName,
                        Size = bucket.DocCount,
                        Group = "Author"
                    };
                }

                var coauthorsNested = bucket.Aggregations.GetNested("nested_coauthors");
                var coauthorsTerms = coauthorsNested?.Aggregations.GetStringTerms("coauthor_buckets");

                if (coauthorsTerms != null)
                {
                    foreach (var coauthorBucket in coauthorsTerms.Buckets)
                    {
                        var coauthorName = coauthorBucket.Key.ToString();
                        if (string.Equals(authorName, coauthorName, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!nodeMap.ContainsKey(coauthorName))
                        {
                            nodeMap[coauthorName] = new GraphNode
                            {
                                Id = coauthorName,
                                Label = coauthorName,
                                Size = coauthorBucket.DocCount,
                                Group = "Author"
                            };
                        }

                        // Create alphabetized edge key to avoid bidirectional duplicates
                        var sortedPair = new[] { authorName, coauthorName }.OrderBy(n => n).ToArray();
                        var edgeKey = $"{sortedPair[0]}-{sortedPair[1]}";

                        if (!edgeMap.ContainsKey(edgeKey))
                        {
                            edgeMap[edgeKey] = new GraphEdge
                            {
                                Source = sortedPair[0],
                                Target = sortedPair[1],
                                Weight = coauthorBucket.DocCount
                            };
                        }
                        else
                        {
                            // In case of overlap, use the max weight
                            edgeMap[edgeKey].Weight = Math.Max(edgeMap[edgeKey].Weight, coauthorBucket.DocCount);
                        }
                    }
                }
            }

            graph.Nodes = nodeMap.Values.ToList();
            graph.Edges = edgeMap.Values.ToList();
            return graph;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetAuthorCollaborationNetworkAsync while querying Elasticsearch");
            return new NetworkGraphDto();
        }
    }

    #endregion

    #region Journal Statistics (Elasticsearch)

    public async Task<List<ChartDataPoint>> GetTopJournalsByPaperCountAsync(int size)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("nested_journal", agg => agg
                        .Nested(n => n.Path("journal"))
                        .Aggregations(sub => sub
                            .Add("top_journals", t => t
                                .Terms(terms => terms
                                    .Field("journal.journalName.keyword")
                                    .Size(size)
                                )
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetTopJournalsByPaperCountAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var nested = response.Aggregations.GetNested("nested_journal");
            if (nested == null) return new List<ChartDataPoint>();

            var terms = nested.Aggregations.GetStringTerms("top_journals");
            if (terms == null) return new List<ChartDataPoint>();

            return terms.Buckets
                .Select(b => new ChartDataPoint
                {
                    Key = b.Key.ToString(),
                    Value = b.DocCount
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetTopJournalsByPaperCountAsync while querying Elasticsearch");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<List<ChartDataPoint>> GetTopJournalsByCitationsAsync(int size)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("nested_journal", agg => agg
                        .Nested(n => n.Path("journal"))
                        .Aggregations(sub => sub
                            .Add("top_journals", t => t
                                .Terms(terms => terms
                                    .Field("journal.journalName.keyword")
                                    .Size(size)
                                    .Order(new[] { KeyValuePair.Create<Field, SortOrder>("rev_citations > sum_citations", SortOrder.Desc) })
                                )
                                .Aggregations(subSub => subSub
                                    .Add("rev_citations", rev => rev
                                        .ReverseNested(rn => { })
                                        .Aggregations(parentSub => parentSub
                                            .Add("sum_citations", sumAgg => sumAgg.Sum(sum => sum.Field("citedByCount")))
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetTopJournalsByCitationsAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var nested = response.Aggregations.GetNested("nested_journal");
            if (nested == null) return new List<ChartDataPoint>();

            var terms = nested.Aggregations.GetStringTerms("top_journals");
            if (terms == null) return new List<ChartDataPoint>();

            return terms.Buckets
                .Select(b => {
                    var revNested = b.Aggregations.GetReverseNested("rev_citations");
                    var sumVal = revNested?.Aggregations.GetSum("sum_citations")?.Value ?? 0;
                    return new ChartDataPoint
                    {
                        Key = b.Key.ToString(),
                        Value = sumVal
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetTopJournalsByCitationsAsync while querying Elasticsearch");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<List<ChartDataPoint>> GetOpenAccessRatioAsync()
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("open_access", agg => agg
                        .Terms(t => t
                            .Field("isOpenAccess")
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetOpenAccessRatioAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var terms = response.Aggregations.GetStringTerms("open_access");
            if (terms != null)
            {
                return terms.Buckets
                    .Select(b => {
                        var isOa = b.Key.ToString();
                        var label = isOa == "1" || isOa == "true" ? "Open Access" : "Closed";
                        return new ChartDataPoint
                        {
                            Key = label,
                            Value = b.DocCount
                        };
                    })
                    .ToList();
            }

            var longTerms = response.Aggregations.GetLongTerms("open_access");
            if (longTerms != null)
            {
                return longTerms.Buckets
                    .Select(b => {
                        var isOa = b.Key.ToString();
                        var label = isOa == "1" || isOa == "true" ? "Open Access" : "Closed";
                        return new ChartDataPoint
                        {
                            Key = label,
                            Value = b.DocCount
                        };
                    })
                    .ToList();
            }

            return new List<ChartDataPoint>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetOpenAccessRatioAsync while querying Elasticsearch");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<List<JournalTrackerDto>> GetJournalTrackerAsync(int size = 20, int years = 5)
    {
        var maxYear = await _context.Papers
            .Where(p => p.JournalId != null && p.PublicationYear != null)
            .MaxAsync(p => (int?)p.PublicationYear);

        if (!maxYear.HasValue)
            return new List<JournalTrackerDto>();

        var endYear = maxYear.Value;
        var startYear = endYear - years + 1;

        var journalRows = await _context.Papers
            .AsNoTracking()
            .Where(p => p.JournalId != null
                        && p.PublicationYear != null
                        && p.PublicationYear >= startYear
                        && p.PublicationYear <= endYear)
            .GroupBy(p => new
            {
                JournalId = p.JournalId!.Value,
                p.Journal.JournalName,
                p.Journal.Publisher,
                p.Journal.HomepageUrl,
                p.Journal.IsOpenAccess
            })
            .Select(g => new
            {
                g.Key.JournalId,
                g.Key.JournalName,
                g.Key.Publisher,
                g.Key.HomepageUrl,
                IsOpenAccess = g.Key.IsOpenAccess ?? false,
                PaperCount = g.Count(),
                CitationCount = g.Sum(p => p.CitedByCount ?? 0),
                LastPublicationYear = g.Max(p => p.PublicationYear),
                StartYearCount = g.Count(p => p.PublicationYear == startYear),
                EndYearCount = g.Count(p => p.PublicationYear == endYear)
            })
            .OrderByDescending(x => x.PaperCount)
            .Take(size)
            .ToListAsync();

        if (!journalRows.Any())
            return new List<JournalTrackerDto>();

        var journalIds = journalRows.Select(j => j.JournalId).ToList();

        var keywordRows = await _context.PaperKeywords
            .AsNoTracking()
            .Where(pk => pk.Paper.JournalId != null
                         && journalIds.Contains(pk.Paper.JournalId.Value)
                         && pk.Paper.PublicationYear != null
                         && pk.Paper.PublicationYear >= startYear
                         && pk.Paper.PublicationYear <= endYear)
            .GroupBy(pk => new
            {
                JournalId = pk.Paper.JournalId!.Value,
                pk.Keyword.KeywordName
            })
            .Select(g => new
            {
                g.Key.JournalId,
                g.Key.KeywordName,
                Count = g.Count()
            })
            .ToListAsync();

        var keywordsByJournal = keywordRows
            .GroupBy(row => row.JournalId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(row => row.Count)
                    .ThenBy(row => row.KeywordName)
                    .Take(3)
                    .Select(row => row.KeywordName)
                    .ToList());

        return journalRows.Select(journal =>
        {
            double growth = 0;
            if (journal.StartYearCount > 0)
            {
                growth = Math.Round((double)(journal.EndYearCount - journal.StartYearCount) / journal.StartYearCount * 100, 1);
            }
            else if (journal.EndYearCount > 0)
            {
                growth = 100.0;
            }

            return new JournalTrackerDto
            {
                JournalId = journal.JournalId,
                JournalName = journal.JournalName,
                Publisher = journal.Publisher,
                HomepageUrl = journal.HomepageUrl,
                IsOpenAccess = journal.IsOpenAccess,
                PaperCount = journal.PaperCount,
                CitationCount = journal.CitationCount,
                GrowthPercentage = growth,
                LastPublicationYear = journal.LastPublicationYear,
                TopKeywords = keywordsByJournal.TryGetValue(journal.JournalId, out var keywords)
                    ? keywords
                    : new List<string>()
            };
        }).ToList();
    }

    #endregion

    #region Keyword Statistics (Elasticsearch)

    public async Task<List<ChartDataPoint>> GetKeywordCloudAsync(int size)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
                .Index(IndexName)
                .Size(0)
                .Aggregations(aggs => aggs
                    .Add("nested_keywords", agg => agg
                        .Nested(n => n.Path("keywords"))
                        .Aggregations(sub => sub
                            .Add("top_keywords", subAgg => subAgg
                                .Terms(t => t
                                    .Field("keywords.keywordName.keyword")
                                    .Size(size)
                                )
                            )
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetKeywordCloudAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<ChartDataPoint>();
            }

            var nested = response.Aggregations.GetNested("nested_keywords");
            if (nested == null) return new List<ChartDataPoint>();

            var terms = nested.Aggregations.GetStringTerms("top_keywords");
            if (terms == null) return new List<ChartDataPoint>();

            return terms.Buckets
                .Select(b => new ChartDataPoint
                {
                    Key = b.Key.ToString(),
                    Value = b.DocCount
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetKeywordCloudAsync while querying Elasticsearch");
            return new List<ChartDataPoint>();
        }
    }

    public async Task<List<SeriesDataDto>> GetTopKeywordsByYearAsync(int size)
    {
        // First get top size keywords overall
        var overallTopKeywords = await GetKeywordCloudAsync(size);
        var keywords = overallTopKeywords.Select(k => k.Key).ToList();

        if (!keywords.Any()) return new List<SeriesDataDto>();

        var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
            .Index(IndexName)
            .Size(0)
            .Aggregations(aggs => aggs
                .Add("by_year", agg => agg
                    .Terms(t => t
                        .Field("publicationYear")
                        .Size(10)
                    )
                    .Aggregations(sub => sub
                        .Add("nested_keywords", subAgg => subAgg
                            .Nested(n => n.Path("keywords"))
                            .Aggregations(subSub => subSub
                                .Add("keyword_buckets", t2 => t2
                                    .Terms(t3 => t3
                                        .Field("keywords.keywordName.keyword")
                                        .Size(100)
                                    )
                                )
                            )
                        )
                    )
                )
            )
        );

        try
        {
            if (!response.IsValidResponse)
            {
                _logger.LogError("ES Error in GetTopKeywordsByYearAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
                return new List<SeriesDataDto>();
            }

            var terms = response.Aggregations.GetLongTerms("by_year");
            if (terms == null) return new List<SeriesDataDto>();

            var result = keywords.ToDictionary(k => k, k => new List<ChartDataPoint>());

            foreach (var yearBucket in terms.Buckets)
            {
                var yearStr = yearBucket.Key.ToString();
                var nested = yearBucket.Aggregations.GetNested("nested_keywords");
                var kwTerms = nested?.Aggregations.GetStringTerms("keyword_buckets");

                if (kwTerms != null)
                {
                    foreach (var kw in keywords)
                    {
                        var kwBucket = kwTerms.Buckets.FirstOrDefault(b => string.Equals(b.Key.ToString(), kw, StringComparison.OrdinalIgnoreCase));
                        var count = kwBucket?.DocCount ?? 0;
                        result[kw].Add(new ChartDataPoint
                        {
                            Key = yearStr,
                            Value = count
                        });
                    }
                }
                else
                {
                    foreach (var kw in keywords)
                    {
                        result[kw].Add(new ChartDataPoint { Key = yearStr, Value = 0 });
                    }
                }
            }

            return result.Select(kv => new SeriesDataDto
            {
                SeriesName = kv.Key,
                DataPoints = kv.Value.OrderBy(dp => dp.Key).ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetTopKeywordsByYearAsync while querying Elasticsearch");
            return new List<SeriesDataDto>();
        }
    }

    public async Task<NetworkGraphDto> GetKeywordCoOccurrenceNetworkAsync(int size)
    {
        var response = await _elasticClient.SearchAsync<PaperDocument>(s => s
            .Index(IndexName)
            .Size(0)
            .Aggregations(aggs => aggs
                .Add("nested_keywords", agg => agg
                    .Nested(n => n.Path("keywords"))
                    .Aggregations(sub => sub
                        .Add("keyword_buckets", t => t
                            .Terms(terms => terms
                                .Field("keywords.keywordName.keyword")
                                .Size(size)
                            )
                            .Aggregations(subSub => subSub
                                .Add("nested_cooccurrences", nc => nc
                                    .Nested(n2 => n2.Path("keywords"))
                                    .Aggregations(subSubSub => subSubSub
                                        .Add("co_buckets", t2 => t2
                                            .Terms(terms2 => terms2
                                                .Field("keywords.keywordName.keyword")
                                                .Size(10)
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            )
        );

        var graph = new NetworkGraphDto();
        if (!response.IsValidResponse)
        {
            _logger.LogError("ES Error in GetKeywordCoOccurrenceNetworkAsync: {Error}", response.ElasticsearchServerError?.Error?.Reason);
            return graph;
        }

        var nested = response.Aggregations.GetNested("nested_keywords");
        var terms = nested?.Aggregations.GetStringTerms("keyword_buckets");
        if (terms == null) return graph;

        var nodeMap = new Dictionary<string, GraphNode>();
        var edgeMap = new Dictionary<string, GraphEdge>();

        foreach (var bucket in terms.Buckets)
        {
            var keywordName = bucket.Key.ToString();
            if (!nodeMap.ContainsKey(keywordName))
            {
                nodeMap[keywordName] = new GraphNode
                {
                    Id = keywordName,
                    Label = keywordName,
                    Size = bucket.DocCount,
                    Group = "Keyword"
                };
            }

            var coNested = bucket.Aggregations.GetNested("nested_cooccurrences");
            var coTerms = coNested?.Aggregations.GetStringTerms("co_buckets");

            if (coTerms != null)
            {
                foreach (var coBucket in coTerms.Buckets)
                {
                    var coKeywordName = coBucket.Key.ToString();
                    if (string.Equals(keywordName, coKeywordName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!nodeMap.ContainsKey(coKeywordName))
                    {
                        nodeMap[coKeywordName] = new GraphNode
                        {
                            Id = coKeywordName,
                            Label = coKeywordName,
                            Size = coBucket.DocCount,
                            Group = "Keyword"
                        };
                    }

                    var sortedPair = new[] { keywordName, coKeywordName }.OrderBy(n => n).ToArray();
                    var edgeKey = $"{sortedPair[0]}-{sortedPair[1]}";

                    if (!edgeMap.ContainsKey(edgeKey))
                    {
                        edgeMap[edgeKey] = new GraphEdge
                        {
                            Source = sortedPair[0],
                            Target = sortedPair[1],
                            Weight = coBucket.DocCount
                        };
                    }
                    else
                    {
                        edgeMap[edgeKey].Weight = Math.Max(edgeMap[edgeKey].Weight, coBucket.DocCount);
                    }
                }
            }
        }

        graph.Nodes = nodeMap.Values.ToList();
        graph.Edges = edgeMap.Values.ToList();
        return graph;
    }

    public async Task<NetworkGraphDto> GetTopicCoOccurrenceNetworkAsync(int size)
    {
        var topTopics = await _context.PaperTopics
            .AsNoTracking()
            .Include(pt => pt.Topic)
                .ThenInclude(t => t.Subfield)
                    .ThenInclude(sf => sf.Field)
                        .ThenInclude(f => f.Domain)
            .Where(pt => pt.Topic != null)
            .GroupBy(pt => new
            {
                pt.TopicId,
                pt.Topic.TopicName,
                DomainName = pt.Topic.Subfield != null && pt.Topic.Subfield.Field != null && pt.Topic.Subfield.Field.Domain != null
                    ? pt.Topic.Subfield.Field.Domain.DomainName
                    : null
            })
            .Select(g => new
            {
                g.Key.TopicId,
                g.Key.TopicName,
                g.Key.DomainName,
                PaperCount = g.Count()
            })
            .OrderByDescending(x => x.PaperCount)
            .Take(size)
            .ToListAsync();

        if (!topTopics.Any())
            return new NetworkGraphDto();

        var topicIds = topTopics.Select(t => t.TopicId).ToHashSet();

        var paperTopicRows = await _context.PaperTopics
            .AsNoTracking()
            .Where(pt => topicIds.Contains(pt.TopicId))
            .Select(pt => new
            {
                pt.PaperId,
                pt.TopicId
            })
            .ToListAsync();

        var nodeMap = topTopics.ToDictionary(
            topic => topic.TopicId,
            topic => new GraphNode
            {
                Id = topic.TopicId.ToString(),
                Label = topic.TopicName,
                Size = topic.PaperCount,
                Group = string.IsNullOrWhiteSpace(topic.DomainName) ? "Research Topic" : topic.DomainName
            });

        var edgeMap = new Dictionary<string, GraphEdge>();

        foreach (var topicGroup in paperTopicRows.GroupBy(row => row.PaperId))
        {
            var topicsInPaper = topicGroup
                .Select(row => row.TopicId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            for (var i = 0; i < topicsInPaper.Count; i++)
            {
                for (var j = i + 1; j < topicsInPaper.Count; j++)
                {
                    var source = topicsInPaper[i].ToString();
                    var target = topicsInPaper[j].ToString();
                    var edgeKey = $"{source}-{target}";

                    if (!edgeMap.TryGetValue(edgeKey, out var edge))
                    {
                        edgeMap[edgeKey] = new GraphEdge
                        {
                            Source = source,
                            Target = target,
                            Weight = 1
                        };
                    }
                    else
                    {
                        edge.Weight += 1;
                    }
                }
            }
        }

        return new NetworkGraphDto
        {
            Nodes = nodeMap.Values.ToList(),
            Edges = edgeMap.Values
                .OrderByDescending(edge => edge.Weight)
                .ToList()
        };
    }

    #endregion

    #region Keyword & Topic Trends (EF Core)

    public async Task<KeywordTrendDto> GetKeywordTrendsAsync(string keyword, int years = 5)
    {
        var normalizedKeyword = keyword.Trim().ToLower();

        var exactKeyword = await _context.Keywords
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.NormalizedName == normalizedKeyword);

        var matchedKeywordsQuery = _context.Keywords
            .AsNoTracking()
            .Where(k => exactKeyword != null
                ? k.KeywordId == exactKeyword.KeywordId
                : k.NormalizedName.Contains(normalizedKeyword));

        var matchedKeywords = await matchedKeywordsQuery
            .Select(k => new { k.KeywordId, k.KeywordName })
            .ToListAsync();

        if (!matchedKeywords.Any())
        {
            return new KeywordTrendDto
            {
                Keyword = keyword,
                YearlyCounts = new List<YearlyCountDto>()
            };
        }

        var maxYear = await _context.Papers
            .Where(p => p.PublicationYear != null)
            .MaxAsync(p => (int?)p.PublicationYear) ?? DateTime.UtcNow.Year;

        var startYear = maxYear - years + 1;
        var keywordIds = matchedKeywords.Select(k => k.KeywordId).ToList();

        var papersWithKeyword = await _context.PaperKeywords
            .Include(pk => pk.Paper)
            .Where(pk => keywordIds.Contains(pk.KeywordId)
                      && pk.Paper.PublicationYear != null
                      && pk.Paper.PublicationYear >= startYear
                      && pk.Paper.PublicationYear <= maxYear)
            .Select(pk => new { pk.PaperId, PublicationYear = pk.Paper.PublicationYear!.Value })
            .Distinct()
            .ToListAsync();

        var yearlyCounts = papersWithKeyword
            .GroupBy(p => p.PublicationYear)
            .Select(g => new YearlyCountDto { Year = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Year, x => x.Count);

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
            Keyword = exactKeyword?.KeywordName ?? keyword,
            YearlyCounts = result
        };
    }

    public async Task<TopicTrendDto> GetTopicTrendsAsync(string topic, int years = 5)
    {
        var normalizedTopic = topic.Trim().ToLower();

        var topicEntity = await _context.ResearchTopics
            .FirstOrDefaultAsync(t => t.NormalizedName.Contains(normalizedTopic));

        if (topicEntity == null)
        {
            return new TopicTrendDto
            {
                TopicName = topic,
                YearlyCounts = new List<YearlyCountDto>()
            };
        }

        var maxYear = await _context.Papers
            .Where(p => p.PublicationYear != null)
            .MaxAsync(p => (int?)p.PublicationYear) ?? DateTime.UtcNow.Year;

        var startYear = maxYear - years + 1;

        var paperYears = await _context.PaperTopics
            .Include(pt => pt.Paper)
            .Where(pt => pt.TopicId == topicEntity.TopicId
                      && pt.Paper.PublicationYear != null
                      && pt.Paper.PublicationYear >= startYear
                      && pt.Paper.PublicationYear <= maxYear)
            .Select(pt => pt.Paper.PublicationYear!.Value)
            .ToListAsync();

        var yearlyCounts = paperYears
            .GroupBy(y => y)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = new List<YearlyCountDto>();
        for (int y = startYear; y <= maxYear; y++)
        {
            result.Add(new YearlyCountDto
            {
                Year = y,
                Count = yearlyCounts.TryGetValue(y, out var count) ? count : 0
            });
        }

        return new TopicTrendDto
        {
            TopicName = topicEntity.TopicName,
            YearlyCounts = result
        };
    }

    public async Task<List<AvailableTopicForCompareDto>> GetAvailableTopicsForCompareAsync(string search = "", int size = 300)
    {
        var normalizedSearch = search?.Trim().ToLower();

        var query = _context.PaperTopics
            .AsNoTracking()
            .Where(pt => pt.Paper.PublicationYear != null);

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(pt =>
                pt.Topic.NormalizedName.Contains(normalizedSearch) ||
                pt.Topic.TopicName.ToLower().Contains(normalizedSearch));
        }

        return await query
            .GroupBy(pt => new
            {
                pt.TopicId,
                pt.Topic.TopicName
            })
            .Select(g => new AvailableTopicForCompareDto
            {
                TopicId = g.Key.TopicId,
                TopicName = g.Key.TopicName,
                PaperCount = g.Select(pt => pt.PaperId).Distinct().Count(),
                FirstYear = g.Min(pt => pt.Paper.PublicationYear!.Value),
                LastYear = g.Max(pt => pt.Paper.PublicationYear!.Value)
            })
            .OrderByDescending(topic => topic.PaperCount)
            .ThenBy(topic => topic.TopicName)
            .Take(size)
            .ToListAsync();
    }

    public async Task<List<TopicComparisonDto>> CompareTopicsAsync(List<Guid> topicIds, int years = 5)
    {
        var distinctTopicIds = topicIds.Distinct().ToList();

        var topics = await _context.ResearchTopics
            .AsNoTracking()
            .Where(t => distinctTopicIds.Contains(t.TopicId))
            .Select(t => new
            {
                t.TopicId,
                t.TopicName
            })
            .ToListAsync();

        if (!topics.Any())
            return new List<TopicComparisonDto>();

        var maxYear = await _context.PaperTopics
            .Where(pt => distinctTopicIds.Contains(pt.TopicId) && pt.Paper.PublicationYear != null)
            .MaxAsync(pt => (int?)pt.Paper.PublicationYear) ?? DateTime.UtcNow.Year;

        var startYear = maxYear - years + 1;

        var rows = await _context.PaperTopics
            .AsNoTracking()
            .Where(pt => distinctTopicIds.Contains(pt.TopicId)
                         && pt.Paper.PublicationYear != null
                         && pt.Paper.PublicationYear >= startYear
                         && pt.Paper.PublicationYear <= maxYear)
            .Select(pt => new
            {
                pt.TopicId,
                pt.PaperId,
                PublicationYear = pt.Paper.PublicationYear!.Value,
                CitedByCount = pt.Paper.CitedByCount ?? 0,
                pt.Paper.JournalId
            })
            .ToListAsync();

        var results = new List<TopicComparisonDto>();

        foreach (var topic in topics)
        {
            var topicPapers = rows
                .Where(row => row.TopicId == topic.TopicId)
                .GroupBy(row => row.PaperId)
                .Select(g => g.First())
                .ToList();

            var yearlyCounts = topicPapers
                .GroupBy(row => row.PublicationYear)
                .ToDictionary(g => g.Key, g => g.Count());

            var yearlyResult = new List<YearlyCountDto>();
            for (var year = startYear; year <= maxYear; year++)
            {
                yearlyResult.Add(new YearlyCountDto
                {
                    Year = year,
                    Count = yearlyCounts.TryGetValue(year, out var count) ? count : 0
                });
            }

            var startCount = yearlyResult.FirstOrDefault()?.Count ?? 0;
            var endCount = yearlyResult.LastOrDefault()?.Count ?? 0;
            double growth = 0;
            if (startCount > 0)
            {
                growth = Math.Round((double)(endCount - startCount) / startCount * 100, 1);
            }
            else if (endCount > 0)
            {
                growth = 100.0;
            }

            results.Add(new TopicComparisonDto
            {
                TopicId = topic.TopicId,
                TopicName = topic.TopicName,
                PaperCount = topicPapers.Count,
                CitationCount = topicPapers.Sum(row => row.CitedByCount),
                JournalCount = topicPapers
                    .Where(row => row.JournalId.HasValue)
                    .Select(row => row.JournalId!.Value)
                    .Distinct()
                    .Count(),
                TopicHIndex = CalculateHIndex(topicPapers.Select(row => row.CitedByCount)),
                GrowthPercentage = growth,
                StartYear = startYear,
                EndYear = maxYear,
                YearlyCounts = yearlyResult
            });
        }

        return results
            .OrderByDescending(result => result.PaperCount)
            .ToList();
    }

    public async Task<IEnumerable<TrendingTopicDto>> GetTrendingTopicsAsync(int years = 1, int topCount = 10)
    {
        var yearlyTopicLinkCounts = await _context.PaperTopics
            .Where(pt => pt.Paper.PublicationYear != null)
            .GroupBy(pt => pt.Paper.PublicationYear!.Value)
            .Select(g => new
            {
                Year = g.Key,
                TopicLinkCount = g.Count()
            })
            .OrderByDescending(x => x.Year)
            .ToListAsync();

        if (!yearlyTopicLinkCounts.Any())
            return Enumerable.Empty<TrendingTopicDto>();

        var minimumTopicLinksForTrendYear = Math.Max(3, topCount);
        var currentYear = yearlyTopicLinkCounts
            .FirstOrDefault(x => x.TopicLinkCount >= minimumTopicLinksForTrendYear)?.Year
            ?? yearlyTopicLinkCounts.First().Year;

        var previousYear = currentYear - years;

        var topicCounts = await _context.PaperTopics
            .Where(pt => pt.Paper.PublicationYear == currentYear || pt.Paper.PublicationYear == previousYear)
            .GroupBy(pt => new { pt.TopicId, pt.Topic.TopicName })
            .Select(g => new
            {
                g.Key.TopicId,
                g.Key.TopicName,
                CurrentYearCount = g.Count(pt => pt.Paper.PublicationYear == currentYear),
                PreviousYearCount = g.Count(pt => pt.Paper.PublicationYear == previousYear)
            })
            .Where(x => x.CurrentYearCount > 0)
            .ToListAsync();

        var trendingTopics = new List<TrendingTopicDto>();

        foreach (var topic in topicCounts)
        {
            double growth = 0;
            string trend;

            if (topic.PreviousYearCount > 0)
            {
                growth = Math.Round((double)(topic.CurrentYearCount - topic.PreviousYearCount) / topic.PreviousYearCount * 100, 1);
            }
            else if (topic.CurrentYearCount > 0)
            {
                growth = 100.0;
            }

            if (growth > 5) trend = "up";
            else if (growth < -5) trend = "down";
            else trend = "stable";

            trendingTopics.Add(new TrendingTopicDto
            {
                TopicName = topic.TopicName,
                PaperCount = topic.CurrentYearCount,
                PreviousPaperCount = topic.PreviousYearCount,
                GrowthPercentage = growth,
                Trend = trend,
                CurrentYear = currentYear,
                PreviousYear = previousYear,
                Years = years
            });
        }

        return trendingTopics
            .OrderByDescending(t => t.GrowthPercentage)
            .ThenByDescending(t => t.PaperCount)
            .Take(topCount);
    }

    private static int CalculateHIndex(IEnumerable<int> citations)
    {
        var orderedCitations = citations
            .OrderByDescending(citationCount => citationCount)
            .ToList();

        var hIndex = 0;
        for (var i = 0; i < orderedCitations.Count; i++)
        {
            var rank = i + 1;
            if (orderedCitations[i] >= rank)
                hIndex = rank;
            else
                break;
        }

        return hIndex;
    }

    #endregion

    #region Researcher Dashboard (EF Core)

    public async Task<ResearcherDashboardDto> GetResearcherDashboardAsync(Guid userId)
    {
        var bookmarkedPapers = await _context.UserBookmarks
            .CountAsync(b => b.UserId == userId);

        var followedTopicIds = await _context.UserFollowingTopics
            .Where(f => f.UserId == userId)
            .Select(f => f.TopicId)
            .ToListAsync();

        var followedTopicsCount = followedTopicIds.Count;

        var maxYear = await _context.Papers
            .Where(p => p.PublicationYear != null)
            .MaxAsync(p => (int?)p.PublicationYear) ?? DateTime.UtcNow.Year;

        var newPapersInFollowedTopics = followedTopicIds.Count > 0
            ? await _context.PaperTopics
                .Include(pt => pt.Paper)
                .CountAsync(pt => followedTopicIds.Contains(pt.TopicId)
                               && pt.Paper.PublicationYear == maxYear)
            : 0;

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

    #endregion
}
