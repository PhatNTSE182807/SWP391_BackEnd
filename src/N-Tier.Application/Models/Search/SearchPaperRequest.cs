using System.Collections.Generic;

namespace N_Tier.Application.Models.Search;

public class SearchPaperRequest
{
    /// <summary>General search: title, author, journal, keyword</summary>
    public string Q { get; set; }

    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;

    // ── Date range filter ────────────────────────────────────────────────────
    public int? From { get; set; }
    public int? To { get; set; }

    // ── Other filters ────────────────────────────────────────────────────────
    public string Language { get; set; }
    public bool? IsOpenAccess { get; set; }

    // ── Facet filters (multi-select — user ticks multiple checkboxes then clicks Apply) ───
    /// <summary>
    /// Filter by journal name(s). Supports multiple values (OR logic).
    /// e.g. ?filterJournal=Nature+Methods&filterJournal=Science
    /// </summary>
    public List<string> FilterJournal { get; set; } = new();

    /// <summary>
    /// Filter by author display name(s). Supports multiple values (OR logic).
    /// e.g. ?filterAuthor=Yann+LeCun&filterAuthor=Geoffrey+Hinton
    /// </summary>
    public List<string> FilterAuthor { get; set; } = new();

    /// <summary>
    /// Filter by keyword(s). Supports multiple values (OR logic).
    /// e.g. ?filterKeyword=Deep+learning&filterKeyword=Neural+network
    /// </summary>
    public List<string> FilterKeyword { get; set; } = new();

    /// <summary>
    /// Filter by publication year(s). Supports multiple values (OR logic).
    /// e.g. ?filterYear=2021&filterYear=2022
    /// </summary>
    public List<int> FilterYear { get; set; } = new();

    /// <summary>
    /// Filter by research topic(s). Supports multiple values (OR logic).
    /// e.g. ?filterTopic=Machine+Learning&filterTopic=Deep+Learning
    /// </summary>
    public List<string> FilterTopic { get; set; } = new();
}

