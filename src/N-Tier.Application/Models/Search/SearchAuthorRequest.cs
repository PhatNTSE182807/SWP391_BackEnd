namespace N_Tier.Application.Models.Search;

public class SearchAuthorRequest
{
    public string Q { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public int? MinWorksCount { get; set; }
    public int? MaxWorksCount { get; set; }
    public int? MinCitedByCount { get; set; }
    public int? MaxCitedByCount { get; set; }
    public int? MinHIndex { get; set; }
    public int? MaxHIndex { get; set; }
}
