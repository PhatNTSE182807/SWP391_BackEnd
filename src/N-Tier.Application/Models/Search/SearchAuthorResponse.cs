using System;
using System.Collections.Generic;

namespace N_Tier.Application.Models.Search;

public class SearchAuthorResponse
{
    public long Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public List<SearchAuthorResultItem> Results { get; set; } = new();
}

public class SearchAuthorResultItem
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
    public SearchAuthorHighlight Highlight { get; set; }
}

public class SearchAuthorHighlight
{
    public List<string> DisplayName { get; set; } = new();
    public List<string> FullName { get; set; } = new();
    public List<string> Affiliations { get; set; } = new();
}
