using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Microsoft.Extensions.Logging;
using N_Tier.Application.Models.Analytics;

namespace N_Tier.Application.Services.Impl;

public class AnalyticsService : IAnalyticsService
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly ILogger<AnalyticsService> _logger;
    private const string IndexName = "papers";

    public AnalyticsService(ElasticsearchClient elasticClient, ILogger<AnalyticsService> logger)
    {
        _elasticClient = elasticClient;
        _logger = logger;
    }

    #region Research Trends

    public async Task<List<ChartDataPoint>> GetPaperCountByYearAsync()
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

    public async Task<List<ChartDataPoint>> GetCitationsByYearAsync()
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

    public async Task<List<ChartDataPoint>> GetTopTopicsAsync(int size)
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

    public async Task<List<ChartDataPoint>> GetTopDomainsAsync(int size)
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

    public async Task<List<SeriesDataDto>> GetKeywordTrendOverTimeAsync(List<string> keywords)
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
                                        .Field("keywords.keywordName")
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

    #endregion

    #region Author Statistics

    public async Task<List<ChartDataPoint>> GetTopAuthorsByCitationsAsync(int size)
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
                                .Field("authors.displayName")
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

    public async Task<List<ChartDataPoint>> GetTopAuthorsByHIndexAsync(int size)
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
                                .Field("authors.displayName")
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

    public async Task<NetworkGraphDto> GetAuthorCollaborationNetworkAsync(int size)
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
                                .Field("authors.displayName")
                                .Size(size)
                            )
                            .Aggregations(subSub => subSub
                                .Add("nested_coauthors", nc => nc
                                    .Nested(n2 => n2.Path("authors"))
                                    .Aggregations(subSubSub => subSubSub
                                        .Add("coauthor_buckets", t2 => t2
                                            .Terms(terms2 => terms2
                                                .Field("authors.displayName")
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

    #endregion

    #region Journal Statistics

    public async Task<List<ChartDataPoint>> GetTopJournalsByPaperCountAsync(int size)
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
                                .Field("journal.journalName")
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

    public async Task<List<ChartDataPoint>> GetTopJournalsByCitationsAsync(int size)
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
                                .Field("journal.journalName")
                                .Size(size)
                                .Order(new[] { KeyValuePair.Create<Field, SortOrder>("sum_citations", SortOrder.Desc) })
                            )
                            .Aggregations(subSub => subSub
                                .Add("sum_citations", sumAgg => sumAgg.Sum(sum => sum.Field("citedByCount")))
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
                var sumVal = b.Aggregations.GetSum("sum_citations")?.Value ?? 0;
                return new ChartDataPoint
                {
                    Key = b.Key.ToString(),
                    Value = sumVal
                };
            })
            .ToList();
    }

    public async Task<List<ChartDataPoint>> GetOpenAccessRatioAsync()
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
        if (terms == null) return new List<ChartDataPoint>();

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

    #endregion

    #region Keyword Statistics

    public async Task<List<ChartDataPoint>> GetKeywordCloudAsync(int size)
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
                                .Field("keywords.keywordName")
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
                                        .Field("keywords.keywordName")
                                        .Size(100)
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
                                .Field("keywords.keywordName")
                                .Size(size)
                            )
                            .Aggregations(subSub => subSub
                                .Add("nested_cooccurrences", nc => nc
                                    .Nested(n2 => n2.Path("keywords"))
                                    .Aggregations(subSubSub => subSubSub
                                        .Add("co_buckets", t2 => t2
                                            .Terms(terms2 => terms2
                                                .Field("keywords.keywordName")
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

    #endregion
}
