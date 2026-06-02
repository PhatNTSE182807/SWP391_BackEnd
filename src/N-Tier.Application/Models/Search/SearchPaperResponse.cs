using System;
using System.Collections.Generic;

namespace N_Tier.Application.Models.Search;

public class SearchPaperResponse
{
    public long Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public List<SearchPaperResultItem> Results { get; set; } = new();
}

public class SearchPaperResultItem
{
    public Guid PaperId { get; set; }
    public string Title { get; set; }
    public string Abstract { get; set; }
    public int? PublicationYear { get; set; }
    public int? CitedByCount { get; set; }
    public SearchHighlight Highlight { get; set; }
}

public class SearchHighlight
{
    public List<string> Title { get; set; } = new();
    public List<string> Abstract { get; set; } = new();
}
