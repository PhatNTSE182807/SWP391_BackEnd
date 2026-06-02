using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using N_Tier.Application.Models.SemanticScholar;

namespace N_Tier.Application.Services.Impl;

public class SemanticScholarService : ISemanticScholarService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private const string BaseUrl = "https://api.semanticscholar.org/graph/v1/paper/search";

    public SemanticScholarService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<SemanticScholarSearchResponseModel> SearchWorksAsync(SemanticScholarSearchRequestModel request)
    {
        var client = _httpClientFactory.CreateClient();
        
        var apiKey = _configuration["SemanticScholar:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }
        
        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Keyword)) queryParts.Add(request.Keyword);
        if (!string.IsNullOrWhiteSpace(request.Author)) queryParts.Add(request.Author);
        if (!string.IsNullOrWhiteSpace(request.Journal)) queryParts.Add(request.Journal);

        string queryStr = queryParts.Any() ? string.Join(" ", queryParts) : "science"; // default to something if empty

        var queryParams = new List<string>
        {
            $"query={Uri.EscapeDataString(queryStr)}",
            "fields=title,authors,venue,year,externalIds",
            $"limit={request.PerPage}"
        };
        
        int offset = (request.Page - 1) * request.PerPage;
        queryParams.Add($"offset={offset}");

        var requestUrl = $"{BaseUrl}?{string.Join("&", queryParams)}";

        var response = await client.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        var result = new SemanticScholarSearchResponseModel();
        result.Meta = new MetaData { Page = request.Page, PerPage = request.PerPage };

        if (root.TryGetProperty("total", out var totalProp) && totalProp.ValueKind == JsonValueKind.Number)
        {
            result.Meta.Count = totalProp.GetInt32();
        }

        if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in dataProp.EnumerateArray())
            {
                var work = new WorkItemModel
                {
                    Id = item.TryGetProperty("paperId", out var idProp) ? idProp.GetString() : null,
                    Title = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null,
                    PublicationYear = item.TryGetProperty("year", out var yearProp) && yearProp.ValueKind == JsonValueKind.Number ? yearProp.GetInt32() : null,
                    JournalName = item.TryGetProperty("venue", out var venueProp) ? venueProp.GetString() : null
                };
                
                if (work.PublicationYear.HasValue)
                {
                    work.PublicationDate = work.PublicationYear.Value.ToString();
                }

                if (item.TryGetProperty("externalIds", out var extIdsProp) && extIdsProp.ValueKind == JsonValueKind.Object)
                {
                    if (extIdsProp.TryGetProperty("DOI", out var doiProp))
                    {
                        work.Doi = doiProp.GetString();
                    }
                }

                if (item.TryGetProperty("authors", out var authorsProp) && authorsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var auth in authorsProp.EnumerateArray())
                    {
                        if (auth.TryGetProperty("name", out var nameProp))
                        {
                            var name = nameProp.GetString();
                            if (!string.IsNullOrEmpty(name))
                            {
                                work.Authors.Add(name);
                            }
                        }
                    }
                }

                result.Results.Add(work);
            }
        }

        return result;
    }
}
