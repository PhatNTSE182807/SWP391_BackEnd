using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using N_Tier.Application.Models.Crossref;

namespace N_Tier.Application.Services.Impl;

public class CrossrefService : ICrossrefService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string BaseUrl = "https://api.crossref.org/works";

    public CrossrefService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CrossrefSearchResponseModel> SearchWorksAsync(CrossrefSearchRequestModel request)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "N-Tier-Architecture/1.0 (mailto:your_email@example.com)");
        
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            queryParams.Add($"query={Uri.EscapeDataString(request.Keyword)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Journal))
        {
            queryParams.Add($"query.container-title={Uri.EscapeDataString(request.Journal)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Author))
        {
            queryParams.Add($"query.author={Uri.EscapeDataString(request.Author)}");
        }

        int offset = (request.Page - 1) * request.PerPage;
        queryParams.Add($"rows={request.PerPage}");
        queryParams.Add($"offset={offset}");

        var requestUrl = $"{BaseUrl}?{string.Join("&", queryParams)}";

        var response = await client.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        var result = new CrossrefSearchResponseModel();
        result.Meta = new MetaData { Page = request.Page, PerPage = request.PerPage };

        if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.Object)
        {
            if (messageProp.TryGetProperty("total-results", out var totalResultsProp))
            {
                result.Meta.Count = totalResultsProp.GetInt32();
            }

            if (messageProp.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemsProp.EnumerateArray())
                {
                    var work = new WorkItemModel
                    {
                        Id = item.TryGetProperty("DOI", out var idProp) ? idProp.GetString() : null,
                        Doi = item.TryGetProperty("DOI", out var doiProp) ? doiProp.GetString() : null
                    };

                    if (item.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.Array && titleProp.GetArrayLength() > 0)
                    {
                        work.Title = titleProp[0].GetString();
                    }

                    if (item.TryGetProperty("container-title", out var containerTitleProp) && containerTitleProp.ValueKind == JsonValueKind.Array && containerTitleProp.GetArrayLength() > 0)
                    {
                        work.JournalName = containerTitleProp[0].GetString();
                    }

                    if (item.TryGetProperty("author", out var authorProp) && authorProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var auth in authorProp.EnumerateArray())
                        {
                            string given = auth.TryGetProperty("given", out var givenProp) ? givenProp.GetString() : "";
                            string family = auth.TryGetProperty("family", out var familyProp) ? familyProp.GetString() : "";
                            string fullName = $"{given} {family}".Trim();
                            if (!string.IsNullOrEmpty(fullName))
                            {
                                work.Authors.Add(fullName);
                            }
                        }
                    }

                    if (item.TryGetProperty("published", out var publishedProp) && publishedProp.ValueKind == JsonValueKind.Object)
                    {
                        if (publishedProp.TryGetProperty("date-parts", out var datePartsProp) && datePartsProp.ValueKind == JsonValueKind.Array && datePartsProp.GetArrayLength() > 0)
                        {
                            var parts = datePartsProp[0];
                            if (parts.ValueKind == JsonValueKind.Array && parts.GetArrayLength() > 0)
                            {
                                work.PublicationYear = parts[0].GetInt32();
                                work.PublicationDate = work.PublicationYear.ToString();
                                if (parts.GetArrayLength() > 1) work.PublicationDate += $"-{parts[1].GetInt32():D2}";
                                if (parts.GetArrayLength() > 2) work.PublicationDate += $"-{parts[2].GetInt32():D2}";
                            }
                        }
                    }

                    result.Results.Add(work);
                }
            }
        }

        return result;
    }
}
