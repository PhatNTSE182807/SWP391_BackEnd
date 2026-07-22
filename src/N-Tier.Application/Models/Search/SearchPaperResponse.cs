using System;
using System.Collections.Generic;

namespace N_Tier.Application.Models.Search;

public class SearchPaperResponse
{
    public long Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalPages { get; set; }
    public List<SearchPaperResultItem> Results { get; set; } = new();

    /// <summary>
    /// Facet counts for filter dropdowns (OpenAlex-style).
    /// Each facet lists the top values with their document counts
    /// matching the current query + active filters.
    /// </summary>
    public SearchFacets Facets { get; set; } = new();
}

public class SearchPaperResultItem
{
    public Guid PaperId { get; set; }
    public string Title { get; set; }
    public string Abstract { get; set; }
    public string Doi { get; set; }
    public int? PublicationYear { get; set; }
    public int? CitedByCount { get; set; }
    public string JournalName { get; set; }
    public List<string> Authors { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<string> Topics { get; set; } = new();
    public SearchHighlight Highlight { get; set; }
}

public class SearchHighlight
{
    public List<string> Title { get; set; } = new();
    // Abstract highlight removed: abstract is not a search field.
    // The Abstract field in SearchPaperResultItem still returns full abstract text for display.
}

/// <summary>Facet aggregation results — one list per filterable dimension.</summary>
public class SearchFacets
{
    /// <summary>Top publication years with counts (sorted by count desc).</summary>
    public List<FacetItem> Years { get; set; } = new();

    /// <summary>Top journals with counts.</summary>
    public List<FacetItem> Journals { get; set; } = new();

    /// <summary>Top authors with counts.</summary>
    public List<FacetItem> Authors { get; set; } = new();

    /// <summary>Top keywords with counts.</summary>
    public List<FacetItem> Keywords { get; set; } = new();

    /// <summary>Top topics with counts.</summary>
    public List<FacetItem> Topics { get; set; } = new();
}

/// <summary>A single facet option: a value and how many documents match it.</summary>
public class FacetItem
{
    public string Value { get; set; }
    public long Count { get; set; }
}

