namespace N_Tier.Application.Models.Search;

public class SearchPaperResponseModel
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public List<SearchPaperResultModel> Results { get; set; } = new();
}

public class SearchPaperResultModel
{
    public Guid PaperId { get; set; }
    public string Title { get; set; }
    public string Abstract { get; set; }
    public int? PublicationYear { get; set; }
    public int? CitedByCount { get; set; }
    public SearchHighlightModel Highlight { get; set; }
}

public class SearchHighlightModel
{
    public List<string> Title { get; set; } = new();
    public List<string> Abstract { get; set; } = new();
}
