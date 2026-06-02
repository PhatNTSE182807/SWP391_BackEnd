using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using N_Tier.Application.Models.OpenAlex;

namespace N_Tier.Application.Services.Impl;

public class OpenAlexService : IOpenAlexService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string BaseUrl = "https://api.openalex.org/works";

    public OpenAlexService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SearchWorksResponseModel> SearchWorksAsync(SearchWorksRequestModel request)
    {
        var client = _httpClientFactory.CreateClient();
        
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            filters.Add($"default.search:{Uri.EscapeDataString(request.Keyword)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Journal))
        {
            filters.Add($"primary_location.source.display_name.search:{Uri.EscapeDataString(request.Journal)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            filters.Add($"authorships.author.display_name.search:{Uri.EscapeDataString(request.Author)}");
        }

        var requestUrl = $"{BaseUrl}?page={request.Page}&per-page={request.PerPage}";

        if (filters.Any())
        {
            requestUrl += $"&filter={string.Join(",", filters)}";
        }

        var response = await client.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        var result = new SearchWorksResponseModel();

        if (root.TryGetProperty("meta", out var metaProp))
        {
            result.Meta = new MetaData
            {
                Count = metaProp.TryGetProperty("count", out var countProp) ? countProp.GetInt32() : 0,
                Page = metaProp.TryGetProperty("page", out var pageProp) ? pageProp.GetInt32() : request.Page,
                PerPage = metaProp.TryGetProperty("per_page", out var perPageProp) ? perPageProp.GetInt32() : request.PerPage
            };
        }
        else
        {
             result.Meta = new MetaData { Page = request.Page, PerPage = request.PerPage };
        }

        if (root.TryGetProperty("results", out var resultsProp) && resultsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in resultsProp.EnumerateArray())
            {
                var work = new WorkItemModel
                {
                    Id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : null,
                    Title = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null,
                    Doi = item.TryGetProperty("doi", out var doiProp) ? doiProp.GetString() : null,
                    PublicationYear = item.TryGetProperty("publication_year", out var pubYearProp) && pubYearProp.ValueKind == JsonValueKind.Number ? pubYearProp.GetInt32() : null,
                    PublicationDate = item.TryGetProperty("publication_date", out var pubDateProp) ? pubDateProp.GetString() : null
                };

                if (item.TryGetProperty("authorships", out var authorshipsProp) && authorshipsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var auth in authorshipsProp.EnumerateArray())
                    {
                        if (auth.TryGetProperty("author", out var authorObj) && authorObj.TryGetProperty("display_name", out var authorNameProp))
                        {
                            var authorName = authorNameProp.GetString();
                            if (!string.IsNullOrEmpty(authorName))
                            {
                                work.Authors.Add(authorName);
                            }
                        }
                    }
                }

                if (item.TryGetProperty("primary_location", out var locationProp) && locationProp.ValueKind == JsonValueKind.Object)
                {
                    if (locationProp.TryGetProperty("source", out var sourceProp) && sourceProp.ValueKind == JsonValueKind.Object)
                    {
                        if (sourceProp.TryGetProperty("display_name", out var sourceNameProp))
                        {
                            work.JournalName = sourceNameProp.GetString();
                        }
                    }
                }

                result.Results.Add(work);
            }
        }

        return result;
    }
}
