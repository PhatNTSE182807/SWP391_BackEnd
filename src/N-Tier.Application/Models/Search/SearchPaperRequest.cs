namespace N_Tier.Application.Models.Search;

public class SearchPaperRequest
{
    public string Q { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public int? From { get; set; }
    public int? To { get; set; }
    public string Language { get; set; }
    public bool? IsOpenAccess { get; set; }
}
