namespace N_Tier.Application.Models.OpenAlex;

public class SearchWorksRequestModel
{
    public string Keyword { get; set; }
    public string Journal { get; set; }
    public string Author { get; set; }
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 10;
}
